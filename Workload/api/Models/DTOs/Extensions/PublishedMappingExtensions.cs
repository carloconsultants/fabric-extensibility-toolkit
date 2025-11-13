using PowerBITips.Api.Models.DTOs.Requests;
using PowerBITips.Api.Models.DTOs.Responses;
using PowerBITips.Api.Models.Entities;
using PowerBITips.Api.Models.Enums;
using PowerBITips.Api.Models.Constants;

namespace PowerBITips.Api.Models.DTOs.Extensions;

/// <summary>
/// Extension methods for mapping between PublishedEntity and DTOs
/// </summary>
public static class PublishedMappingExtensions
{
    /// <summary>
    /// Helper method to get PublishedItemType from partition key
    /// </summary>
    private static PublishedItemType GetTypeFromPartitionKey(string partitionKey)
    {
        return partitionKey switch
        {
            PublishedConstants.PartitionKeys.Themes => PublishedItemType.Theme,
            PublishedConstants.PartitionKeys.Layouts => PublishedItemType.Layout,
            PublishedConstants.PartitionKeys.Projects => PublishedItemType.Project,
            PublishedConstants.PartitionKeys.Scrims => PublishedItemType.Scrims,
            _ => PublishedItemType.Theme // Default fallback
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

    /// <summary>
    /// Converts DateTime to Unix timestamp (milliseconds) as double
    /// </summary>
    private static double ConvertDateTimeToUnixTimestamp(DateTime dateTime)
    {
        var utcDateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
        return new DateTimeOffset(utcDateTime).ToUnixTimeMilliseconds();
    }
    /// <summary>
    /// Maps a PublishedEntity to PublishedItemResponse DTO
    /// Note: PreviewImage is set to null here because it must be fetched asynchronously from blob storage.
    /// The actual preview image fetching is handled in PublishedService.MapToResponseAsync()
    /// </summary>
    public static PublishedItemResponse ToDto(this PublishedEntity item, bool includeStats = false, bool includeMetadata = false)
    {
        return new PublishedItemResponse
        {
            PublishedGuid = item.PublishedGuid,
            Type = GetTypeFromPartitionKey(item.PartitionKey),
            Name = item.Name,
            Downloads = item.Downloads,
            FavoriteCount = item.FavoriteCount,
            OwnerId = item.OwnerId,
            OwnerIdentityProvider = item.OwnerIdentityProvider,
            CreatedDate = ConvertUnixTimestampToDateTime(item.CreatedDate),
            UpdatedDate = ConvertUnixTimestampToDateTime(item.UpdatedDate),
            ItemLink = item.ItemLink,
            PreviewImage = null, // Preview images must be fetched asynchronously from blob storage in the service layer
            Layout = (object?)item.Layout,
            IsPublic = !item.Restricted,
            Metadata = includeMetadata ? CreateItemMetadata(item) : null,
            Stats = includeStats ? CreateItemStats(item) : null,
            Success = true,
            Message = "Published item retrieved successfully",
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Maps a PublishItemRequest to PublishedUserItem entity
    /// </summary>
    public static PublishedEntity ToEntity(this PublishItemRequest request, string ownerId, string ownerIdentityProvider)
    {
        return new PublishedEntity
        {
            PartitionKey = request.PublishType.ToString(),
            RowKey = request.ItemId,
            PublishedGuid = request.ItemId,
            Name = request.ItemName,
            OwnerId = ownerId,
            OwnerIdentityProvider = ownerIdentityProvider,
            UserThemeId = request.UserThemeId ?? string.Empty,
            PreviewImage = request.PreviewImage ?? string.Empty,
            Layout = request.Layout?.ToString() ?? string.Empty,
            ItemLink = string.Empty,
            Restricted = !request.IsPublic,
            CreatedDate = ConvertDateTimeToUnixTimestamp(DateTime.UtcNow),
            UpdatedDate = ConvertDateTimeToUnixTimestamp(DateTime.UtcNow),
            Downloads = 0,
            FavoriteCount = 0
        };
    }

    /// <summary>
    /// Maps a collection of PublishedUserItem entities to PublishedItemsResponse DTO
    /// </summary>
    public static PublishedItemsResponse ToDto(this IEnumerable<PublishedEntity> items, int totalCount, int pageNumber, int pageSize, bool includeStats = false)
    {
        var itemsList = items.ToList();
        var mostPopular = itemsList.OrderByDescending(i => i.Downloads).FirstOrDefault();
        var latest = itemsList.OrderByDescending(i => ConvertUnixTimestampToDateTime(i.CreatedDate)).FirstOrDefault();

        return new PublishedItemsResponse
        {
            Items = itemsList.Select(i => i.ToDto(includeStats)).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            Success = true,
            Message = "Published items retrieved successfully",
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Maps a collection to PublishedItemsListResponse (without pagination)
    /// </summary>
    public static PublishedItemsListResponse ToListDto(this IEnumerable<PublishedEntity> items)
    {
        var itemsList = items.ToList();

        return new PublishedItemsListResponse
        {
            Items = itemsList.Select(i => i.ToDto()).ToList(),
            TotalCount = itemsList.Count,
            RetrievedAt = DateTime.UtcNow,
            TypeCounts = itemsList.GroupBy(i => GetTypeFromPartitionKey(i.PartitionKey))
                .ToDictionary(g => g.Key, g => g.Count()),
            Success = true,
            Message = "Published items list retrieved successfully",
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a PublishItemResponse from PublishedUserItem entity
    /// </summary>
    public static PublishItemResponse ToPublishResponse(this PublishedEntity item, bool isNew = false)
    {
        return new PublishItemResponse
        {
            PublishedGuid = item.PublishedGuid,
            ItemName = item.Name,
            Type = GetTypeFromPartitionKey(item.PartitionKey),
            PublishedAt = ConvertUnixTimestampToDateTime(item.CreatedDate),
            ItemLink = item.ItemLink,
            PreviewImageLink = item.PreviewImage,
            IsNewPublication = isNew,
            Success = true,
            Message = isNew ? "Item published successfully" : "Item updated successfully",
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a DeletePublishedItemResponse from item information
    /// </summary>
    public static DeletePublishedItemResponse ToDeleteResponse(this PublishedEntity item, string deletedBy, string? reason = null)
    {
        return new DeletePublishedItemResponse
        {
            PublishedGuid = item.PublishedGuid,
            ItemName = item.Name,
            Type = GetTypeFromPartitionKey(item.PartitionKey),
            DeletedAt = DateTime.UtcNow,
            DeletedBy = deletedBy,
            DeletionReason = reason,
            Success = true,
            Message = "Published item deleted successfully",
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates an UpdatePublishedItemResponse from update information
    /// </summary>
    public static UpdatePublishedItemResponse ToUpdateResponse(this PublishedEntity item, Dictionary<string, object>? changedFields = null)
    {
        return new UpdatePublishedItemResponse
        {
            PublishedGuid = item.PublishedGuid,
            ItemName = item.Name,
            UpdatedAt = DateTime.UtcNow,
            ChangedFields = changedFields,
            Success = true,
            Message = "Published item updated successfully",
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Maps layouts to PublishedLayoutsResponse
    /// </summary>
    public static PublishedLayoutsResponse ToLayoutsResponse(this IEnumerable<PublishedEntity> layouts, int totalCount, int pageNumber, int pageSize)
    {
        var layoutsList = layouts.Where(l => GetTypeFromPartitionKey(l.PartitionKey) == PublishedItemType.Layout).ToList();
        var mostDownloaded = layoutsList.OrderByDescending(l => l.Downloads).FirstOrDefault();

        return new PublishedLayoutsResponse
        {
            Items = layoutsList.Select(l => l.ToDto()).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            Success = true,
            Message = "Published layouts retrieved successfully",
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates comprehensive published statistics
    /// </summary>
    public static PublishedStatsResponse ToStatsResponse(this IEnumerable<PublishedEntity> items)
    {
        var itemsList = items.ToList();
        var topItems = itemsList.OrderByDescending(i => i.Downloads).Take(10).ToList();
        var recentItems = itemsList.OrderByDescending(i => ConvertUnixTimestampToDateTime(i.CreatedDate)).Take(10).ToList();

        return new PublishedStatsResponse
        {
            TotalPublishedItems = itemsList.Count,
            TotalDownloads = itemsList.Sum(i => i.Downloads),
            TotalFavorites = itemsList.Sum(i => i.FavoriteCount),
            ItemsByType = itemsList.GroupBy(i => GetTypeFromPartitionKey(i.PartitionKey))
                .ToDictionary(g => g.Key, g => g.Count()),
            TopItems = topItems.Select(i => i.ToDto()).ToList(),
            RecentItems = recentItems.Select(i => i.ToDto()).ToList(),
            StatsGeneratedAt = DateTime.UtcNow,
            Success = true,
            Message = "Published statistics retrieved successfully",
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Formats file size in bytes to human readable format
    /// </summary>
    private static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }

    /// <summary>
    /// Creates metadata dictionary for published item
    /// </summary>
    private static Dictionary<string, object> CreateItemMetadata(PublishedEntity item)
    {
        return new Dictionary<string, object>
        {
            ["hasPreviewImage"] = !string.IsNullOrEmpty(item.PreviewImage),
            ["hasLayout"] = !string.IsNullOrEmpty(item.Layout),
            ["popularity"] = CalculatePopularityScore(item),
            ["engagement"] = CalculateEngagementScore(item),
            ["publishedDaysAgo"] = (DateTime.UtcNow - ConvertUnixTimestampToDateTime(item.CreatedDate)).Days
        };
    }

    /// <summary>
    /// Creates statistics for published item
    /// </summary>
    private static PublishedItemStats CreateItemStats(PublishedEntity item)
    {
        return new PublishedItemStats
        {
            TotalDownloads = item.Downloads,
            TotalFavorites = item.FavoriteCount,
            DownloadsByDay = new Dictionary<string, int>(), // Would be populated from analytics
            DownloadsByRegion = new Dictionary<string, int>(), // Would be populated from analytics
            MostActiveRegion = "Unknown", // Would be determined from analytics
            StatsGeneratedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Calculates popularity score for published item
    /// </summary>
    private static double CalculatePopularityScore(PublishedEntity item)
    {
        var downloads = item.Downloads;
        var favorites = item.FavoriteCount;
        var ageInDays = (DateTime.UtcNow - ConvertUnixTimestampToDateTime(item.CreatedDate)).Days + 1;

        return (downloads * 5 + favorites * 4) / (double)ageInDays;
    }

    /// <summary>
    /// Calculates engagement score for published item
    /// </summary>
    private static double CalculateEngagementScore(PublishedEntity item)
    {
        var downloads = item.Downloads;
        var favorites = item.FavoriteCount;

        // Simple engagement calculation based on available metrics
        return downloads + favorites;
    }

    /// <summary>
    /// Calculates complexity distribution for layouts
    /// </summary>
    private static Dictionary<string, int> CalculateComplexityDistribution(List<PublishedEntity> layouts)
    {
        return new Dictionary<string, int>
        {
            ["Simple"] = layouts.Count(l => EstimateComplexity(l) == "Simple"),
            ["Medium"] = layouts.Count(l => EstimateComplexity(l) == "Medium"),
            ["Complex"] = layouts.Count(l => EstimateComplexity(l) == "Complex")
        };
    }

    /// <summary>
    /// Estimates layout complexity based on available metadata
    /// </summary>
    private static string EstimateComplexity(PublishedEntity layout)
    {
        // For now, return a simple estimate based on layout content length
        var layoutLength = layout.Layout?.Length ?? 0;
        return layoutLength switch
        {
            < 1000 => "Simple",
            < 5000 => "Medium",
            _ => "Complex"
        };
    }

    /// <summary>
    /// Calculates downloads by day for the last 30 days
    /// </summary>
    private static Dictionary<string, int> CalculateDownloadsByDay(List<PublishedEntity> items)
    {
        var result = new Dictionary<string, int>();
        for (int i = 29; i >= 0; i--)
        {
            var date = DateTime.UtcNow.AddDays(-i).ToString("yyyy-MM-dd");
            result[date] = 0; // Would be populated from actual analytics data
        }
        return result;
    }
}