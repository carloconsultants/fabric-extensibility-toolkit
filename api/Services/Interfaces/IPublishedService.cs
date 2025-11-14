using PowerBITips.Api.Models.DTOs.Requests;
using PowerBITips.Api.Models.DTOs.Responses;

namespace PowerBITips.Api.Services.Interfaces;

/// <summary>
/// Service interface for managing published items
/// </summary>
public interface IPublishedService
{
    /// <summary>
    /// Gets published items with pagination support
    /// </summary>
    Task<PublishedItemsResponse> GetPublishedItemsAsync(GetPublishedItemsRequest request, string? continuationToken = null);

    /// <summary>
    /// Gets all published items from table without pagination
    /// </summary>
    Task<PublishedItemsListResponse> GetPublishedItemsFromTableAsync(GetPublishedItemsFromTableRequest request);

    /// <summary>
    /// Publishes a new item
    /// </summary>
    Task<PublishItemResponse> PublishItemAsync(PublishItemRequest request, string userId, string identityProvider);

    /// <summary>
    /// Deletes a published item (owner or admin only)
    /// </summary>
    Task<DeletePublishedItemResponse> DeletePublishedItemAsync(DeletePublishedItemRequest request, string userId, string identityProvider);
}