using Azure.Storage.Blobs;
using TemplateWorkload.Core;
using Microsoft.Extensions.Logging;

namespace TemplateWorkload.Services
{
    public class BlobStorageClient : IBlobStorageClient
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly ILogger<BlobStorageClient> _logger;

        public BlobStorageClient(ILogger<BlobStorageClient> logger)
        {
            _logger = logger;
            var connectionString = StorageConfig.StorageAccount.ConnectionString;
            _blobServiceClient = new BlobServiceClient(connectionString);
        }

        public async Task<BlobContainerClient> GetContainerClientAsync(string containerName)
        {
            await CreateContainerIfNotExistsAsync(containerName);
            return _blobServiceClient.GetBlobContainerClient(containerName);
        }

        public async Task<bool> ContainerExistsAsync(string containerName)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                await containerClient.GetPropertiesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogDebug($"Container {containerName} does not exist: {ex.Message}");
                return false;
            }
        }

        public async Task CreateContainerIfNotExistsAsync(string containerName)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                await containerClient.CreateIfNotExistsAsync();
                _logger.LogInformation($"Container {containerName} created or already exists");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to create container {containerName}: {ex.Message}");
                throw;
            }
        }
    }
}
