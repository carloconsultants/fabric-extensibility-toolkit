namespace PowerBITips.Api.Models.DTOs.Responses;

public class TrackEventResponse
{
    public bool Success { get; set; } = true;
    public string EventId { get; set; } = string.Empty;
    public string Message { get; set; } = "Event tracked successfully";
    public DateTime TrackedAt { get; set; } = DateTime.UtcNow;
    public string? SessionId { get; set; }
}

public class TrackPageViewResponse
{
    public bool Success { get; set; } = true;
    public string PageViewId { get; set; } = string.Empty;
    public string Message { get; set; } = "Page view tracked successfully";
    public DateTime TrackedAt { get; set; } = DateTime.UtcNow;
    public string? SessionId { get; set; }
    public TimeSpan? SessionDuration { get; set; }
}

public class TrackLoginEventResponse
{
    public bool Success { get; set; } = true;
    public string LoginEventId { get; set; } = string.Empty;
    public string Message { get; set; } = "Login event tracked successfully";
    public DateTime TrackedAt { get; set; } = DateTime.UtcNow;
    public bool IsNewUserSession { get; set; } = false;
    public string? WelcomeMessage { get; set; }
}

public class GoogleAnalyticsResponse
{
    public bool Success { get; set; } = true;
    public string Message { get; set; } = "Events sent to Google Analytics successfully";
    public int EventsProcessed { get; set; }
    public List<string> EventIds { get; set; } = new();
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    public string? ValidationMessages { get; set; }
}

public class AnalyticsStatsResponse
{
    public string UserId { get; set; } = string.Empty;
    public int TotalEvents { get; set; }
    public int TotalPageViews { get; set; }
    public int TotalSessions { get; set; }
    public DateTime LastActivity { get; set; }
    public TimeSpan AverageSessionDuration { get; set; }
    public Dictionary<string, int> TopEvents { get; set; } = new();
    public Dictionary<string, int> TopPages { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}