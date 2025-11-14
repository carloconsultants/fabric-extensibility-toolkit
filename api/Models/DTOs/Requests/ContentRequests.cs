using System.ComponentModel.DataAnnotations;

namespace PowerBITips.Api.Models.DTOs.Requests;

public class GetSharedResourcesRequest
{
    public string? ResourceType { get; set; } // e.g., "images", "templates", "docs"
    public string? Category { get; set; }
    public string? Tags { get; set; }
    public int? Limit { get; set; }
    public int? Offset { get; set; }
}

public class DownloadDataRequest
{
    [Required]
    public string DataType { get; set; } = string.Empty; // e.g., "published", "themes", "analytics"
    
    public string? UserId { get; set; }
    
    public string? Format { get; set; } = "json"; // "json", "csv", "xml"
    
    public DateTime? StartDate { get; set; }
    
    public DateTime? EndDate { get; set; }
    
    public List<string>? IncludeFields { get; set; }
    
    public List<string>? ExcludeFields { get; set; }
    
    public Dictionary<string, object>? Filters { get; set; }
}

public class DownloadPublishedItemRequest
{
    [Required]
    public string ItemId { get; set; } = string.Empty;
    
    [Required]
    public string RequestedBy { get; set; } = string.Empty;
    
    public string? Format { get; set; } = "zip";
    
    public bool IncludeMetadata { get; set; } = true;
    
    public bool IncludeAssets { get; set; } = true;
    
    public string? Version { get; set; }
}

public class DownloadThemeRequest
{
    [Required]
    public string ThemeId { get; set; } = string.Empty;
    
    [Required] 
    public string RequestedBy { get; set; } = string.Empty;
    
    public string? Format { get; set; } = "json";
    
    public bool IncludeCustomization { get; set; } = true;
    
    public string? Version { get; set; }
}

public class GetYoutubeLinkRequest
{
    public string? VideoId { get; set; }
    
    public string? PlaylistId { get; set; }
    
    public string? Category { get; set; }
    
    public bool? AutoPlay { get; set; } = false;
    
    public string? StartTime { get; set; } // e.g., "1m30s"
    
    public string? Quality { get; set; } = "720p";
}