using System.ComponentModel.DataAnnotations;

namespace PowerBITips.Api.Models.DTOs.Requests;

public class TrackEventRequest
{
    [Required]
    public string EventName { get; set; } = string.Empty;
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    public string? SessionId { get; set; }
    
    public Dictionary<string, object>? Properties { get; set; }
    
    public DateTime? Timestamp { get; set; }
    
    public string? UserAgent { get; set; }
    
    public string? IpAddress { get; set; }
    
    public string? ReferrerUrl { get; set; }
}

public class TrackPageViewRequest
{
    [Required]
    public string PageUrl { get; set; } = string.Empty;
    
    [Required]
    public string PageTitle { get; set; } = string.Empty;
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    public string? SessionId { get; set; }
    
    public string? ReferrerUrl { get; set; }
    
    public Dictionary<string, object>? CustomDimensions { get; set; }
    
    public TimeSpan? TimeOnPage { get; set; }
    
    public DateTime? Timestamp { get; set; }
}

public class TrackLoginEventRequest
{
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    public string LoginMethod { get; set; } = string.Empty; // e.g., "google", "microsoft", "github"
    
    public string? SessionId { get; set; }
    
    public bool IsNewUser { get; set; } = false;
    
    public string? UserAgent { get; set; }
    
    public string? IpAddress { get; set; }
    
    public DateTime? Timestamp { get; set; }
    
    public Dictionary<string, object>? AdditionalData { get; set; }
}

public class PostGoogleAnalyticsRequest
{
    [Required]
    public string MeasurementId { get; set; } = string.Empty;
    
    [Required]
    public string ApiSecret { get; set; } = string.Empty;
    
    [Required]
    public string ClientId { get; set; } = string.Empty;
    
    [Required]
    public List<GoogleAnalyticsEvent> Events { get; set; } = new();
    
    public string? UserId { get; set; }
    
    public Dictionary<string, object>? UserProperties { get; set; }
}

public class GoogleAnalyticsEvent
{
    [Required]
    public string Name { get; set; } = string.Empty;
    
    public Dictionary<string, object>? Parameters { get; set; }
}