using System.ComponentModel.DataAnnotations;

namespace PowerBITips.Api.Models.DTOs.Requests;

public class EmbedAccessRequest
{
    [Required]
    public string WorkspaceId { get; set; } = string.Empty;
    
    [Required]
    public string ReportId { get; set; } = string.Empty;
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    public string? DatasetId { get; set; }
    
    public List<string>? Roles { get; set; }
    
    public Dictionary<string, object>? RowLevelSecurityFilters { get; set; }
    
    public string? AccessLevel { get; set; } = "View"; // "View", "Edit", "Create"
    
    public TimeSpan? TokenLifetime { get; set; } = TimeSpan.FromHours(1);
    
    public string? IdentityType { get; set; } = "ServicePrincipal"; // "ServicePrincipal", "MasterUser"
    
    public bool? AllowSaveAs { get; set; } = false;
    
    public bool? AllowPrint { get; set; } = true;
    
    public bool? AllowExport { get; set; } = true;
}

public class RefreshEmbedTokenRequest
{
    [Required]
    public string TokenId { get; set; } = string.Empty;
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    public TimeSpan? ExtendLifetime { get; set; } = TimeSpan.FromHours(1);
    
    public bool? MaintainSamePermissions { get; set; } = true;
}

public class ValidateEmbedTokenRequest
{
    [Required]
    public string AccessToken { get; set; } = string.Empty;
    
    public string? UserId { get; set; }
    
    public string? ReportId { get; set; }
}

public class EmbedConfigurationRequest
{
    [Required]
    public string WorkspaceId { get; set; } = string.Empty;
    
    public List<string>? ReportIds { get; set; }
    
    public List<string>? DatasetIds { get; set; }
    
    public Dictionary<string, object>? Settings { get; set; }
    
    public string? Theme { get; set; }
    
    public string? Language { get; set; } = "en-US";
}