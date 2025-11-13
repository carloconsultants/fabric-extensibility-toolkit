namespace PowerBITips.Api.Models.DTOs.Responses;

public class SharedResourceResponse
{
    public string ResourceId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<string> Tags { get; set; } = new();
    public long SizeInBytes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastModified { get; set; }
    public string? ThumbnailUrl { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

public class SharedResourcesListResponse
{
    public List<SharedResourceResponse> Resources { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageSize { get; set; }
    public int CurrentPage { get; set; }
    public bool HasNextPage { get; set; }
    public Dictionary<string, int>? CategoryCounts { get; set; }
}

public class DownloadDataResponse
{
    public string DownloadId { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public int RecordCount { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddHours(24);
    public string? FileName { get; set; }
    public Dictionary<string, object>? Summary { get; set; }
}

public class DownloadPublishedItemResponse
{
    public string DownloadId { get; set; } = string.Empty;
    public string ItemId { get; set; } = string.Empty;
    public string ItemTitle { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddHours(48);
    public string? Version { get; set; }
    public List<string> IncludedAssets { get; set; } = new();
    public Dictionary<string, object>? Metadata { get; set; }
}

public class DownloadThemeResponse
{
    public string DownloadId { get; set; } = string.Empty;
    public string ThemeId { get; set; } = string.Empty;
    public string ThemeName { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddHours(24);
    public string? Version { get; set; }
    public Dictionary<string, object>? ThemeProperties { get; set; }
}

public class YoutubeLinkResponse
{
    public string VideoId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string EmbedUrl { get; set; } = string.Empty;
    public string DirectUrl { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public string? Description { get; set; }
    public string Quality { get; set; } = string.Empty;
    public Dictionary<string, string>? AdditionalFormats { get; set; }
    public DateTime RetrievedAt { get; set; } = DateTime.UtcNow;
}