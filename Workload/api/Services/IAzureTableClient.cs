using Azure.Data.Tables;

namespace TemplateWorkload.Services
{
    public interface IAzureTableClient
    {
        Task<TableClient> GetTableClientAsync(string tableName);
        Task<bool> TableExistsAsync(string tableName);
        Task CreateTableIfNotExistsAsync(string tableName);
    }
}
