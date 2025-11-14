namespace PowerBITips.Api.Models.Content;

// YouTube Link Models
public class GetYoutubeLinkResponse
{
    public string YoutubeUrl { get; set; } = string.Empty;
}

// Shared Resources Models
public class GetSharedResourcesRequest
{
    public string[] RequestedResources { get; set; } = Array.Empty<string>();
}

public class SharedResourceItem
{
    public string? Theme { get; set; }
    public string? Project { get; set; }
    public string? Images { get; set; }
    public string[]? Palette { get; set; }
    public string ThemeId { get; set; } = string.Empty;
}

public class GetSharedResourcesResponse
{
    public List<SharedResourceItem> SharedResources { get; set; } = new();
}

// Content Resource Types (for filtering)
public static class ContentResourceTypes
{
    public const string Theme = "theme";
    public const string Project = "project"; 
    public const string Scrims = "scrims";
    public const string Palette = "palette";
}