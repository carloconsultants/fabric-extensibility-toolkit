using Azure;
using Azure.Data.Tables;
using PowerBITips.Api.Models.Enums;

namespace PowerBITips.Api.Models.Entities;

public class ThemeEntity : ITableEntity
{
    public string PartitionKey { get; set; } = string.Empty;
    public string RowKey { get; set; } = string.Empty;
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    // Theme Properties
    public string ThemeId => RowKey;
    public int DownloadCount { get; set; }
    public string JSON { get; set; } = string.Empty;
    public string ProjectJSON { get; set; } = string.Empty;
    public string Relationships { get; set; } = string.Empty;
    public string ThemeFile { get; set; } = string.Empty;
    public string ProjectFile { get; set; } = string.Empty;
    public string ProjectImages { get; set; } = string.Empty;
    public ThemeType Type { get; set; }
    public DateTime Created { get; set; }
    public string Url { get; set; } = string.Empty;
    public string ProjectUrl { get; set; } = string.Empty;
    public string ProjectImagesUrl { get; set; } = string.Empty;
    public string ThemeName { get; set; } = string.Empty;
    public string ScrimsFamilyName { get; set; } = string.Empty;
    public bool IsCommunity { get; set; }
    public string UserId { get; set; } = string.Empty;
}