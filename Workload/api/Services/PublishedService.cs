using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PowerBITips.Api.Core.Interfaces;
using PowerBITips.Api.Core.Azure;
using PowerBITips.Api.Models.Constants;
using PowerBITips.Api.Models.DTOs.Requests;
using PowerBITips.Api.Models.DTOs.Responses;

using PowerBITips.Api.Models.Entities;
using PowerBITips.Api.Models.Enums;
using PowerBITips.Api.Services.Common;
using PowerBITips.Api.Services.Interfaces;
using System.Text.Json;

namespace PowerBITips.Api.Services;

public class PublishedService : IPublishedService
{
    private readonly IAzureTableStorage _tableStorage;
    private readonly IUserService _userService;
    private readonly ILogger<PublishedService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IBlobStorageClient _blobStorageClient;

    public PublishedService(
        IAzureTableStorage tableStorage,
        IUserService userService,
        ILogger<PublishedService> logger,
        IConfiguration configuration,
        IBlobStorageClient blobStorageClient)
    {
        _tableStorage = tableStorage;
        _userService = userService;
        _logger = logger;
        _configuration = configuration;
        _blobStorageClient = blobStorageClient;

        _logger.LogInformation("✅ PublishedService initialized - table name will be retrieved from PublishedConstants.TableName");
    }
    public async Task<PublishedItemsResponse> GetPublishedItemsAsync(GetPublishedItemsRequest request, string? continuationToken = null)
    {
        try
        {
            _logger.LogInformation("Getting published items with pagination. Page: {Page}, PageSize: {PageSize}, Search: '{Search}'",
                request.Page, request.PageSize, request.Search);

            _logger.LogInformation("Querying table: '{TableName}' for published items", PublishedConstants.TableName);

            var allItems = new List<PublishedEntity>();

            // Query each requested item type separately by partition key (like the working API)
            foreach (var itemType in request.ItemTypes)
            {
                if (Enum.TryParse<PublishedItemType>(itemType, true, out var parsedType))
                {
                    var partitionKey = parsedType switch
                    {
                        PublishedItemType.Theme => PublishedConstants.PartitionKeys.Themes,
                        PublishedItemType.Layout => PublishedConstants.PartitionKeys.Layouts,
                        PublishedItemType.Project => PublishedConstants.PartitionKeys.Projects,
                        PublishedItemType.Scrims => PublishedConstants.PartitionKeys.Scrims,
                        _ => PublishedConstants.PartitionKeys.Themes
                    };

                    _logger.LogInformation("Querying partition: '{PartitionKey}' for item type: '{ItemType}'", partitionKey, itemType);

                    // Build filter for this partition (partition key + search if provided)
                    var partitionFilter = $"PartitionKey eq '{partitionKey}'";
                    if (!string.IsNullOrEmpty(request.Search))
                    {
                        partitionFilter += $" and (contains(Name, '{request.Search}') or contains(OwnerId, '{request.Search}'))";
                    }

                    var partitionItems = await _tableStorage.GetEntitiesByFilterAsync<PublishedEntity>(PublishedConstants.TableName, partitionFilter);

                    allItems.AddRange(partitionItems);
                }
            }

            // If no item types specified, get all types
            if (request.ItemTypes.Length == 0)
            {
                var allPartitions = new[] { "theme", "layout", "project", "scrims" };

                foreach (var partitionKey in allPartitions)
                {
                    _logger.LogInformation("Querying partition: '{PartitionKey}' (no type filter specified)", partitionKey);

                    var partitionFilter = $"PartitionKey eq '{partitionKey}'";
                    if (!string.IsNullOrEmpty(request.Search))
                    {
                        partitionFilter += $" and (contains(Name, '{request.Search}') or contains(OwnerId, '{request.Search}'))";
                    }

                    var partitionItems = await _tableStorage.GetEntitiesByFilterAsync<PublishedEntity>(PublishedConstants.TableName, partitionFilter);

                    allItems.AddRange(partitionItems);
                }
            }

            _logger.LogInformation("Raw query returned {Count} items from table '{TableName}'", allItems.Count, PublishedConstants.TableName);

            // Apply sorting
            var sortBy = Enum.TryParse<PublishedSortBy>(request.SortBy, true, out var parsedSortBy)
                ? parsedSortBy
                : PublishedSortBy.CreatedDate;

            var sortOrder = Enum.TryParse<PublishedSortOrder>(request.SortOrder, true, out var parsedSortOrder)
                ? parsedSortOrder
                : PublishedSortOrder.Descending;

            var sortedItems = sortBy switch
            {
                PublishedSortBy.Name => sortOrder == PublishedSortOrder.Ascending
                    ? allItems.OrderBy(x => x.Name)
                    : allItems.OrderByDescending(x => x.Name),
                PublishedSortBy.Downloads => sortOrder == PublishedSortOrder.Ascending
                    ? allItems.OrderBy(x => x.Downloads)
                    : allItems.OrderByDescending(x => x.Downloads),
                PublishedSortBy.FavoriteCount => sortOrder == PublishedSortOrder.Ascending
                    ? allItems.OrderBy(x => x.FavoriteCount)
                    : allItems.OrderByDescending(x => x.FavoriteCount),
                _ => sortOrder == PublishedSortOrder.Ascending
                    ? allItems.OrderBy(x => x.CreatedDate)
                    : allItems.OrderByDescending(x => x.CreatedDate)
            };

            // Apply pagination
            var totalCount = sortedItems.Count();
            var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);
            var startIndex = (request.Page - 1) * request.PageSize;

            var paginatedItems = sortedItems
                .Skip(startIndex)
                .Take(request.PageSize)
                .ToList();

            // Convert to response models
            var responseItems = new List<PublishedItemResponse>();
            foreach (var item in paginatedItems)
            {
                var responseItem = await MapToResponseAsync(item);
                responseItems.Add(responseItem);
            }

            var response = new PublishedItemsResponse
            {
                Items = responseItems,
                TotalCount = totalCount,
                PageSize = request.PageSize,
                PageNumber = request.Page
            };

            _logger.LogInformation("Successfully retrieved {ItemCount} published items", responseItems.Count);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting published items");
            throw;
        }
    }

    public async Task<PublishedItemsListResponse> GetPublishedItemsFromTableAsync(GetPublishedItemsFromTableRequest request)
    {
        try
        {
            _logger.LogInformation("Getting all published items from table");
            _logger.LogInformation("Querying table: '{TableName}' for all published items", PublishedConstants.TableName);

            var allItems = new List<PublishedEntity>();

            // Query each requested item type separately by partition key (like the working API)
            if (request.ItemTypes.Length > 0)
            {
                foreach (var itemType in request.ItemTypes)
                {
                    if (Enum.TryParse<PublishedItemType>(itemType, true, out var parsedType))
                    {
                        var partitionKey = parsedType switch
                        {
                            PublishedItemType.Theme => PublishedConstants.PartitionKeys.Themes,
                            PublishedItemType.Layout => PublishedConstants.PartitionKeys.Layouts,
                            PublishedItemType.Project => PublishedConstants.PartitionKeys.Projects,
                            PublishedItemType.Scrims => PublishedConstants.PartitionKeys.Scrims,
                            _ => PublishedConstants.PartitionKeys.Themes
                        };

                        _logger.LogInformation("Querying partition: '{PartitionKey}' for item type: '{ItemType}'", partitionKey, itemType);

                        var partitionFilter = $"PartitionKey eq '{partitionKey}'";
                        var partitionItems = await _tableStorage.GetEntitiesByFilterAsync<PublishedEntity>(PublishedConstants.TableName, partitionFilter);
                        allItems.AddRange(partitionItems);
                    }
                }
            }
            else
            {
                // If no item types specified, get all types
                var allPartitions = new[] {
                    PublishedConstants.PartitionKeys.Themes,
                    PublishedConstants.PartitionKeys.Layouts,
                    PublishedConstants.PartitionKeys.Projects,
                    PublishedConstants.PartitionKeys.Scrims
                };

                foreach (var partitionKey in allPartitions)
                {
                    _logger.LogInformation("Querying partition: '{PartitionKey}' (no type filter specified)", partitionKey);
                    var partitionFilter = $"PartitionKey eq '{partitionKey}'";
                    var partitionItems = await _tableStorage.GetEntitiesByFilterAsync<PublishedEntity>(PublishedConstants.TableName, partitionFilter);
                    allItems.AddRange(partitionItems);
                }
            }

            var items = allItems;

            _logger.LogInformation("Raw query returned {Count} items from table '{TableName}'", items.Count, PublishedConstants.TableName);

            // Convert to response models
            var responseItems = new List<PublishedItemResponse>();
            foreach (var item in items)
            {
                var responseItem = await MapToResponseAsync(item);
                responseItems.Add(responseItem);
            }

            var response = new PublishedItemsListResponse
            {
                Items = responseItems,
                TotalCount = responseItems.Count
            };

            _logger.LogInformation("Successfully retrieved {ItemCount} published items from table", responseItems.Count);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting published items from table");
            throw;
        }
    }

    public async Task<PublishItemResponse> PublishItemAsync(PublishItemRequest request, string userId, string identityProvider)
    {
        try
        {
            _logger.LogInformation("Publishing item: {ItemName} of type {PublishType} for user {UserId}",
                request.ItemName, request.PublishType, userId);

            // Determine partition key based on item type
            var partitionKey = request.PublishType switch
            {
                PublishedItemType.Theme => PublishedConstants.PartitionKeys.Themes,
                PublishedItemType.Layout => PublishedConstants.PartitionKeys.Layouts,
                PublishedItemType.Project => PublishedConstants.PartitionKeys.Projects,
                PublishedItemType.Scrims => PublishedConstants.PartitionKeys.Scrims,
                _ => PublishedConstants.PartitionKeys.Themes
            };

            // Always create new entity - matching original API behavior (no updates)
            var publishedGuid = request.ItemId; // Use ItemId as GUID like original API

            var publishedEntity = new PublishedEntity
            {
                PartitionKey = partitionKey,
                RowKey = publishedGuid,
                PublishedGuid = publishedGuid,
                Name = request.ItemName,
                Downloads = 0,
                FavoriteCount = 0,
                OwnerId = userId,
                OwnerIdentityProvider = identityProvider,
                CreatedDate = (double)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                UpdatedDate = (double)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            // Set optional properties based on request
            if (!string.IsNullOrEmpty(request.PreviewImage))
            {
                publishedEntity.PreviewImage = request.PreviewImage;
            }

            if (request.PublishType == PublishedItemType.Layout && request.Layout != null)
            {
                publishedEntity.Layout = System.Text.Json.JsonSerializer.Serialize(request.Layout);
            }

            // Set ItemLink to match original API behavior - stores blob reference
            publishedEntity.ItemLink = $"{{\"container\":\"published\",\"id\":\"{publishedGuid}\"}}";

            // Save to table using table storage interface
            await _tableStorage.CreateEntityAsync(PublishedConstants.TableName, publishedEntity);

            _logger.LogInformation("Successfully published new item with GUID: {PublishedGuid}", publishedGuid);

            return new PublishItemResponse
            {
                Success = true,
                Message = "Item published successfully",
                PublishedGuid = publishedGuid,
                ItemName = request.ItemName,
                Type = request.PublishType,
                PublishedAt = DateTime.UtcNow,
                ItemLink = string.Empty,
                PreviewImageLink = request.PreviewImage ?? string.Empty,
                IsNewPublication = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing item: {ItemName}", request.ItemName);
            return new PublishItemResponse
            {
                Success = false,
                Message = $"Error publishing item: {ex.Message}",
                ItemName = request.ItemName,
                Type = request.PublishType
            };
        }
    }

    public async Task<DeletePublishedItemResponse> DeletePublishedItemAsync(DeletePublishedItemRequest request, string userId, string identityProvider)
    {
        try
        {
            _logger.LogInformation("Deleting published item: {ItemId} of type {ItemType} for user {UserId}",
                request.ItemId, request.ItemType, userId);

            // Determine partition key based on item type
            var partitionKey = request.ItemType switch
            {
                PublishedItemType.Theme => PublishedConstants.PartitionKeys.Themes,
                PublishedItemType.Layout => PublishedConstants.PartitionKeys.Layouts,
                PublishedItemType.Project => PublishedConstants.PartitionKeys.Projects,
                PublishedItemType.Scrims => PublishedConstants.PartitionKeys.Scrims,
                _ => PublishedConstants.PartitionKeys.Themes
            };

            // Get the item to verify ownership
            var existingEntity = await _tableStorage.GetEntityAsync<PublishedEntity>(
                PublishedConstants.TableName,
                partitionKey,
                request.ItemId);

            if (existingEntity == null)
            {
                return new DeletePublishedItemResponse
                {
                    Success = false,
                    Message = "Published item not found"
                };
            }

            // Check ownership - user can only delete their own items (unless admin)
            var isOwner = existingEntity.OwnerId == userId && existingEntity.OwnerIdentityProvider == identityProvider;

            // For now, allow deletion by owner (admin check can be added later if needed)
            if (!isOwner)
            {
                return new DeletePublishedItemResponse
                {
                    Success = false,
                    Message = "You can only delete your own published items"
                };
            }

            // Delete the entity
            await _tableStorage.DeleteEntityAsync(PublishedConstants.TableName, partitionKey, request.ItemId);

            _logger.LogInformation("Successfully deleted published item: {ItemId}", request.ItemId);

            return new DeletePublishedItemResponse
            {
                Success = true,
                Message = "Item deleted successfully",
                PublishedGuid = request.ItemId,
                ItemName = existingEntity.Name ?? string.Empty,
                Type = request.ItemType,
                DeletedAt = DateTime.UtcNow,
                DeletedBy = userId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting published item: {ItemId}", request.ItemId);
            return new DeletePublishedItemResponse
            {
                Success = false,
                Message = $"Error deleting item: {ex.Message}",
                Type = request.ItemType
            };
        }
    }

    private async Task<PublishedItemResponse> MapToResponseAsync(PublishedEntity entity)
    {
        object? layout = null;
        if (!string.IsNullOrEmpty(entity.Layout))
        {
            try
            {
                layout = JsonSerializer.Deserialize<object>(entity.Layout);
            }
            catch
            {
                // If deserialization fails, keep as null
                layout = null;
            }
        }

        // Fetch preview image from blob storage
        // Match old Node.js API behavior: always try to fetch using PublishedGuid (rowKey) directly
        // Skip for layout items (like the old API does)
        PBIPageImageResponse? previewImage = null;
        var itemType = GetTypeFromPartitionKey(entity.PartitionKey);
        if (itemType != PublishedItemType.Layout)
        {
            try
            {
                previewImage = await GetPreviewImageAsync(entity.PublishedGuid);
                if (previewImage != null)
                {
                    _logger.LogDebug("Successfully retrieved preview image for {PublishedGuid}: Name={Name}, HasBase64={HasBase64}", 
                        entity.PublishedGuid, previewImage.Name, !string.IsNullOrEmpty(previewImage.Base64));
                }
                else
                {
                    _logger.LogDebug("Preview image is null for published item: {PublishedGuid}", entity.PublishedGuid);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch preview image for published item: {PublishedGuid}", entity.PublishedGuid);
            }
        }

        return new PublishedItemResponse
        {
            PublishedGuid = entity.PublishedGuid,
            Type = itemType,
            Name = entity.Name,
            Downloads = entity.Downloads,
            FavoriteCount = entity.FavoriteCount,
            OwnerId = entity.OwnerId,
            OwnerIdentityProvider = entity.OwnerIdentityProvider,
            CreatedDate = ConvertUnixTimestampToDateTime(entity.CreatedDate),
            UpdatedDate = ConvertUnixTimestampToDateTime(entity.UpdatedDate),
            ItemLink = entity.ItemLink,
            PreviewImage = previewImage,
            Layout = layout
        };
    }

    /// <summary>
    /// Converts Unix timestamp (milliseconds) to DateTime
    /// </summary>
    private static DateTime ConvertUnixTimestampToDateTime(double? unixTimestamp)
    {
        if (!unixTimestamp.HasValue || unixTimestamp.Value == 0)
            return DateTime.UtcNow;

        // Convert from Unix timestamp (assuming milliseconds)
        var timestampLong = (long)unixTimestamp.Value;
        var dateTime = DateTimeOffset.FromUnixTimeMilliseconds(timestampLong).DateTime;
        return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
    }

    private static PublishedItemType GetTypeFromPartitionKey(string partitionKey)
    {
        return partitionKey switch
        {
            PublishedConstants.PartitionKeys.Themes => PublishedItemType.Theme,
            PublishedConstants.PartitionKeys.Layouts => PublishedItemType.Layout,
            PublishedConstants.PartitionKeys.Projects => PublishedItemType.Project,
            PublishedConstants.PartitionKeys.Scrims => PublishedItemType.Scrims,
            _ => PublishedItemType.Theme
        };
    }

    /// <summary>
    /// Gets preview image from blob storage using the published GUID directly (matching old Node.js API behavior)
    /// </summary>
    private async Task<PBIPageImageResponse?> GetPreviewImageAsync(string publishedGuid)
    {
        try
        {
            // Match old Node.js API: buildPublishImagesFileName returns `${publishedFolder}/${id}.json`
            // Uses publishedImagesContainer and publishedFolder from environment variables
            // Note: Old code used publishedFolder (STORAGE_PUBLISHED_FOLDER), not publishedImagesFolder
            var folder = AzureConstants.PublishedFolder;
            var container = AzureConstants.PublishedImagesContainer;
            var filename = $"{folder}/{publishedGuid}.json";

            _logger.LogDebug("Fetching preview image: container={Container}, filename={Filename}", container, filename);

            // Get the JSON blob content
            var jsonContent = await _blobStorageClient.GetJsonBlobContent(filename, container);
            if (string.IsNullOrEmpty(jsonContent))
            {
                _logger.LogDebug("Preview image not found for published item: {PublishedGuid}", publishedGuid);
                return null;
            }

            _logger.LogDebug("Retrieved JSON content for {PublishedGuid}, length: {Length}, preview: {Preview}", 
                publishedGuid, jsonContent.Length, jsonContent.Length > 200 ? jsonContent.Substring(0, 200) : jsonContent);

            // Match old Node.js behavior: parse JSON, but handle empty/invalid JSON gracefully
            // Old code does: JSON.parse(previewImageString || '{}')
            // So if empty, it returns an empty object, not null
            try
            {
                // Parse the PBIPageImage from JSON
                var imageData = JsonSerializer.Deserialize<PBIPageImageResponse>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                if (imageData == null)
                {
                    _logger.LogWarning("Deserialization returned null for published item: {PublishedGuid}. JSON content: {JsonContent}", 
                        publishedGuid, jsonContent);
                    // Return empty object to match old behavior (old code returns {} for empty string)
                    return new PBIPageImageResponse();
                }

                _logger.LogDebug("Successfully deserialized preview image for {PublishedGuid}: Name={Name}, Base64Length={Base64Length}, Width={Width}, Height={Height}", 
                    publishedGuid, imageData.Name, imageData.Base64?.Length ?? 0, imageData.Width, imageData.Height);
                return imageData;
            }
            catch (JsonException jsonEx)
            {
                _logger.LogWarning(jsonEx, "Failed to deserialize preview image JSON for published item: {PublishedGuid}. JSON content: {JsonContent}", 
                    publishedGuid, jsonContent);
                // Return empty object to match old behavior
                return new PBIPageImageResponse();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching preview image for published item: {PublishedGuid}", publishedGuid);
            return null;
        }
    }
}