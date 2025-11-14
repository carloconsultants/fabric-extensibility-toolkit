using PowerBITips.Api.Models.DTOs.Requests;
using PowerBITips.Api.Models.DTOs.Responses;
using PowerBITips.Api.Models.Entities;
using PowerBITips.Api.Models.Enums;

namespace PowerBITips.Api.Models.DTOs.Extensions;

/// <summary>
/// Extension methods for mapping between UserTheme entities and DTOs
/// </summary>
public static class ThemeMappingExtensions
{
    /// <summary>
    /// Maps a UserTheme entity to ThemeResponse DTO
    /// </summary>
    public static ThemeResponse ToDto(this ThemeEntity theme, bool includeMetadata = false)
    {
        return new ThemeResponse
        {
            ThemeId = theme.ThemeId,
            ThemeName = theme.ThemeName,
            Type = theme.Type,
            Created = theme.Created,
            DownloadCount = theme.DownloadCount,
            UserId = theme.UserId,
            IsCommunity = theme.IsCommunity,
            ScrimsFamilyName = theme.ScrimsFamilyName,
            Url = theme.Url,
            ProjectUrl = theme.ProjectUrl,
            ProjectImagesUrl = theme.ProjectImagesUrl
        };
    }

    /// <summary>
    /// Maps a SaveThemeRequest to UserTheme entity
    /// </summary>
    public static ThemeEntity ToEntity(this SaveThemeRequest request, string userId)
    {
        return new ThemeEntity
        {
            PartitionKey = userId,
            RowKey = request.Id,
            UserId = userId,
            ThemeName = request.ThemeName ?? string.Empty,
            Type = DetermineThemeType(request.Payload),
            IsCommunity = request.IsCommunity ?? false,
            ScrimsFamilyName = request.ScrimsFamilyName ?? string.Empty,
            Created = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Maps a collection of UserTheme entities to UserThemesResponse DTO
    /// </summary>
    public static UserThemesResponse ToDto(this IEnumerable<ThemeEntity> themes, int totalCount, int pageNumber, int pageSize, bool includeStats = false)
    {
        var themesList = themes.ToList();
        
        return new UserThemesResponse
        {
            Items = themesList.Select(t => t.ToDto()).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            ThemeTypeDistribution = themesList.GroupBy(t => t.Type)
                .ToDictionary(g => g.Key, g => g.Count()),
            CommunityThemesCount = themesList.Count(t => t.IsCommunity == true),
            PrivateThemesCount = themesList.Count(t => t.IsCommunity == false),
            LastThemeCreated = themesList.Any() ? themesList.Max(t => t.Created) : DateTime.UtcNow,
            UserThemeStats = includeStats ? CalculateUserThemeStats(themesList) : null,
            Success = true,
            Message = "User themes retrieved successfully",
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Maps a UserTheme to ThemeDetailResponse with full payload data
    /// </summary>
    public static ThemeDetailResponse ToDetailDto(this ThemeEntity theme, object? themePayload = null, object? projectPayload = null, string[]? images = null)
    {
        return new ThemeDetailResponse
        {
            Theme = theme.ToDto(includeMetadata: true),
            ThemeData = themePayload,
            ProjectData = projectPayload,
            Images = images,
            UsageStats = CreateThemeUsageStats(theme),
            DataRetrievedAt = DateTime.UtcNow,
            Success = true,
            Message = "Theme details retrieved successfully",
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a SaveThemeResponse from UserTheme entity
    /// </summary>
    public static SaveThemeResponse ToSaveResponse(this ThemeEntity theme, bool isNew = false)
    {
        return new SaveThemeResponse
        {
            ThemeId = theme.ThemeId,
            ThemeName = theme.ThemeName,
            SavedAt = theme.Timestamp?.DateTime ?? theme.Created,
            Url = theme.Url,
            ProjectUrl = theme.ProjectUrl,
            IsNewTheme = isNew,
            Success = true,
            Message = isNew ? "Theme created successfully" : "Theme updated successfully",
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a DeleteThemeResponse from theme information
    /// </summary>
    public static DeleteThemeResponse ToDeleteResponse(this ThemeEntity theme, string deletedBy)
    {
        return new DeleteThemeResponse
        {
            ThemeId = theme.ThemeId,
            ThemeName = theme.ThemeName,
            DeletedAt = DateTime.UtcNow,
            DeletedBy = deletedBy,
            Success = true,
            Message = "Theme deleted successfully",
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Determines theme type based on payload content
    /// </summary>
    private static ThemeType DetermineThemeType(string? payload)
    {
        // Logic to determine theme type based on payload structure
        // This would need to be implemented based on your business logic
        return ThemeType.Theme; // Default
    }

    /// <summary>
    /// Creates metadata dictionary for theme
    /// </summary>
    private static Dictionary<string, object> CreateThemeMetadata(ThemeEntity theme)
    {
        return new Dictionary<string, object>
        {
            ["hasProject"] = !string.IsNullOrEmpty(theme.ProjectUrl),
            ["hasImages"] = !string.IsNullOrEmpty(theme.ProjectImagesUrl),
            ["hasScrims"] = !string.IsNullOrEmpty(theme.ScrimsFamilyName),
            ["lastModified"] = theme.Timestamp?.DateTime ?? theme.Created,
            ["popularity"] = CalculatePopularityScore(theme)
        };
    }

    /// <summary>
    /// Calculates popularity score for theme
    /// </summary>
    private static double CalculatePopularityScore(ThemeEntity theme)
    {
        var downloads = theme.DownloadCount;
        var ageInDays = (DateTime.UtcNow - theme.Created).Days + 1;
        
        return downloads * 3.0 / ageInDays;
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
    /// Creates theme usage statistics
    /// </summary>
    private static ThemeUsageStats CreateThemeUsageStats(ThemeEntity theme)
    {
        return new ThemeUsageStats
        {
            TotalDownloads = theme.DownloadCount,
            DownloadsByDay = new Dictionary<string, int>(), // Would be populated from analytics data
            MostActiveRegion = "Unknown", // Would be determined from analytics data
            StatsGeneratedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Calculates comprehensive user theme statistics
    /// </summary>
    private static ThemeStatsDto CalculateUserThemeStats(List<ThemeEntity> themes)
    {
        var mostPopular = themes.OrderByDescending(t => t.DownloadCount).FirstOrDefault();
        var latest = themes.OrderByDescending(t => t.Created).FirstOrDefault();

        return new ThemeStatsDto
        {
            TotalThemes = themes.Count,
            TotalDownloads = themes.Sum(t => t.DownloadCount),
            MostPopularTheme = mostPopular?.ToDto(),
            LatestTheme = latest?.ToDto(),
            StatsCalculatedAt = DateTime.UtcNow
        };
    }
}