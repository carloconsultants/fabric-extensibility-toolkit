using System.Net.Http.Headers;
using System.Text.Json;
using TemplateWorkload.Core;
using TemplateWorkload.Models;
using TemplateWorkload.Utilities;
using Microsoft.Extensions.Logging;

namespace TemplateWorkload.Core
{
    public interface IFabricApi
    {
        Task<ServiceResponse<CreateExternalDataShareResponse>> CreateExternalDataShareAsync(
            AppRegistrationCredentials credentials, 
            string workspaceId, 
            string itemId, 
            List<string> paths, 
            ExternalDataShareRecipient recipient);
    }

    public class FabricApi : IFabricApi
    {
        private readonly IKeyVaultAccess _keyVaultAccess;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<FabricApi> _logger;

        public FabricApi(IKeyVaultAccess keyVaultAccess, IHttpClientFactory httpClientFactory, ILogger<FabricApi> logger)
        {
            _keyVaultAccess = keyVaultAccess;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<ServiceResponse<CreateExternalDataShareResponse>> CreateExternalDataShareAsync(
            AppRegistrationCredentials credentials, 
            string workspaceId, 
            string itemId, 
            List<string> paths, 
            ExternalDataShareRecipient recipient)
        {
            try
            {
                // Validate input parameters
                if (string.IsNullOrEmpty(credentials.ClientId))
                {
                    return ServiceResponse<CreateExternalDataShareResponse>.Error(
                        System.Net.HttpStatusCode.BadRequest, 
                        "Client ID is required");
                }
                
                if (string.IsNullOrEmpty(credentials.ClientSecret))
                {
                    return ServiceResponse<CreateExternalDataShareResponse>.Error(
                        System.Net.HttpStatusCode.BadRequest, 
                        "Client secret is required");
                }
                
                if (string.IsNullOrEmpty(credentials.TenantId))
                {
                    return ServiceResponse<CreateExternalDataShareResponse>.Error(
                        System.Net.HttpStatusCode.BadRequest, 
                        "Tenant ID is required");
                }
                
                if (string.IsNullOrEmpty(workspaceId))
                {
                    return ServiceResponse<CreateExternalDataShareResponse>.Error(
                        System.Net.HttpStatusCode.BadRequest, 
                        "Workspace ID is required");
                }
                
                if (string.IsNullOrEmpty(itemId))
                {
                    return ServiceResponse<CreateExternalDataShareResponse>.Error(
                        System.Net.HttpStatusCode.BadRequest, 
                        "Item ID is required");
                }
                
                if (paths == null || !paths.Any())
                {
                    return ServiceResponse<CreateExternalDataShareResponse>.Error(
                        System.Net.HttpStatusCode.BadRequest, 
                        "At least one path is required");
                }

                // Get access token using client credentials flow
                var accessToken = await GetAccessTokenAsync(credentials);
                if (string.IsNullOrEmpty(accessToken))
                {
                    _logger.LogError("Failed to obtain access token for client ID: {ClientId}, tenant: {TenantId}", 
                        credentials.ClientId, credentials.TenantId);
                    return ServiceResponse<CreateExternalDataShareResponse>.Error(
                        System.Net.HttpStatusCode.Unauthorized, 
                        "Failed to obtain access token. Please verify your app registration credentials.");
                }

                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var apiUrl = $"https://api.fabric.microsoft.com/v1/workspaces/{workspaceId}/items/{itemId}/externalDataShares";

                var requestBody = new CreateExternalDataShareRequest
                {
                    Paths = paths,
                    Recipient = recipient
                };

                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(requestBody), 
                    System.Text.Encoding.UTF8, 
                    "application/json");

                _logger.LogInformation("Creating external data share for workspace: {WorkspaceId}, item: {ItemId}", workspaceId, itemId);
                _logger.LogInformation("Request URL: {Url}", apiUrl);
                _logger.LogInformation("Request body: {RequestBody}", JsonSerializer.Serialize(requestBody));
                _logger.LogInformation("Access token (first 20 chars): {TokenPrefix}", accessToken?.Substring(0, Math.Min(20, accessToken?.Length ?? 0)));

                var response = await client.PostAsync(apiUrl, jsonContent);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to create external data share: {StatusCode} - {Content}", response.StatusCode, content);
                    return ServiceResponse<CreateExternalDataShareResponse>.Error(
                        response.StatusCode, 
                        $"Failed to create external data share: {content}");
                }

                var shareResponse = JsonSerializer.Deserialize<CreateExternalDataShareResponse>(content);
                if (shareResponse == null)
                {
                    return ServiceResponse<CreateExternalDataShareResponse>.Error(
                        System.Net.HttpStatusCode.InternalServerError, 
                        "Failed to deserialize response");
                }

                _logger.LogInformation("Successfully created external data share: {ShareId}", shareResponse.Id);
                return ServiceResponse<CreateExternalDataShareResponse>.Success(shareResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating external data share");
                return ServiceResponse<CreateExternalDataShareResponse>.Error(
                    System.Net.HttpStatusCode.InternalServerError, 
                    $"Error creating external data share: {ex.Message}");
            }
        }

        private async Task<string?> GetAccessTokenAsync(AppRegistrationCredentials credentials)
        {
            try
            {
                using var httpClient = new HttpClient();
                
                var tokenRequest = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client_id", credentials.ClientId),
                    new KeyValuePair<string, string>("client_secret", credentials.ClientSecret),
                    new KeyValuePair<string, string>("scope", "https://analysis.windows.net/powerbi/api/.default"),
                    new KeyValuePair<string, string>("grant_type", "client_credentials")
                });

                var tokenUrl = $"https://login.microsoftonline.com/{credentials.TenantId}/oauth2/v2.0/token";
                
                var response = await httpClient.PostAsync(tokenUrl, tokenRequest);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var tokenResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    if (tokenResponse.TryGetProperty("access_token", out var accessToken))
                    {
                        _logger.LogInformation("Successfully obtained access token for client ID: {ClientId}", credentials.ClientId);
                        return accessToken.GetString();
                    }
                    else
                    {
                        _logger.LogError("Access token not found in response for client ID: {ClientId}. Response: {Content}", 
                            credentials.ClientId, responseContent);
                    }
                }
                else
                {
                    _logger.LogError("Failed to obtain access token for client ID: {ClientId}. Status: {StatusCode}, Response: {Content}", 
                        credentials.ClientId, response.StatusCode, responseContent);
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obtaining access token");
                return null;
            }
        }
    }
}
