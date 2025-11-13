using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using PowerBITips.Api.Services.Common;
using PowerBITips.Api.Utilities.Helpers;
using Microsoft.Extensions.Logging;

namespace PowerBITips.Api.Core.Azure
{
    public interface IBlobStorageClient
    {
        Task<string> UploadLogoBlob(string image, string filename, string container);
        Task<string> GetJsonBlob(string filename, string container);
        Task<string> UploadImage(string image, string filename, string container);
        Task<ServiceResponse<string>> DeleteImage(string filename, string container);
        Task<string> GetLicenseData();
        Task<ServiceResponse<bool>> RecordLicenseUpdate(string updateDetails);
        Task CreateTableEntityAsync<T>(string tableName, T entity) where T : class, ITableEntity;
        Task<string?> GetJsonBlobContent(string filename, string container);
    }
    public class BlobStorageClient : IBlobStorageClient
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly TableServiceClient _tableServiceClient;
        private readonly ILogger<BlobStorageClient> _logger;

        public BlobStorageClient(ILogger<BlobStorageClient> logger)
        {
            _logger = logger;

            // Get the connection string from environment or config
            var connectionString = Environment.GetEnvironmentVariable("WEBSITE_CONTENTAZUREFILECONNECTIONSTRING");
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = Environment.GetEnvironmentVariable("STORAGE_CONNECTION_STRING");
                if (string.IsNullOrEmpty(connectionString))
                {
                    _logger.LogError("Storage connection string is not configured.");
                    throw new ArgumentNullException(nameof(connectionString), "Storage connection string not configured. Please set STORAGE_CONNECTION_STRING in local.settings.json.");
                }
            }

            // Initialize BlobServiceClient with connection string
            _logger.LogInformation("Initializing BlobServiceClient with connection string");
            _blobServiceClient = new BlobServiceClient(connectionString);

            // Initialize TableServiceClient with connection string  
            _logger.LogInformation("Initializing TableServiceClient with connection string");
            _tableServiceClient = new TableServiceClient(connectionString);
        }

        public async Task<string> UploadLogoBlob(string image, string filename, string container)
        {
            _logger.LogInformation("Uploading logo to blob storage, container: {container}", container);
            var containerClient = _blobServiceClient.GetBlobContainerClient(container);

            // Ensure container exists
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            var regex = new Regex(@"^data:([A-Za-z-+/]+);base64,(.+)$");
            var match = regex.Match(image);
            var base64Data = match.Groups[2].Value;
            var buffer = Convert.FromBase64String(base64Data);

            var blobBlockOptions = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = match.Groups[1].Value
                }
            };

            var blockBlobClient = containerClient.GetBlobClient(filename);
            _logger.LogInformation("Uploading logo to blob storage, filename: {filename}", filename);
            using (var stream = new MemoryStream(buffer))
            {
                await blockBlobClient.UploadAsync(stream, blobBlockOptions);
            }

            return blockBlobClient.Uri.ToString();
        }

        public async Task<string> GetJsonBlob(string filename, string container)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(container);
            var blockBlobClient = containerClient.GetBlobClient(filename);

            if (await blockBlobClient.ExistsAsync())
            {
                return blockBlobClient.Uri.ToString();
            }

            return string.Empty;
        }

        public async Task<string> UploadImage(string image, string filename, string container)
        {
            return await UploadLogoBlob(image, filename, container);
        }

        public async Task<ServiceResponse<string>> DeleteImage(string filename, string container)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(container);
                var blockBlobClient = containerClient.GetBlobClient(filename);

                await blockBlobClient.DeleteAsync();

                return ServiceResponse<string>.Success("Successfully deleted report group image");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to delete image {filename}: {ex}");
                throw new Exception($"Could not delete image: {ex.Message}");
            }
        }

        public async Task<string> GetLicenseData()
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(AzureConstants.LicenseContainer);
            var blockBlobClient = containerClient.GetBlobClient("license.json");

            if (blockBlobClient.Exists())
            {
                var downloadResult = await blockBlobClient.DownloadContentAsync();
                return downloadResult.Value.Content.ToString();
            }
            else
            {
                throw new Exception("License file not found.");
            }
        }

        public async Task<ServiceResponse<bool>> RecordLicenseUpdate(string licenseSku)
        {
            if (string.IsNullOrWhiteSpace(licenseSku))
            {
                return ServiceResponse<bool>.Error(HttpStatusCode.BadRequest, "License SKU cannot be empty");
            }

            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(AzureConstants.LicenseContainer);
                var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                var filename = $"updates/{timestamp}_license_update.json";

                var update = new LicenseUpdate
                {
                    LicenseSku = licenseSku,
                    UpdatedAt = DateTime.UtcNow
                };

                var jsonContent = JsonSerializer.Serialize(update, new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                var content = new BinaryData(jsonContent);

                var blockBlobClient = containerClient.GetBlobClient(filename);
                await blockBlobClient.UploadAsync(content, new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders
                    {
                        ContentType = "application/json"
                    }
                });

                return ServiceResponse<bool>.Success(true);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to record license update: {ex}");
                return ServiceResponse<bool>.Error(HttpStatusCode.InternalServerError, "Failed to record license update");
            }
        }

        public async Task CreateTableEntityAsync<T>(string tableName, T entity) where T : class, ITableEntity
        {
            if (string.IsNullOrEmpty(tableName))
            {
                _logger.LogError("Table name cannot be empty.");
                throw new ArgumentNullException(nameof(tableName), "Table name cannot be empty.");
            }

            if (entity == null)
            {
                _logger.LogError("Entity cannot be null.");
                throw new ArgumentNullException(nameof(entity), "Entity cannot be null.");
            }

            try
            {
                _logger.LogInformation("Creating entity in table: {tableName}", tableName);
                var tableClient = _tableServiceClient.GetTableClient(tableName);
                await tableClient.CreateIfNotExistsAsync();
                await tableClient.AddEntityAsync(entity);
                _logger.LogInformation("Successfully created entity in table: {tableName}", tableName);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to create entity in table {tableName}: {error}", tableName, ex.Message);
                throw new Exception($"Failed to create entity in table {tableName}: {ex.Message}", ex);
            }
        }

        public async Task<string?> GetJsonBlobContent(string filename, string container)
        {
            try
            {
                _logger.LogInformation("Getting JSON blob content from container: {container}, filename: {filename}", container, filename);
                var containerClient = _blobServiceClient.GetBlobContainerClient(container);
                var blobClient = containerClient.GetBlobClient(filename);

                if (await blobClient.ExistsAsync())
                {
                    var downloadResult = await blobClient.DownloadContentAsync();
                    var content = downloadResult.Value.Content.ToString();
                    _logger.LogInformation("Successfully retrieved JSON blob content, length: {length}", content?.Length ?? 0);
                    return content;
                }

                _logger.LogWarning("Blob not found: {filename} in container: {container}", filename, container);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving JSON blob content from container: {container}, filename: {filename}", container, filename);
                return null;
            }
        }
    }

    internal class LicenseUpdate
    {
        public required string LicenseSku { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}