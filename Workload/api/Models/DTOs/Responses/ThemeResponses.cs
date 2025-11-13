using PowerBITips.Api.Models.Enums;
using PowerBITips.Api.Models.DTOs.Common;

namespace PowerBITips.Api.Models.DTOs.Responses;

/// <summary>
/// Response containing theme information
/// </summary>
public class ThemeResponse : BaseResponseDto
{
    public string ThemeId { get; set; } = string.Empty;
    public string ThemeName { get; set; } = string.Empty;
    public ThemeType Type { get; set; }
    public DateTime Created { get; set; }
    public int DownloadCount { get; set; }
    public string UserId { get; set; } = string.Empty;
    public bool IsCommunity { get; set; }
    public string ScrimsFamilyName { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string ProjectUrl { get; set; } = string.Empty;
    public string ProjectImagesUrl { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public string[]? Tags { get; set; }
    public DateTime? LastModified { get; set; }
    public int ViewCount { get; set; }
    public int LikeCount { get; set; }
    public bool IsPublic { get; set; } = true;
    public string FileSize { get; set; } = string.Empty;
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Response containing detailed theme information with payload data
/// </summary>
public class ThemeDetailResponse : BaseResponseDto
{
    public ThemeResponse? Theme { get; set; }
    public object? ThemeData { get; set; }
    public object? ProjectData { get; set; }
    public string[]? Images { get; set; }
    public ThemeUsageStats? UsageStats { get; set; }
    public DateTime DataRetrievedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Response containing a paginated list of user themes
/// </summary>
public class UserThemesResponse : PaginatedResponseDto<ThemeResponse>
{
    public Dictionary<ThemeType, int>? ThemeTypeDistribution { get; set; }
    public Dictionary<string, int>? CategoryDistribution { get; set; }
    public int CommunityThemesCount { get; set; }
    public int PrivateThemesCount { get; set; }
    public DateTime? LastThemeCreated { get; set; }
    public ThemeStatsDto? UserThemeStats { get; set; }
}

/// <summary>
/// Response for theme save operations
/// </summary>
public class SaveThemeResponse : SuccessResponseDto
{
    public string ThemeId { get; set; } = string.Empty;
    public string ThemeName { get; set; } = string.Empty;
    public DateTime SavedAt { get; set; } = DateTime.UtcNow;
    public string Url { get; set; } = string.Empty;
    public string ProjectUrl { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public bool IsNewTheme { get; set; }
}

/// <summary>
/// Response for theme deletion operations
/// </summary>
public class DeleteThemeResponse : SuccessResponseDto
{
    public string ThemeId { get; set; } = string.Empty;
    public string ThemeName { get; set; } = string.Empty;
    public DateTime DeletedAt { get; set; } = DateTime.UtcNow;
    public string DeletedBy { get; set; } = string.Empty;
}

/// <summary>
/// Response containing theme usage statistics
/// </summary>
public class ThemeUsageStats
{
    public int TotalDownloads { get; set; }
    public int TotalViews { get; set; }
    public int TotalLikes { get; set; }
    public Dictionary<string, int> DownloadsByDay { get; set; } = new();
    public Dictionary<string, int> ViewsByDay { get; set; } = new();
    public string? MostActiveRegion { get; set; }
    public DateTime StatsGeneratedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// DTO for user theme statistics
/// </summary>
public class ThemeStatsDto
{
    public int TotalThemes { get; set; }
    public int TotalDownloads { get; set; }
    public int TotalViews { get; set; }
    public int TotalLikes { get; set; }
    public ThemeResponse? MostPopularTheme { get; set; }
    public ThemeResponse? LatestTheme { get; set; }
    public DateTime StatsCalculatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Response containing raw theme JSON strings to avoid serialization issues
/// </summary>
public class RawThemeResponse
{
    public string ThemeJson { get; set; } = string.Empty;
    public string ProjectJson { get; set; } = string.Empty;
    public string ImagesJson { get; set; } = string.Empty;
    public string ThemeName { get; set; } = string.Empty;
}