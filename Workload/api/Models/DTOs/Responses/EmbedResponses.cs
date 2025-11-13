namespace PowerBITips.Api.Models.DTOs.Responses;

public class EmbedAccessResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string TokenId { get; set; } = string.Empty;
    public string EmbedUrl { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string ReportId { get; set; } = string.Empty;
    public string WorkspaceId { get; set; } = string.Empty;
    public string? DatasetId { get; set; }
    public List<string> Permissions { get; set; } = new();
    public Dictionary<string, object>? EmbedConfiguration { get; set; }
    public string TokenType { get; set; } = "Embed";
    public TimeSpan ValidityDuration { get; set; }
}

public class EmbedTokenValidationResponse
{
    public bool IsValid { get; set; }
    public string Status { get; set; } = string.Empty; // "Valid", "Expired", "Invalid", "Revoked"
    public DateTime? ExpiresAt { get; set; }
    public string? UserId { get; set; }
    public string? ReportId { get; set; }
    public string? WorkspaceId { get; set; }
    public List<string> Permissions { get; set; } = new();
    public Dictionary<string, object>? TokenClaims { get; set; }
    public DateTime ValidatedAt { get; set; } = DateTime.UtcNow;
}

public class EmbedConfigurationResponse
{
    public string WorkspaceId { get; set; } = string.Empty;
    public Dictionary<string, string> ReportEmbedUrls { get; set; } = new();
    public Dictionary<string, string> DatasetConnections { get; set; } = new();
    public Dictionary<string, object> Settings { get; set; } = new();
    public string? ThemeConfiguration { get; set; }
    public string Language { get; set; } = "en-US";
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public List<EmbedCapability> AvailableCapabilities { get; set; } = new();
}

public class EmbedCapability
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public Dictionary<string, object>? Parameters { get; set; }
}

public class EmbedUsageStatsResponse
{
    public string WorkspaceId { get; set; } = string.Empty;
    public int TotalEmbeds { get; set; }
    public int ActiveTokens { get; set; }
    public int ExpiredTokens { get; set; }
    public Dictionary<string, int> EmbedsByReport { get; set; } = new();
    public Dictionary<string, int> EmbedsByUser { get; set; } = new();
    public DateTime LastEmbed { get; set; }
    public TimeSpan AverageTokenLifetime { get; set; }
    public Dictionary<string, object>? AdditionalMetrics { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}