using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PowerBITips.Api.Core.Interfaces;
using PowerBITips.Api.Extensions;
using PowerBITips.Api.Models.DTOs.Common;
using PowerBITips.Api.Models.DTOs.Requests;
using PowerBITips.Api.Models.DTOs.Responses;
using PowerBITips.Api.Middleware;
using PowerBITips.Api.Models.Workload;
using PowerBITips.Api.Services.Interfaces;
using PowerBITips.Api.Utilities.Helpers;
using System.Net;

namespace PowerBITips.Api.Controllers;

public class WorkloadController
{
    private readonly IWorkloadService _workloadService;
    private readonly IAuthenticationService _authenticationService;
    private readonly ILogger<WorkloadController> _logger;

    public WorkloadController(
        IWorkloadService workloadService,
        IAuthenticationService authenticationService,
        ILogger<WorkloadController> logger)
    {
        _workloadService = workloadService;
        _authenticationService = authenticationService;
        _logger = logger;
    }

    [Function("GetWorkloadInfo")]
    public async Task<HttpResponseData> GetWorkloadInfo(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "workload")] HttpRequestData req)
    {
        try
        {
            // Build and validate request DTO from query parameters
            var request = new WorkloadInfoRequest
            {
                WorkspaceId = req.Query["workspaceId"] ?? string.Empty,
                IncludeFields = req.Query["includeFields"],
                ItemType = req.Query["itemType"],
                ContinuationToken = req.Query["continuationToken"]
            };

            if (int.TryParse(req.Query["limit"], out var limit))
            {
                request.Limit = limit;
            }

            // Validate the request DTO
            var (isValid, validationErrors) = ValidationHelper.TryValidateDto(request);
            if (!isValid)
            {
                var validationErrorResponse = new ErrorResponseDto("Invalid request parameters")
                {
                    ValidationErrors = validationErrors
                };

                return await req.CreateStandardResponseAsync<object>(success: false, data: null, errorMessage: "Validation failed", statusCode: HttpStatusCode.BadRequest);
            }

            _logger.LogInformation("Getting workload info for workspace {WorkspaceId}",
                request.WorkspaceId);

            var result = await _workloadService.GetWorkloadInfoAsync(request.WorkspaceId);

            if (result.IsSuccess && result.Data != null)
            {
                return await req.CreateStandardResponseAsync(success: true, data: result.Data, errorMessage: null, statusCode: HttpStatusCode.OK);
            }

            var errorResponseDto = new ErrorResponseDto(result.ErrorMessage ?? "Failed to get workload info");
            var errorResponse = req.CreateResponse(result.Status);
            await errorResponse.WriteAsJsonAsync(errorResponseDto);
            return errorResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetWorkloadInfo");
            var errorResponseDto = new ErrorResponseDto("An unexpected error occurred");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteAsJsonAsync(errorResponseDto);
            return errorResponse;
        }
    }

    [Function("GetWorkloadItemPayload")]
    public async Task<HttpResponseData> GetItemPayload(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "workload/workspaces/{workspaceId}/items/{itemType}/{itemId}/payload")]
            HttpRequestData req, string workspaceId, string itemType, string itemId)
    {
        try
        {
            // Build and validate request DTO from URL and query parameters
            var request = new WorkloadItemPayloadRequest
            {
                WorkspaceId = workspaceId,
                ItemType = itemType,
                ItemId = itemId,
                Version = req.Query["version"],
                Format = req.Query["format"] ?? "json"
            };

            if (bool.TryParse(req.Query["includeMetadata"], out var includeMetadata))
            {
                request.IncludeMetadata = includeMetadata;
            }

            // Validate the request DTO
            var (isValid, validationErrors) = ValidationHelper.TryValidateDto(request);
            if (!isValid)
            {
                var validationErrorResponse = new ErrorResponseDto("Invalid request parameters")
                {
                    ValidationErrors = validationErrors
                };

                return await req.CreateStandardResponseAsync<object>(success: false, data: null, errorMessage: "Validation failed", statusCode: HttpStatusCode.BadRequest);
            }

            _logger.LogInformation("Getting item payload for {ItemType} {ItemId} in workspace {WorkspaceId}",
                request.ItemType, request.ItemId, request.WorkspaceId);

            var result = await _workloadService.GetItemPayloadAsync(request.WorkspaceId, request.ItemType, request.ItemId);

            if (result.IsSuccess && result.Data != null)
            {
                return await req.CreateStandardResponseAsync(success: true, data: result.Data, errorMessage: null, statusCode: HttpStatusCode.OK);
            }

            var errorResponseDto = new ErrorResponseDto(result.ErrorMessage ?? "Failed to get item payload");
            var errorResponse = req.CreateResponse(result.Status);
            await errorResponse.WriteAsJsonAsync(errorResponseDto);
            return errorResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetItemPayload");
            var errorResponseDto = new ErrorResponseDto("An unexpected error occurred");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteAsJsonAsync(errorResponseDto);
            return errorResponse;
        }
    }

    [Function("UpdateWorkloadItem")]
    public async Task<HttpResponseData> UpdateWorkloadItem(
        [HttpTrigger(AuthorizationLevel.Anonymous, "patch", "put", Route = "workload/workspaces/{workspaceId}/items/{itemType}/{itemId}")]
            HttpRequestData req, string workspaceId, string itemType, string itemId)
    {
        try
        {
            // Parse and validate request body
            var body = await req.ReadAsStringAsync();
            PowerBITips.Api.Models.DTOs.Requests.UpdateWorkloadItemRequest? request = null;

            if (!string.IsNullOrEmpty(body))
            {
                try
                {
                    request = JsonConvert.DeserializeObject<PowerBITips.Api.Models.DTOs.Requests.UpdateWorkloadItemRequest>(body);
                }
                catch (JsonException)
                {
                    var jsonError = new ErrorResponseDto("Invalid JSON in request body");
                    return await req.CreateStandardResponseAsync<object>(success: false, data: null, errorMessage: "Validation failed", statusCode: HttpStatusCode.BadRequest);
                }
            }

            request ??= new PowerBITips.Api.Models.DTOs.Requests.UpdateWorkloadItemRequest();

            // Validate the request DTO
            var (isValid, validationErrors) = ValidationHelper.TryValidateDto(request);
            if (!isValid)
            {
                var validationErrorResponse = new ErrorResponseDto("Invalid request parameters")
                {
                    ValidationErrors = validationErrors
                };

                return await req.CreateStandardResponseAsync<object>(success: false, data: null, errorMessage: "Validation failed", statusCode: HttpStatusCode.BadRequest);
            }

            // Map DTO to service model
            var serviceRequest = new PowerBITips.Api.Models.Workload.UpdateWorkloadItemRequest
            {
                WorkspaceId = workspaceId,
                ItemType = itemType,
                ItemId = itemId,
                Properties = request.Properties
            };

            _logger.LogInformation("Updating workload item {ItemType} {ItemId} in workspace {WorkspaceId}",
                itemType, itemId, workspaceId);

            var result = await _workloadService.UpdateWorkloadItemAsync(workspaceId, itemType, itemId, serviceRequest);

            if (result.IsSuccess && result.Data != null)
            {
                return await req.CreateStandardResponseAsync(success: true, data: result.Data, errorMessage: null, statusCode: HttpStatusCode.OK);
            }

            var errorResponseDto = new ErrorResponseDto(result.ErrorMessage ?? "Failed to update workload item");
            var errorResponse = req.CreateResponse(result.Status);
            await errorResponse.WriteAsJsonAsync(errorResponseDto);
            return errorResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in UpdateWorkloadItem");
            var errorResponseDto = new ErrorResponseDto("An unexpected error occurred");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteAsJsonAsync(errorResponseDto);
            return errorResponse;
        }
    }

    [Function("CreateWorkloadItem")]
    public async Task<HttpResponseData> CreateWorkloadItem(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "workload/workspaces/{workspaceId}/items/{itemType}")]
            HttpRequestData req, string workspaceId, string itemType)
    {
        try
        {
            // Read the request body for logging purposes
            var body = await req.ReadAsStringAsync();

            _logger.LogInformation("Microsoft Fabric CreateItem request received - WorkspaceId: {WorkspaceId}, ItemType: {ItemType}, Body: {Body}",
                workspaceId, itemType, body);

            // Simply acknowledge the request - we handle item creation separately when themes are saved
            // This matches the behavior of the original Node API which just returned HttpResponse.ok({})
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new { });
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error acknowledging CreateWorkloadItem request for WorkspaceId: {WorkspaceId}, ItemType: {ItemType}", workspaceId, itemType);
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new { });
            return response;
        }
    }

    [Function("WorkloadProxy")]
    public async Task<HttpResponseData> WorkloadProxy(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", "put", "patch", "delete", "options", Route = "workload/{*route}")]
            HttpRequestData req, string route)
    {
        try
        {
            _logger.LogInformation("Proxying {Method} request to Fabric API: /{Route}",
                req.Method, route);

            // Check if this is a CreateItem request that we should handle locally
            if (req.Method.Equals("POST", StringComparison.OrdinalIgnoreCase) && 
                route.Contains("workspaces/") && 
                route.Contains("/items/") &&
                !route.Contains("/payload"))
            {
                _logger.LogInformation("Intercepting CreateItem request locally instead of proxying to Fabric API: {Route}", route);
                
                // Read the request body for logging purposes
                var requestBody = await req.ReadAsStringAsync();
                
                _logger.LogInformation("Microsoft Fabric CreateItem request received via proxy - Route: {Route}, Body: {Body}",
                    route, requestBody);

                // Simply acknowledge the request - we handle item creation separately when themes are saved
                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(new { });
                return response;
            }

            // Extract client principal from Static Web Apps header
            var principal = _authenticationService.GetClientPrincipal(req);
            if (principal != null)
            {
                _logger.LogInformation("Found authenticated user: {UserId}", principal.UserId);
            }
            else
            {
                _logger.LogWarning("No authenticated user found in request");
            }

            // Extract headers (convert from HttpRequestData headers to Dictionary)
            var headers = req.Headers.ToDictionary(h => h.Key, h => string.Join(", ", h.Value));

            // Extract query parameters
            var queryParams = new Dictionary<string, string>();
            if (req.Query.AllKeys != null)
            {
                foreach (var key in req.Query.AllKeys)
                {
                    if (!string.IsNullOrEmpty(key))
                    {
                        queryParams[key] = req.Query[key] ?? "";
                    }
                }
            }

            // Read request body
            string? body = null;
            if (req.Body.CanRead && (req.Method.Equals("POST", StringComparison.OrdinalIgnoreCase) ||
                                    req.Method.Equals("PUT", StringComparison.OrdinalIgnoreCase) ||
                                    req.Method.Equals("PATCH", StringComparison.OrdinalIgnoreCase)))
            {
                body = await req.ReadAsStringAsync();
            }

            // Construct the path for the Fabric API
            var fabricPath = $"/{route}";

            var result = await _workloadService.ProxyToFabricApiAsync(fabricPath, req.Method, principal, headers, body, queryParams);

            if (result.IsSuccess && result.Data != null)
            {
                var proxyData = result.Data;
                var response = req.CreateResponse((HttpStatusCode)proxyData.StatusCode);

                // Set response headers from Fabric API
                if (proxyData.Headers != null)
                {
                    foreach (var header in proxyData.Headers)
                    {
                        try
                        {
                            response.Headers.Add(header.Key, header.Value);
                        }
                        catch (Exception headerEx)
                        {
                            _logger.LogWarning(headerEx, "Failed to set response header {HeaderName}: {HeaderValue}", header.Key, header.Value);
                        }
                    }
                }

                // Write response body
                if (!string.IsNullOrEmpty(proxyData.Body))
                {
                    await response.WriteStringAsync(proxyData.Body);
                }

                return response;
            }

            var errorResponseDto = new ErrorResponseDto(result.ErrorMessage ?? "Failed to proxy request to Fabric API");
            var errorResponse = req.CreateResponse(result.Status);
            await errorResponse.WriteAsJsonAsync(errorResponseDto);
            return errorResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in WorkloadProxy for route: {Route}", route);
            var errorResponseDto = new ErrorResponseDto("An unexpected error occurred");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteAsJsonAsync(errorResponseDto);
            return errorResponse;
        }
    }


}