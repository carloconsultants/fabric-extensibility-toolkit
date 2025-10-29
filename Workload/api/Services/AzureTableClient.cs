using Azure.Data.Tables;
using TemplateWorkload.Core;
using Microsoft.Extensions.Logging;

namespace TemplateWorkload.Services
{
    public class AzureTableClient : IAzureTableClient
    {
        private readonly TableServiceClient _tableServiceClient;
        private readonly ILogger<AzureTableClient> _logger;

        public AzureTableClient(ILogger<AzureTableClient> logger)
        {
            _logger = logger;
            var connectionString = StorageConfig.StorageAccount.ConnectionString;
            _tableServiceClient = new TableServiceClient(connectionString);
        }

        public async Task<TableClient> GetTableClientAsync(string tableName)
        {
            await CreateTableIfNotExistsAsync(tableName);
            return _tableServiceClient.GetTableClient(tableName);
        }

        public async Task<bool> TableExistsAsync(string tableName)
        {
            try
            {
                var tableClient = _tableServiceClient.GetTableClient(tableName);
                await tableClient.GetAccessPoliciesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogDebug($"Table {tableName} does not exist: {ex.Message}");
                return false;
            }
        }

        public async Task CreateTableIfNotExistsAsync(string tableName)
        {
            try
            {
                var tableClient = _tableServiceClient.GetTableClient(tableName);
                await tableClient.CreateIfNotExistsAsync();
                _logger.LogInformation($"Table {tableName} created or already exists");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to create table {tableName}: {ex.Message}");
                throw;
            }
        }
    }
}
