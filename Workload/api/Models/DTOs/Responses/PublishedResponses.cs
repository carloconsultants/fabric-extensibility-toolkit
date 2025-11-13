using PowerBITips.Api.Models.Enums;
using PowerBITips.Api.Models.DTOs.Common;

namespace PowerBITips.Api.Models.DTOs.Responses;

/// <summary>
/// Response model for a published item
/// </summary>
public class PublishedItemResponse : BaseResponseDto
{
    public string PublishedGuid { get; set; } = string.Empty;
    public PublishedItemType Type { get; set; } = PublishedItemType.Theme;
    public string Name { get; set; } = string.Empty;
    public int Downloads { get; set; } = 0;
    public int FavoriteCount { get; set; } = 0;
    public string OwnerId { get; set; } = string.Empty;
    public string OwnerIdentityProvider { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }

    // Extended properties
    public string? ItemLink { get; set; }
    public PBIPageImageResponse? PreviewImage { get; set; }
    public object? Layout { get; set; } // For layout items
    public string? Description { get; set; }
    public string? Category { get; set; }
    public string[]? Tags { get; set; }
    public bool IsPublic { get; set; } = true;
    public string? Version { get; set; }
    public int ViewCount { get; set; }
    public int LikeCount { get; set; }
    public double? Rating { get; set; }
    public int RatingCount { get; set; }
    public string FileSize { get; set; } = string.Empty;
    public Dictionary<string, object>? Metadata { get; set; }
    public PublishedItemStats? Stats { get; set; }
}

/// <summary>
/// Paginated response for published items
/// </summary>
public class PublishedItemsResponse : PaginatedResponseDto<PublishedItemResponse>
{
    public Dictionary<PublishedItemType, int>? ItemTypeDistribution { get; set; }
    public Dictionary<string, int>? CategoryDistribution { get; set; }
    public Dictionary<string, int>? TagDistribution { get; set; }
    public PublishedItemResponse? MostPopularItem { get; set; }
    public PublishedItemResponse? LatestItem { get; set; }
    public DateTime? LastPublishedAt { get; set; }
}

/// <summary>
/// Simple list response without pagination
/// </summary>
public class PublishedItemsListResponse : BaseResponseDto
{
    public List<PublishedItemResponse> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public DateTime RetrievedAt { get; set; } = DateTime.UtcNow;
    public Dictionary<PublishedItemType, int>? TypeCounts { get; set; }
}

/// <summary>
/// Response for publish operations
/// </summary>
public class PublishItemResponse : SuccessResponseDto
{
    public string PublishedGuid { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public PublishedItemType Type { get; set; }
    public DateTime PublishedAt { get; set; } = DateTime.UtcNow;
    public string ItemLink { get; set; } = string.Empty;
    public string PreviewImageLink { get; set; } = string.Empty;
    public bool IsNewPublication { get; set; }
}

/// <summary>
/// Response for delete operations
/// </summary>
public class DeletePublishedItemResponse : SuccessResponseDto
{
    public string PublishedGuid { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public PublishedItemType Type { get; set; }
    public DateTime DeletedAt { get; set; } = DateTime.UtcNow;
    public string DeletedBy { get; set; } = string.Empty;
    public string? DeletionReason { get; set; }
}

/// <summary>
/// Response for updating published items
/// </summary>
public class UpdatePublishedItemResponse : SuccessResponseDto
{
    public string PublishedGuid { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object>? ChangedFields { get; set; }
}

/// <summary>
/// Response containing published layouts
/// </summary>
public class PublishedLayoutsResponse : PaginatedResponseDto<PublishedItemResponse>
{
    public Dictionary<string, int>? LayoutTypeDistribution { get; set; }
    public Dictionary<string, int>? ComplexityDistribution { get; set; }
    public PublishedItemResponse? MostDownloadedLayout { get; set; }
}

/// <summary>
/// Statistics for a published item
/// </summary>
public class PublishedItemStats
{
    public int TotalDownloads { get; set; }
    public int TotalViews { get; set; }
    public int TotalLikes { get; set; }
    public int TotalFavorites { get; set; }
    public Dictionary<string, int> DownloadsByDay { get; set; } = new();
    public Dictionary<string, int> ViewsByDay { get; set; } = new();
    public Dictionary<string, int> DownloadsByRegion { get; set; } = new();
    public string? MostActiveRegion { get; set; }
    public DateTime StatsGeneratedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Response containing published item statistics and metrics
/// </summary>
public class PublishedStatsResponse : BaseResponseDto
{
    public int TotalPublishedItems { get; set; }
    public int TotalDownloads { get; set; }
    public int TotalViews { get; set; }
    public int TotalFavorites { get; set; }
    public Dictionary<PublishedItemType, int> ItemsByType { get; set; } = new();
    public Dictionary<string, int> ItemsByCategory { get; set; } = new();
    public Dictionary<string, int> DownloadsByDay { get; set; } = new();
    public List<PublishedItemResponse> TopItems { get; set; } = new();
    public List<PublishedItemResponse> RecentItems { get; set; } = new();
    public DateTime StatsGeneratedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Response for generic published item operations (create, update, delete, etc.)
/// </summary>
public class PublishedOperationResponse : SuccessResponseDto
{
    /// <summary>
    /// GUID of the published item (when applicable)
    /// </summary>
    public string? PublishedGuid { get; set; }
}