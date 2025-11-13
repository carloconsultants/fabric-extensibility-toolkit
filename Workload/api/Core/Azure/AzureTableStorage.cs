using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PowerBITips.Api.Core.Interfaces;

namespace PowerBITips.Api.Core.Azure;

/// <summary>
/// Minimal Azure Table Storage client implementing IAzureTableStorage.
/// Provides basic CRUD/query functionality used by domain services.
/// </summary>
public class AzureTableStorage : IAzureTableStorage
{
    private readonly TableServiceClient _serviceClient;
    private readonly ILogger<AzureTableStorage> _logger;

    public AzureTableStorage(IConfiguration configuration, ILogger<AzureTableStorage> logger)
    {
        _logger = logger;

        // Try multiple sources for the connection string
        var connectionString = configuration.GetConnectionString("AzureWebJobsStorage")
            ?? Environment.GetEnvironmentVariable("AzureWebJobsStorage")
            ?? configuration["AzureWebJobsStorage"]
            ?? configuration.GetConnectionString("STORAGE_CONNECTION_STRING")
            ?? Environment.GetEnvironmentVariable("STORAGE_CONNECTION_STRING")
            ?? configuration["STORAGE_CONNECTION_STRING"];

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            _logger.LogError("CRITICAL: AzureWebJobsStorage connection string not found in any configuration source. Table operations will fail.");
            _logger.LogError("   Checked: GetConnectionString('AzureWebJobsStorage'), Environment['AzureWebJobsStorage'], Configuration['AzureWebJobsStorage']");
            _logger.LogError("   Also checked: STORAGE_CONNECTION_STRING as fallback");
            _logger.LogWarning("Falling back to UseDevelopmentStorage=true (Azurite emulator)");
            connectionString = "UseDevelopmentStorage=true"; // fallback to Azurite
        }
        else
        {
            // Log that we found it (but don't log the actual connection string for security)
            var source = configuration.GetConnectionString("AzureWebJobsStorage") != null ? "ConnectionStrings:AzureWebJobsStorage"
                : Environment.GetEnvironmentVariable("AzureWebJobsStorage") != null ? "Environment:AzureWebJobsStorage"
                : configuration["AzureWebJobsStorage"] != null ? "Configuration:AzureWebJobsStorage"
                : configuration.GetConnectionString("STORAGE_CONNECTION_STRING") != null ? "ConnectionStrings:STORAGE_CONNECTION_STRING"
                : Environment.GetEnvironmentVariable("STORAGE_CONNECTION_STRING") != null ? "Environment:STORAGE_CONNECTION_STRING"
                : "Configuration:STORAGE_CONNECTION_STRING";

            _logger.LogInformation("AzureTableStorage connection string found from {Source}", source);
            _logger.LogDebug("   Connection string length: {Length} characters", connectionString.Length);

            // Extract account name for debugging
            var accountMatch = System.Text.RegularExpressions.Regex.Match(connectionString, @"AccountName=([^;]+)");
            if (accountMatch.Success)
            {
                _logger.LogInformation("   Using Azure Storage Account: {AccountName}", accountMatch.Groups[1].Value);
            }
        }

        try
        {
            _serviceClient = new TableServiceClient(connectionString);
            _logger.LogInformation("TableServiceClient initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize TableServiceClient");
            throw;
        }
    }

    private TableClient GetTableClient(string tableName)
    {
        var client = _serviceClient.GetTableClient(tableName);
        _logger.LogDebug("Got table client for '{TableName}'", tableName);
        return client;
    }

    public async Task<T?> GetEntityAsync<T>(string tableName, string partitionKey, string rowKey) where T : class, ITableEntity, new()
    {
        try
        {
            var client = GetTableClient(tableName);
            var response = await client.GetEntityAsync<T>(partitionKey, rowKey);
            return response.Value;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogDebug("Entity not found {PartitionKey}/{RowKey} in {Table}", partitionKey, rowKey, tableName);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting entity {PartitionKey}/{RowKey} from {Table}", partitionKey, rowKey, tableName);
            return null;
        }
    }

    public async Task<IReadOnlyList<T>> GetEntitiesByPartitionAsync<T>(string tableName, string partitionKey) where T : class, ITableEntity, new()
    {
        try
        {
            var client = GetTableClient(tableName);
            var results = client.QueryAsync<T>(e => e.PartitionKey == partitionKey);
            var list = new List<T>();
            await foreach (var entity in results) list.Add(entity);
            return list;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying partition {PartitionKey} from {Table}", partitionKey, tableName);
            return Array.Empty<T>();
        }
    }

    public async Task<IReadOnlyList<T>> GetEntitiesByFilterAsync<T>(string tableName, string filter) where T : class, ITableEntity, new()
    {
        try
        {
            var client = GetTableClient(tableName);
            _logger.LogInformation("Querying table '{TableName}' with filter: '{Filter}' (empty means all entities)", tableName, filter);

            var query = string.IsNullOrWhiteSpace(filter) ? client.QueryAsync<T>() : client.QueryAsync<T>(filter);
            var list = new List<T>();
            await foreach (var entity in query)
            {
                list.Add(entity);
                _logger.LogDebug("Found entity: {PartitionKey}/{RowKey}", entity.PartitionKey, entity.RowKey);
            }

            _logger.LogInformation("Table '{TableName}' query completed. Found {Count} entities with filter '{Filter}'", tableName, list.Count, filter);
            return list;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying with filter '{Filter}' from {Table}", filter, tableName);
            return Array.Empty<T>();
        }
    }
    public async Task CreateEntityAsync<T>(string tableName, T entity) where T : class, ITableEntity, new()
    {
        try
        {
            var client = GetTableClient(tableName);
            await client.AddEntityAsync(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating entity {PartitionKey}/{RowKey} in {Table}", entity.PartitionKey, entity.RowKey, tableName);
            throw;
        }
    }

    public async Task UpdateEntityAsync<T>(string tableName, T entity) where T : class, ITableEntity, new()
    {
        try
        {
            var client = GetTableClient(tableName);
            await client.UpsertEntityAsync(entity, TableUpdateMode.Replace);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating entity {PartitionKey}/{RowKey} in {Table}", entity.PartitionKey, entity.RowKey, tableName);
            throw;
        }
    }

    public async Task DeleteEntityAsync(string tableName, string partitionKey, string rowKey)
    {
        try
        {
            var client = GetTableClient(tableName);
            await client.DeleteEntityAsync(partitionKey, rowKey);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogDebug("Entity already absent {PartitionKey}/{RowKey} in {Table}", partitionKey, rowKey, tableName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting entity {PartitionKey}/{RowKey} in {Table}", partitionKey, rowKey, tableName);
            throw;
        }
    }
}
