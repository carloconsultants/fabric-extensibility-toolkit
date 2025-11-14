using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PowerBITips.Api.Models.Authentication;
using PowerBITips.Api.Services.Interfaces;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PowerBITips.Api.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthenticationService> _logger;

        public AuthenticationService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<AuthenticationService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public ClientPrincipal? GetClientPrincipal(HttpRequestData request)
        {
            try
            {
                if (request.Headers.TryGetValues("x-ms-client-principal", out var headerValues))
                {
                    var header = headerValues.FirstOrDefault();
                    if (!string.IsNullOrEmpty(header))
                    {
                        var data = Convert.FromBase64String(header);
                        var json = Encoding.UTF8.GetString(data);
                        var principal = JsonSerializer.Deserialize<ClientPrincipal>(json, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        _logger.LogInformation("Successfully extracted client principal for user: {UserId}", principal?.UserId);
                        return principal;
                    }
                }

                _logger.LogWarning("No x-ms-client-principal header found in request");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting client principal from request");
                return null;
            }
        }

        public async Task<string?> GetFabricAccessTokenAsync(ClientPrincipal principal)
        {
            // Fabric API requires the https://analysis.windows.net/powerbi/api/.default scope
            return await GetAccessTokenAsync(principal, new[] { "https://analysis.windows.net/powerbi/api/.default" });
        }

        public async Task<string?> GetAccessTokenAsync(ClientPrincipal principal, string[] scopes)
        {
            try
            {
                if (principal == null)
                {
                    _logger.LogWarning("Cannot get access token: principal is null");
                    return null;
                }

                var clientId = _configuration["AZURE_CLIENT_ID"];
                var clientSecret = _configuration["AZURE_CLIENT_SECRET"];
                var tenantId = _configuration["AZURE_TENANT_ID"];

                if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret) || string.IsNullOrEmpty(tenantId))
                {
                    _logger.LogError("Missing Azure AD configuration. ClientId, ClientSecret, or TenantId not found");
                    return null;
                }

                // Use the On-Behalf-Of flow to get an access token
                // This requires the user's access token from the client principal
                var userAccessToken = GetUserAccessTokenFromPrincipal(principal);
                if (string.IsNullOrEmpty(userAccessToken))
                {
                    _logger.LogWarning("Could not extract user access token from principal");
                    return null;
                }

                var tokenEndpoint = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";
                var requestBody = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    {"grant_type", "urn:ietf:params:oauth:grant-type:jwt-bearer"},
                    {"client_id", clientId},
                    {"client_secret", clientSecret},
                    {"assertion", userAccessToken},
                    {"scope", string.Join(" ", scopes)},
                    {"requested_token_use", "on_behalf_of"}
                });

                _logger.LogInformation("Requesting on-behalf-of token for scopes: {Scopes}", string.Join(", ", scopes));

                var response = await _httpClient.PostAsync(tokenEndpoint, requestBody);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseContent);
                    _logger.LogInformation("Successfully acquired access token");
                    return tokenResponse?.AccessToken;
                }
                else
                {
                    _logger.LogError("Failed to acquire access token. Status: {StatusCode}, Response: {Response}",
                        response.StatusCode, responseContent);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error acquiring access token");
                return null;
            }
        }

        private string? GetUserAccessTokenFromPrincipal(ClientPrincipal principal)
        {
            // Try to find the access token in claims
            var accessTokenClaim = principal.Claims.FirstOrDefault(c =>
                string.Equals(c.Type, "access_token", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(c.Type, "token", StringComparison.OrdinalIgnoreCase));

            return accessTokenClaim?.Value;
        }

        private class TokenResponse
        {
            [JsonPropertyName("access_token")]
            public string? AccessToken { get; set; }
        }
    }
}