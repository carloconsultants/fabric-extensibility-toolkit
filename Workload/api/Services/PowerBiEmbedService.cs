using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using PowerBITips.Api.Models.Embed;
using PowerBITips.Api.Utilities.Helpers;

namespace PowerBITips.Api.Services;

public interface IPowerBiEmbedService
{
    Task<string> GetClientTenantTokenAsync(ClientTenantTokenRequest request);
    Task<ReportInfoResponse?> GetReportInfoAsync(ReportInfoRequest request);
    Task<string> GetEmbedTokenAsync(EmbedTokenRequest request);
    Task<TokenExchangeResult> ExchangeUserTokenAsync(string userToken);
    Task<TokenExchangeResult> ExchangeUserTokenForResourceAsync(string userToken, string resource);
}

public class PowerBiEmbedService : IPowerBiEmbedService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PowerBiEmbedService> _logger;

    public PowerBiEmbedService(HttpClient httpClient, ILogger<PowerBiEmbedService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string> GetClientTenantTokenAsync(ClientTenantTokenRequest request)
    {
        try
        {
            var requestParams = new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = request.AppRegistration.ClientId,
                ["client_secret"] = request.AppRegistration.ClientSecret,
                ["resource"] = "https://analysis.windows.net/powerbi/api"
            };

            var content = new FormUrlEncodedContent(requestParams);
            var url = $"https://login.windows.net/{request.TenantId}/oauth2/token";

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Cookie",
                "fpc=AutbFI66xVNJvWQKZcyk981zLJ3kAQAAAJfMotYOAAAAFmxSAwEAAACezKLWDgAAAA");

            var response = await _httpClient.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to get client tenant token. Status: {StatusCode}, Response: {Response}",
                    response.StatusCode, responseContent);
                return string.Empty;
            }

            var tokenResponse = JsonSerializer.Deserialize<ClientTenantTokenResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            });

            return tokenResponse?.AccessToken ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting client tenant token");
            return string.Empty;
        }
    }

    public async Task<ReportInfoResponse?> GetReportInfoAsync(ReportInfoRequest request)
    {
        try
        {
            var url = $"https://api.powerbi.com/v1.0/myorg/groups/{request.PbiWorkspaceId}/reports/{request.PbiReportId}";

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {request.Token}");

            var response = await _httpClient.GetAsync(url);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to get report info. Status: {StatusCode}, Response: {Response}",
                    response.StatusCode, responseContent);
                return null;
            }

            var reportInfo = JsonSerializer.Deserialize<ReportInfoResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            return reportInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting report info for report {ReportId}", request.PbiReportId);
            return null;
        }
    }

    public async Task<string> GetEmbedTokenAsync(EmbedTokenRequest request)
    {
        try
        {
            var requestBody = new { accessLevel = "View" };
            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var url = $"https://api.powerbi.com/v1.0/myorg/groups/{request.PbiWorkspaceId}/reports/{request.PbiReportId}/GenerateToken";

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {request.Token}");

            var response = await _httpClient.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to get embed token. Status: {StatusCode}, Response: {Response}",
                    response.StatusCode, responseContent);
                return string.Empty;
            }

            var embedTokenResponse = JsonSerializer.Deserialize<EmbedTokenResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            return embedTokenResponse?.Token ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting embed token for report {ReportId}", request.PbiReportId);
            return string.Empty;
        }
    }

    public async Task<TokenExchangeResult> ExchangeUserTokenAsync(string userToken)
    {
        try
        {
            // Decode JWT to get tenant ID
            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jsonToken = handler.ReadJwtToken(userToken);
            var tenantId = jsonToken.Claims.FirstOrDefault(x => x.Type == "tid")?.Value;

            if (string.IsNullOrEmpty(tenantId))
            {
                _logger.LogError("Could not extract tenant ID from user token");
                return TokenExchangeResult.Failure(
                    "INVALID_TOKEN",
                    "The provided token is invalid or does not contain a tenant ID. Please ensure you are using a valid authentication token."
                );
            }

            _logger.LogInformation("Exchanging user token for PowerBI scope, Tenant ID: {TenantId}", tenantId);

            // Debug: Log token claims to understand what permissions we have
            var claims = jsonToken.Claims.ToList();
            _logger.LogInformation("Input token claims:");
            foreach (var claim in claims)
            {
                _logger.LogInformation("  {Type}: {Value}", claim.Type, claim.Value);
            }

            // Specifically check for scope/scp claims
            var scopeClaim = jsonToken.Claims.FirstOrDefault(x => x.Type == "scp" || x.Type == "scope")?.Value;
            _logger.LogInformation("Input token scopes: {Scopes}", scopeClaim ?? "No scopes found");

            // Get client credentials from environment
            string clientId, clientSecret;
            try
            {
                var envVars = ConfigurationHelper.GetRequiredEnvironmentVariables(
                    "AAD_APP_CLIENT_ID", "AAD_APP_CLIENT_SECRET");

                clientId = envVars["AAD_APP_CLIENT_ID"];
                clientSecret = envVars["AAD_APP_CLIENT_SECRET"];
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Configuration error: {Message}", ex.Message);
                return TokenExchangeResult.Failure(
                    "CONFIGURATION_ERROR",
                    "The application is not properly configured. Please contact support."
                );
            }

            var requestParams = new Dictionary<string, string>
            {
                ["grant_type"] = "urn:ietf:params:oauth:grant-type:jwt-bearer",
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret,
                ["assertion"] = userToken,
                ["requested_token_use"] = "on_behalf_of",
                ["scope"] = "https://analysis.windows.net/powerbi/api/.default"
            };

            var content = new FormUrlEncodedContent(requestParams);
            var url = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";

            // Clear any existing headers to ensure clean request (matching old TypeScript behavior)
            _httpClient.DefaultRequestHeaders.Clear();

            _logger.LogInformation("Sending OBO token exchange request to: {Url}", url);

            var response = await _httpClient.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Log the full response for debugging (matching old TypeScript console.log behavior)
            _logger.LogInformation("OBO token exchange response. Status: {StatusCode}, Response: {Response}",
                response.StatusCode, responseContent);

            if (!response.IsSuccessStatusCode)
            {
                // Try to parse error details from Azure AD response
                string? azureAdErrorCode = null;
                string? azureAdErrorDescription = null;
                string errorCode = "TOKEN_EXCHANGE_FAILED";
                string errorDescription = "Failed to exchange the user token. Please try again or contact support if the issue persists.";

                try
                {
                    var errorResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    if (errorResponse.TryGetProperty("error", out var errorProp))
                    {
                        azureAdErrorCode = errorProp.GetString();
                        azureAdErrorDescription = errorResponse.TryGetProperty("error_description", out var errorDesc)
                            ? errorDesc.GetString()
                            : null;

                        // Map Azure AD error codes to our error codes and determine if it's an authorization issue
                        if (azureAdErrorCode != null)
                        {
                            if (azureAdErrorCode.Contains("65001") || azureAdErrorCode.Contains("AADSTS65001"))
                            {
                                // User consent required
                                errorCode = "CONSENT_REQUIRED";
                                errorDescription = "The user or administrator has not consented to use the application. Please grant the required permissions and try again.";
                            }
                            else if (azureAdErrorCode.Contains("70011") || azureAdErrorCode.Contains("AADSTS70011"))
                            {
                                // Invalid scope
                                errorCode = "INVALID_SCOPE";
                                errorDescription = "The requested scope is invalid or not configured for this application.";
                            }
                            else if (azureAdErrorCode.Contains("700016") || azureAdErrorCode.Contains("AADSTS700016"))
                            {
                                // Application not found
                                errorCode = "APPLICATION_NOT_FOUND";
                                errorDescription = "The application is not found in the directory. Please verify the application configuration.";
                            }
                            else if (azureAdErrorCode.Contains("7000215") || azureAdErrorCode.Contains("AADSTS7000215"))
                            {
                                // Invalid client secret
                                errorCode = "INVALID_CLIENT_SECRET";
                                errorDescription = "The client secret provided is invalid. Please verify the application configuration.";
                            }
                            else if (azureAdErrorCode.Contains("invalid_grant") || azureAdErrorCode.Contains("invalid_assertion"))
                            {
                                // Invalid token
                                errorCode = "INVALID_TOKEN";
                                errorDescription = "The provided token is invalid or has expired. Please acquire a new token and try again.";
                            }
                            else if (azureAdErrorCode.Contains("unauthorized_client"))
                            {
                                // Unauthorized client
                                errorCode = "UNAUTHORIZED_CLIENT";
                                errorDescription = "The client is not authorized to request tokens for this resource. Please verify the application permissions.";
                            }
                            else
                            {
                                // Use Azure AD error description if available, otherwise use generic message
                                errorDescription = azureAdErrorDescription ?? errorDescription;
                            }
                        }

                        _logger.LogError("Failed to exchange user token. Azure AD Error: {Error}, Description: {Description}, Full Response: {Response}",
                            azureAdErrorCode, azureAdErrorDescription, responseContent);
                    }
                    else
                    {
                        _logger.LogError("Failed to exchange user token. Status: {StatusCode}, Response: {Response}",
                            response.StatusCode, responseContent);
                    }
                }
                catch (Exception parseEx)
                {
                    _logger.LogError(parseEx, "Failed to parse error response. Status: {StatusCode}, Response: {Response}",
                        response.StatusCode, responseContent);
                }

                return TokenExchangeResult.Failure(
                    errorCode,
                    errorDescription,
                    azureAdErrorCode,
                    azureAdErrorDescription
                );
            }

            var tokenResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
            if (tokenResponse.TryGetProperty("access_token", out var accessTokenProp))
            {
                var accessToken = accessTokenProp.GetString();
                if (string.IsNullOrEmpty(accessToken))
                {
                    _logger.LogError("Token exchange succeeded but access_token is empty in response: {Response}", responseContent);
                    return TokenExchangeResult.Failure(
                        "EMPTY_TOKEN",
                        "The token exchange succeeded but no access token was returned. Please try again."
                    );
                }

                _logger.LogInformation("Successfully exchanged user token for PowerBI");
                return TokenExchangeResult.Success(accessToken);
            }

            _logger.LogError("Token exchange succeeded but access_token not found in response: {Response}", responseContent);
            return TokenExchangeResult.Failure(
                "MISSING_TOKEN",
                "The token exchange succeeded but the access token was not found in the response. Please try again."
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exchanging user token for PowerBI");
            return TokenExchangeResult.Failure(
                "INTERNAL_ERROR",
                "An internal error occurred while exchanging the token. Please try again or contact support if the issue persists."
            );
        }
    }

    public async Task<TokenExchangeResult> ExchangeUserTokenForResourceAsync(string userToken, string resource)
    {
        try
        {
            // Decode JWT to get tenant ID
            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jsonToken = handler.ReadJwtToken(userToken);
            var tenantId = jsonToken.Claims.FirstOrDefault(x => x.Type == "tid")?.Value;

            if (string.IsNullOrEmpty(tenantId))
            {
                _logger.LogError("Could not extract tenant ID from user token");
                return TokenExchangeResult.Failure(
                    "INVALID_TOKEN",
                    "The provided token is invalid or does not contain a tenant ID. Please ensure you are using a valid authentication token."
                );
            }

            _logger.LogInformation("Exchanging user token for resource: {Resource}, Tenant ID: {TenantId}", resource, tenantId);

            // Get client credentials from environment
            string clientId, clientSecret;
            try
            {
                var envVars = ConfigurationHelper.GetRequiredEnvironmentVariables(
                    "AAD_APP_CLIENT_ID", "AAD_APP_CLIENT_SECRET");

                clientId = envVars["AAD_APP_CLIENT_ID"];
                clientSecret = envVars["AAD_APP_CLIENT_SECRET"];
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Configuration error: {Message}", ex.Message);
                return TokenExchangeResult.Failure(
                    "CONFIGURATION_ERROR",
                    "The application is not properly configured. Please contact support."
                );
            }

            // Use v1.0 endpoint with resource parameter for OneLake/Storage scenarios
            var requestParams = new Dictionary<string, string>
            {
                ["grant_type"] = "urn:ietf:params:oauth:grant-type:jwt-bearer",
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret,
                ["assertion"] = userToken,
                ["requested_token_use"] = "on_behalf_of",
                ["resource"] = resource,
                ["scope"] = "openid"
            };

            var content = new FormUrlEncodedContent(requestParams);
            var url = $"https://login.microsoftonline.com/{tenantId}/oauth2/token";

            // Clear any existing headers to ensure clean request
            _httpClient.DefaultRequestHeaders.Clear();

            _logger.LogInformation("Sending OBO token exchange request to: {Url}", url);

            var response = await _httpClient.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("OBO token exchange response. Status: {StatusCode}, Response: {Response}",
                response.StatusCode, responseContent);

            if (!response.IsSuccessStatusCode)
            {
                // Error handling logic similar to PowerBI method...
                var errorResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                string? azureAdErrorCode = null;
                string? azureAdErrorDescription = null;

                if (errorResponse.TryGetProperty("error", out var errorProp))
                {
                    azureAdErrorCode = errorProp.GetString();
                    azureAdErrorDescription = errorResponse.TryGetProperty("error_description", out var errorDesc)
                        ? errorDesc.GetString()
                        : null;
                }

                return TokenExchangeResult.Failure(
                    "TOKEN_EXCHANGE_FAILED",
                    "Failed to exchange token for resource",
                    azureAdErrorCode,
                    azureAdErrorDescription
                );
            }

            var tokenResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
            if (tokenResponse.TryGetProperty("access_token", out var accessTokenProp))
            {
                var accessToken = accessTokenProp.GetString();
                if (string.IsNullOrEmpty(accessToken))
                {
                    return TokenExchangeResult.Failure(
                        "EMPTY_TOKEN",
                        "The token exchange succeeded but no access token was returned."
                    );
                }

                return TokenExchangeResult.Success(accessToken);
            }

            return TokenExchangeResult.Failure(
                "MISSING_ACCESS_TOKEN",
                "No access token found in the response."
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exchanging user token for resource: {Resource}", resource);
            return TokenExchangeResult.Failure(
                "INTERNAL_ERROR",
                "An internal error occurred while exchanging the token. Please try again or contact support if the issue persists."
            );
        }
    }
}