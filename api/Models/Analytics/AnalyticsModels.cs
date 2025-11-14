namespace PowerBITips.Api.Models.Analytics;

// Google Analytics Event Models
public class PostGoogleAnalyticsRequest
{
    public GoogleAnalyticsEventData EventData { get; set; } = new();
}

public class GoogleAnalyticsEventData
{
    public string ClientId { get; set; } = string.Empty;
    public List<GoogleAnalyticsEvent> Events { get; set; } = new();
}

public class GoogleAnalyticsEvent
{
    public string Name { get; set; } = string.Empty;
    public Dictionary<string, object> Params { get; set; } = new();
}

public class PostGoogleAnalyticsResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

// Event Tracking Models
public class EventTrackingRequest
{
    public string Action { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Label { get; set; }
    public int? Value { get; set; }
    public Dictionary<string, object>? Params { get; set; }
}

public class PageViewTrackingRequest
{
    public string Page { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Location { get; set; }
}

public class LoginEventRequest
{
    public string UserToken { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
}

// Analytics Configuration
public static class AnalyticsCategories
{
    public const string Accounts = "Accounts";
    public const string Themes = "Themes";
    public const string Gallery = "Gallery";
    public const string ThemeGenerator = "ThemeGenerator";
}