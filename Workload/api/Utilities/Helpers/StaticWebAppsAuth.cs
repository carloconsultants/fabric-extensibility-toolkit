using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace PowerBITips.Api.Utilities.Helpers;

public static class StaticWebAppsAuth
{
    public class ClientPrincipal
    {
        public string? UserId { get; set; }
        public List<string>? UserRoles { get; set; }
        public string? IdentityProvider { get; set; }
        public string? UserDetails { get; set; }
        public List<Claim>? Claims { get; set; }
    }

    public class Claim
    {
        public string? Typ { get; set; }
        public string? Val { get; set; }
    }

    public static ClientPrincipal Parse(HttpRequestData req, ILogger? logger = null)
    {
        // Return ONLY what is supplied via header. No automatic local fallback user.
        // Fabric workload will supply a base64 client principal built from the OBO token.
        var principal = new ClientPrincipal();

        logger?.LogInformation("🔍 [StaticWebAppsAuth] Parsing client principal from request headers");

        // Log all available headers for debugging
        var allHeaders = req.Headers.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}").ToList();
        logger?.LogInformation("📋 [StaticWebAppsAuth] All request headers: {Headers}", string.Join(" | ", allHeaders));

        // Accept both standard SWA header and proxy header used by workload front-end.
        var hasStandardHeader = req.Headers.TryGetValues("x-ms-client-principal", out var standardHeaders);
        var hasProxyHeader = req.Headers.TryGetValues("x-ms-client-principal-proxy", out var proxyHeaders);

        logger?.LogInformation("🔑 [StaticWebAppsAuth] Header check - Standard: {HasStandard}, Proxy: {HasProxy}",
            hasStandardHeader, hasProxyHeader);

        var headers = standardHeaders ?? proxyHeaders;
        var headerName = hasStandardHeader ? "x-ms-client-principal" : "x-ms-client-principal-proxy";

        if (!hasStandardHeader && !hasProxyHeader || headers == null)
        {
            logger?.LogWarning("⚠️ [StaticWebAppsAuth] No client principal headers found in request");
            return principal; // empty principal (caller must handle unauthenticated state)
        }

        var header = headers.FirstOrDefault();
        if (string.IsNullOrEmpty(header))
        {
            logger?.LogWarning("⚠️ [StaticWebAppsAuth] Client principal header exists but is empty");
            return principal;
        }

        logger?.LogInformation("📦 [StaticWebAppsAuth] Found {HeaderName} header (length: {Length})",
            headerName, header.Length);

        try
        {
            var decoded = Convert.FromBase64String(header);
            var json = System.Text.Encoding.UTF8.GetString(decoded);
            logger?.LogInformation("📄 [StaticWebAppsAuth] Decoded JSON: {Json}", json);

            var clientPrincipal = JsonSerializer.Deserialize<ClientPrincipal>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (clientPrincipal != null)
            {
                principal = clientPrincipal;

                // Log FULL client principal details
                logger?.LogInformation("✅ [StaticWebAppsAuth] ========== FULL CLIENT PRINCIPAL DETAILS ==========");
                logger?.LogInformation("   UserId: {UserId}", principal.UserId ?? "NULL");
                logger?.LogInformation("   UserDetails: {UserDetails}", principal.UserDetails ?? "NULL");
                logger?.LogInformation("   IdentityProvider: {IdentityProvider}", principal.IdentityProvider ?? "NULL");
                logger?.LogInformation("   UserRoles Count: {Count}", principal.UserRoles?.Count ?? 0);
                if (principal.UserRoles != null && principal.UserRoles.Any())
                {
                    logger?.LogInformation("   UserRoles: {Roles}", string.Join(", ", principal.UserRoles));
                }
                logger?.LogInformation("   Claims Count: {Count}", principal.Claims?.Count ?? 0);
                if (principal.Claims != null && principal.Claims.Any())
                {
                    foreach (var claim in principal.Claims)
                    {
                        logger?.LogInformation("     Claim - Type: {Type}, Value: {Value}", claim.Typ ?? "NULL", claim.Val ?? "NULL");
                    }
                }
                logger?.LogInformation("   Full JSON: {Json}", json);
                logger?.LogInformation("✅ [StaticWebAppsAuth] ================================================");
            }
            else
            {
                logger?.LogWarning("⚠️ [StaticWebAppsAuth] Deserialized client principal is null");
            }
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "❌ [StaticWebAppsAuth] Error parsing client principal header");
            // On any parse error, return empty principal (do NOT inject a fake user)
        }
        return principal;
    }

    /// <summary>
    /// Parse client principal from AspNetCore HttpRequest (for middleware integration)
    /// </summary>
    public static ClientPrincipal Parse(HttpRequest req)
    {
        var principal = new ClientPrincipal();

        var hasHeader = req.Headers.TryGetValue("x-ms-client-principal", out var headerValues) ||
                        req.Headers.TryGetValue("x-ms-client-principal-proxy", out headerValues);
        if (!hasHeader || headerValues.Count == 0)
            return principal;

        var header = headerValues.FirstOrDefault();
        if (string.IsNullOrEmpty(header)) return principal;

        try
        {
            var decoded = Convert.FromBase64String(header);
            var json = System.Text.Encoding.UTF8.GetString(decoded);
            var clientPrincipal = JsonSerializer.Deserialize<ClientPrincipal>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            if (clientPrincipal != null)
                principal = clientPrincipal;
        }
        catch
        {
            // Silent failure -> empty principal
        }
        return principal;
    }
}