namespace PowerBITips.Api.Models.Embed;

public class EmbedTokenResponse
{
    public string ODataContext { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string TokenId { get; set; } = string.Empty;
    public string Expiration { get; set; } = string.Empty;
}

public class EmbedTokenRequest
{
    public string Token { get; set; } = string.Empty;
    public string PbiWorkspaceId { get; set; } = string.Empty;
    public string PbiReportId { get; set; } = string.Empty;
}

public class ReportInfoResponse
{
    public string ODataContext { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
    public string ReportType { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string WebUrl { get; set; } = string.Empty;
    public string EmbedUrl { get; set; } = string.Empty;
    public bool IsFromPbix { get; set; }
    public bool IsOwnedByMe { get; set; }
    public string DatasetId { get; set; } = string.Empty;
}

public class ReportInfoRequest
{
    public string Token { get; set; } = string.Empty;
    public string PbiWorkspaceId { get; set; } = string.Empty;
    public string PbiReportId { get; set; } = string.Empty;
}

public class ClientTenantTokenResponse
{
    public string TokenType { get; set; } = "Bearer";
    public string ExpiresIn { get; set; } = string.Empty;
    public string ExtExpiresIn { get; set; } = string.Empty;
    public string ExpiresOn { get; set; } = string.Empty;
    public string NotBefore { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
}

public class ClientTenantTokenRequest
{
    public string TenantId { get; set; } = string.Empty;
    public AppRegistration AppRegistration { get; set; } = new();
}

public class AppRegistration
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
}

// Request/Response models for the API endpoints
public class EmbedAccessRequest
{
    public string PbiReportId { get; set; } = string.Empty;
}

public class EmbedAccessResponse
{
    public string Id { get; set; } = string.Empty;
    public string EmbedUrl { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
}

public class OboExchangeRequest
{
    public string Token { get; set; } = string.Empty;
}

public class OboExchangeResponse
{
    public string Token { get; set; } = string.Empty;
}