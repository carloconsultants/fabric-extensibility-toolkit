using Azure;
using Azure.Data.Tables;

namespace PowerBITips.Api.Core.Interfaces;

/// <summary>
/// Abstraction over Azure Table Storage operations used by services.
/// Keep minimal surface required by current services to ease future replacement/testing.
/// </summary>
public interface IAzureTableStorage
{
    Task<T?> GetEntityAsync<T>(string tableName, string partitionKey, string rowKey) where T : class, ITableEntity, new();
    Task<IReadOnlyList<T>> GetEntitiesByPartitionAsync<T>(string tableName, string partitionKey) where T : class, ITableEntity, new();
    Task<IReadOnlyList<T>> GetEntitiesByFilterAsync<T>(string tableName, string filter) where T : class, ITableEntity, new();
    Task CreateEntityAsync<T>(string tableName, T entity) where T : class, ITableEntity, new();
    Task UpdateEntityAsync<T>(string tableName, T entity) where T : class, ITableEntity, new();
    Task DeleteEntityAsync(string tableName, string partitionKey, string rowKey);
}
