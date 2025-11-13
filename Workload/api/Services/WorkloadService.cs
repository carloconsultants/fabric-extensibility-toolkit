using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PowerBITips.Api.Models.Authentication;
using PowerBITips.Api.Models.Workload;
using PowerBITips.Api.Services.Common;
using PowerBITips.Api.Services.Interfaces;
using System.Net;
using System.Text;

namespace PowerBITips.Api.Services
{
    public class WorkloadService : IWorkloadService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<WorkloadService> _logger;
        private readonly IAuthenticationService _authenticationService;
        private readonly string _fabricApiBaseUrl;

        public WorkloadService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<WorkloadService> logger,
            IAuthenticationService authenticationService)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _authenticationService = authenticationService;
            _fabricApiBaseUrl = _configuration["FABRIC_API_BASE_URL"] ?? "https://api.fabric.microsoft.com/v1";
        }

        public Task<ServiceResponse<WorkloadInfoResponse>> GetWorkloadInfoAsync(string workspaceId)
        {
            try
            {
                _logger.LogInformation("Getting workload info for workspace {WorkspaceId}", workspaceId);

                // For now, return a placeholder response since the original function returns empty object
                var response = new WorkloadInfoResponse
                {
                    WorkspaceId = workspaceId,
                    Items = new List<WorkloadItem>(),
                    Metadata = new Dictionary<string, object>
                    {
                        { "status", "active" },
                        { "itemCount", 0 }
                    }
                };

                return Task.FromResult(ServiceResponse<WorkloadInfoResponse>.Success(response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting workload info for workspace {WorkspaceId}", workspaceId);
                return Task.FromResult(ServiceResponse<WorkloadInfoResponse>.Error(
                    HttpStatusCode.InternalServerError,
                    $"Error getting workload info: {ex.Message}"));
            }
        }

        public Task<ServiceResponse<ItemPayloadResponse>> GetItemPayloadAsync(string workspaceId, string itemType, string itemId)
        {
            try
            {
                _logger.LogInformation("Getting item payload for {ItemType} {ItemId} in workspace {WorkspaceId}",
                    itemType, itemId, workspaceId);

                // Based on the original TypeScript function, return a placeholder payload
                var response = new ItemPayloadResponse
                {
                    ItemId = itemId,
                    ItemType = Enum.TryParse<WorkloadItemType>(itemType, true, out var parsedType)
                        ? parsedType
                        : WorkloadItemType.Report,
                    ItemPayload = new Dictionary<string, object>(),
                    ContentType = "application/json",
                    Size = 0
                };

                return Task.FromResult(ServiceResponse<ItemPayloadResponse>.Success(response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting item payload for {ItemType} {ItemId} in workspace {WorkspaceId}",
                    itemType, itemId, workspaceId);
                return Task.FromResult(ServiceResponse<ItemPayloadResponse>.Error(
                    HttpStatusCode.InternalServerError,
                    $"Error getting item payload: {ex.Message}"));
            }
        }

        public Task<ServiceResponse<CreateWorkloadItemResponse>> CreateWorkloadItemAsync(
            string workspaceId, string itemType, CreateWorkloadItemRequest request)
        {
            try
            {
                _logger.LogInformation("Creating workload item {ItemType} in workspace {WorkspaceId} with display name {DisplayName}",
                    itemType, workspaceId, request.DisplayName);

                // Generate a new item ID
                var itemId = Guid.NewGuid().ToString();

                // For now, return a success response
                // In a real implementation, this would create the item in your data store
                var response = new CreateWorkloadItemResponse
                {
                    Success = true,
                    ItemId = itemId,
                    ItemType = itemType,
                    DisplayName = request.DisplayName ?? "New Workload Item",
                    Description = request.Description,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "User", // You might want to get this from authentication context
                    Message = "Item created successfully",
                    Metadata = new Dictionary<string, object>
                    {
                        { "displayName", request.DisplayName ?? "New Workload Item" },
                        { "description", request.Description ?? "" },
                        { "itemType", itemType },
                        { "workspaceId", workspaceId },
                        { "createdAt", DateTime.UtcNow.ToString("O") }
                    }
                };

                _logger.LogInformation("Successfully created workload item with ID {ItemId}", itemId);
                return Task.FromResult(ServiceResponse<CreateWorkloadItemResponse>.Success(response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating workload item {ItemType} in workspace {WorkspaceId}",
                    itemType, workspaceId);
                return Task.FromResult(ServiceResponse<CreateWorkloadItemResponse>.Error(
                    HttpStatusCode.InternalServerError,
                    $"Error creating workload item: {ex.Message}"));
            }
        }

        public Task<ServiceResponse<UpdateWorkloadItemResponse>> UpdateWorkloadItemAsync(
            string workspaceId, string itemType, string itemId, UpdateWorkloadItemRequest request)
        {
            try
            {
                _logger.LogInformation("Updating workload item {ItemType} {ItemId} in workspace {WorkspaceId}",
                    itemType, itemId, workspaceId);

                // For now, return a success response since the original function returns empty object
                var response = new UpdateWorkloadItemResponse
                {
                    Success = true,
                    ItemId = itemId,
                    Message = "Item updated successfully",
                    UpdatedProperties = request.Properties ?? new Dictionary<string, object>()
                };

                return Task.FromResult(ServiceResponse<UpdateWorkloadItemResponse>.Success(response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating workload item {ItemType} {ItemId} in workspace {WorkspaceId}",
                    itemType, itemId, workspaceId);
                return Task.FromResult(ServiceResponse<UpdateWorkloadItemResponse>.Error(
                    HttpStatusCode.InternalServerError,
                    $"Error updating workload item: {ex.Message}"));
            }
        }

        public async Task<ServiceResponse<WorkloadProxyResponse>> ProxyToFabricApiAsync(
            string path, string method, ClientPrincipal? principal = null, Dictionary<string, string>? headers = null, string? body = null, Dictionary<string, string>? queryParams = null)
        {
            try
            {
                _logger.LogInformation("Proxying {Method} request to Fabric API: {Path}", method, path);

                // Get access token for Fabric API
                string? accessToken = null;
                if (principal != null)
                {
                    accessToken = await _authenticationService.GetFabricAccessTokenAsync(principal);
                    if (string.IsNullOrEmpty(accessToken))
                    {
                        _logger.LogWarning("Failed to acquire access token for Fabric API");
                        return ServiceResponse<WorkloadProxyResponse>.Error(
                            HttpStatusCode.Unauthorized,
                            "Failed to acquire access token for Fabric API");
                    }
                    _logger.LogInformation("Successfully acquired access token for Fabric API");
                }
                else
                {
                    _logger.LogWarning("No client principal provided - proceeding without authentication");
                }

                // Build the full URL
                var url = $"{_fabricApiBaseUrl}{path}";

                // Add query parameters if provided
                if (queryParams?.Any() == true)
                {
                    var queryString = string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
                    url += $"?{queryString}";
                }

                _logger.LogInformation("Making request to URL: {Url}", url);

                // Create the HTTP request
                var request = new HttpRequestMessage(new HttpMethod(method.ToUpper()), url);

                // Add headers (filter out problematic headers)
                if (headers != null)
                {
                    var filteredHeaders = headers
                        .Where(h => !string.Equals(h.Key, "expect", StringComparison.OrdinalIgnoreCase) &&
                                   !string.Equals(h.Key, "content-length", StringComparison.OrdinalIgnoreCase))
                        .ToDictionary(h => h.Key, h => h.Value);

                    _logger.LogDebug("Adding {HeaderCount} headers to request", filteredHeaders.Count);

                    foreach (var header in filteredHeaders)
                    {
                        try
                        {
                            if (string.Equals(header.Key, "content-type", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(body))
                            {
                                // Content-Type will be set with the StringContent
                                continue;
                            }
                            request.Headers.Add(header.Key, header.Value);
                        }
                        catch (Exception headerEx)
                        {
                            _logger.LogWarning(headerEx, "Failed to add header {HeaderName}: {HeaderValue}", header.Key, header.Value);
                        }
                    }
                }

                // Add Authorization header with access token
                if (!string.IsNullOrEmpty(accessToken))
                {
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                    _logger.LogDebug("Added Authorization header for Fabric API request");
                }

                // Add body if provided
                if (!string.IsNullOrEmpty(body) && (method.ToUpper() == "POST" || method.ToUpper() == "PUT" || method.ToUpper() == "PATCH"))
                {
                    request.Content = new StringContent(body, Encoding.UTF8, "application/json");
                    _logger.LogDebug("Added request body with content type: application/json");
                }

                _logger.LogInformation("Sending {Method} request to Fabric API", method);

                // Send the request
                var response = await _httpClient.SendAsync(request);

                _logger.LogInformation("Received response from Fabric API: {StatusCode}", response.StatusCode);

                // Read response
                var responseBody = await response.Content.ReadAsStringAsync();
                var responseHeaders = response.Headers
                    .Concat(response.Content.Headers)
                    .ToDictionary(h => h.Key, h => string.Join(", ", h.Value));

                var proxyResponse = new WorkloadProxyResponse
                {
                    StatusCode = (int)response.StatusCode,
                    Headers = responseHeaders,
                    Body = responseBody,
                    IsSuccess = response.IsSuccessStatusCode
                };

                if (!response.IsSuccessStatusCode)
                {
                    proxyResponse.ErrorMessage = $"Fabric API returned {response.StatusCode}: {response.ReasonPhrase}";
                    _logger.LogWarning("Fabric API returned error: {StatusCode} - {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
                }

                return ServiceResponse<WorkloadProxyResponse>.Success(proxyResponse);
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP error while proxying to Fabric API: {Path}. Full exception: {FullException}", path, httpEx);
                return ServiceResponse<WorkloadProxyResponse>.Error(
                    HttpStatusCode.BadGateway,
                    $"Error communicating with Fabric API: {httpEx.Message}");
            }
            catch (TaskCanceledException tcEx)
            {
                _logger.LogError(tcEx, "Request timeout while proxying to Fabric API: {Path}", path);
                return ServiceResponse<WorkloadProxyResponse>.Error(
                    HttpStatusCode.RequestTimeout,
                    $"Request to Fabric API timed out: {tcEx.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error proxying request to Fabric API: {Path}. Full exception: {FullException}", path, ex);
                return ServiceResponse<WorkloadProxyResponse>.Error(
                    HttpStatusCode.InternalServerError,
                    $"Error proxying to Fabric API: {ex.Message}");
            }
        }
    }
}