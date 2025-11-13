using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using PowerBITips.Api.Extensions;
using PowerBITips.Api.Models.Constants;
using PowerBITips.Api.Models.DTOs.Requests;
using PowerBITips.Api.Models.DTOs.Responses;
using PowerBITips.Api.Models.DTOs.Extensions;
using PowerBITips.Api.Models.DTOs.Common;
using PowerBITips.Api.Services.Interfaces;
using PowerBITips.Api.Utilities.Helpers;
using PowerBITips.Api.Middleware;
using System.Net;
using System.Text.Json;
using System.Web;

namespace PowerBITips.Api.Controllers;

public class PublishedController
{
    private readonly IPublishedService _publishedService;
    private readonly ILogger<PublishedController> _logger;

    public PublishedController(
        IPublishedService publishedService,
        ILogger<PublishedController> logger)
    {
        _publishedService = publishedService;
        _logger = logger;
    }

    /// <summary>
    /// Gets published items with pagination support
    /// Supports both paginated results and table source via query parameter
    /// </summary>
    [Function("GetPublishedItems")]
    public async Task<HttpResponseData> GetPublishedItems(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RouteConstants.Published)] HttpRequestData req)
    {
        _logger.LogInformation("Processing GetPublishedItems request");

        try
        {
            // Parse query parameters
            var queryParams = System.Web.HttpUtility.ParseQueryString(req.Url.Query);

            // Check if this is a "from table" request (legacy endpoint consolidation)
            var source = queryParams["source"];
            if (source?.ToLowerInvariant() == "table")
            {
                _logger.LogInformation("Processing GetPublishedItemsFromTable request (via source=table parameter)");

                // Handle the table source request (no pagination)
                var tableRequest = new GetPublishedItemsFromTableRequest();
                if (!string.IsNullOrEmpty(queryParams["itemTypes"]))
                {
                    tableRequest.ItemTypes = queryParams["itemTypes"]!.Split(',', StringSplitOptions.RemoveEmptyEntries);
                }

                // Validate the constructed request DTO
                var (isValid, validationErrors) = ValidationHelper.TryValidateDto(tableRequest);
                if (!isValid)
                {
                    var validationErrorResponse = new ErrorResponseDto("Invalid request parameters")
                    {
                        ValidationErrors = validationErrors
                    };

                    return await req.CreateStandardResponseAsync<object>(success: false, data: null, errorMessage: "Validation failed", statusCode: HttpStatusCode.BadRequest);
                }

                // Get published items from table
                var tableResult = await _publishedService.GetPublishedItemsFromTableAsync(tableRequest);

                // Create response - ensure it matches the expected structure
                var tableResponse = req.CreateResponse(HttpStatusCode.OK);
                tableResponse.Headers.Add("Content-Type", "application/json; charset=utf-8");
                await tableResponse.WriteStringAsync(JsonSerializer.Serialize(tableResult, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }));
                return tableResponse;
            }

            // Standard paginated request
            // Parse and validate query parameters
            var request = new GetPublishedItemsRequest();

            if (!string.IsNullOrEmpty(queryParams["itemTypes"]))
            {
                request.ItemTypes = queryParams["itemTypes"]!.Split(',', StringSplitOptions.RemoveEmptyEntries);
            }

            if (!string.IsNullOrEmpty(queryParams["page"]) && int.TryParse(queryParams["page"], out var page))
            {
                request.Page = Math.Max(1, page);
            }

            if (!string.IsNullOrEmpty(queryParams["pageSize"]) && int.TryParse(queryParams["pageSize"], out var pageSize))
            {
                request.PageSize = Math.Min(Math.Max(1, pageSize), 100); // Cap at 100 items
            }

            if (!string.IsNullOrEmpty(queryParams["search"]))
            {
                request.Search = queryParams["search"]!;
            }

            if (!string.IsNullOrEmpty(queryParams["sortBy"]))
            {
                request.SortBy = queryParams["sortBy"]!;
            }

            if (!string.IsNullOrEmpty(queryParams["sortOrder"]))
            {
                request.SortOrder = queryParams["sortOrder"]!;
            }

            // Validate the constructed request DTO
            var (isValidPaginated, validationErrorsPaginated) = ValidationHelper.TryValidateDto(request);
            if (!isValidPaginated)
            {
                var validationErrorResponse = new ErrorResponseDto("Invalid request parameters")
                {
                    ValidationErrors = validationErrorsPaginated
                };

                return await req.CreateStandardResponseAsync<object>(success: false, data: null, errorMessage: "Validation failed", statusCode: HttpStatusCode.BadRequest);
            }

            // Get continuation token from header
            string? continuationToken = null;
            if (req.Headers.TryGetValues("x-continuation-token", out var tokenValues))
            {
                continuationToken = tokenValues.FirstOrDefault();
            }

            // Get published items
            var result = await _publishedService.GetPublishedItemsAsync(request, continuationToken);

            // Create response
            return await req.CreateStandardResponseAsync(success: true, data: result, errorMessage: null, statusCode: HttpStatusCode.OK);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting published items");
            return await CreateErrorResponse(req, HttpStatusCode.InternalServerError, "Internal server error", ex.Message);
        }
    }

    /// <summary>
    /// Gets all published items from table without pagination
    /// </summary>
    [Function("GetPublishedItemsFromTable")]
    public async Task<HttpResponseData> GetPublishedItemsFromTable(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RouteConstants.PublishedItemsFromTable)] HttpRequestData req)
    {
        _logger.LogInformation("Processing GetPublishedItemsFromTable request");

        try
        {
            // Parse and validate query parameters
            var request = new GetPublishedItemsFromTableRequest();
            var queryParams = System.Web.HttpUtility.ParseQueryString(req.Url.Query);

            if (!string.IsNullOrEmpty(queryParams["itemTypes"]))
            {
                request.ItemTypes = queryParams["itemTypes"]!.Split(',', StringSplitOptions.RemoveEmptyEntries);
            }

            // Validate the constructed request DTO
            var (isValid, validationErrors) = ValidationHelper.TryValidateDto(request);
            if (!isValid)
            {
                var validationErrorResponse = new ErrorResponseDto("Invalid request parameters")
                {
                    ValidationErrors = validationErrors
                };

                return await req.CreateStandardResponseAsync<object>(success: false, data: null, errorMessage: "Validation failed", statusCode: HttpStatusCode.BadRequest);
            }

            // Get published items
            var result = await _publishedService.GetPublishedItemsFromTableAsync(request);

            // Create response
            return await req.CreateStandardResponseAsync(success: true, data: result, errorMessage: null, statusCode: HttpStatusCode.OK);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting published items from table");
            return await CreateErrorResponse(req, HttpStatusCode.InternalServerError, "Internal server error", ex.Message);
        }
    }

    /// <summary>
    /// Publishes a new item
    /// </summary>
    [Function("PublishItem")]
    public async Task<HttpResponseData> PublishItem(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RouteConstants.Published)] HttpRequestData req)
    {
        _logger.LogInformation("Processing PublishItem request");

        try
        {
            // Get client principal for authorization
            var clientPrincipal = StaticWebAppsAuth.Parse(req);
            var isLocalEnvironment = Environment.GetEnvironmentVariable("APP_ENVIRONMENT") == "local";
            var bypassAuth = Environment.GetEnvironmentVariable("BYPASS_AUTH") == "true";

            // Check if user has valid authentication first
            if (!isLocalEnvironment && !bypassAuth && (clientPrincipal == null || string.IsNullOrEmpty(clientPrincipal.UserId)))
            {
                _logger.LogWarning("PublishItem request without valid authentication");
                return await CreateErrorResponse(req, HttpStatusCode.Unauthorized, "Authentication required");
            }

            // For local development, use default values if auth is bypassed
            var userId = clientPrincipal?.UserId ?? "default-user-id";
            var identityProvider = clientPrincipal?.IdentityProvider ?? "default-provider";

            // Parse and validate request body
            string requestBody = await req.ReadAsStringAsync() ?? string.Empty;

            if (string.IsNullOrEmpty(requestBody))
            {
                var emptyBodyErrorResponse = new ErrorResponseDto("Request body is required");
                var emptyBodyResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                emptyBodyResponse.Headers.Add("Content-Type", "application/json; charset=utf-8");
                await emptyBodyResponse.WriteStringAsync(JsonSerializer.Serialize(emptyBodyErrorResponse, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }));
                return emptyBodyResponse;
            }

            PublishItemRequest? request;
            try
            {
                request = JsonSerializer.Deserialize<PublishItemRequest>(requestBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Invalid JSON in publish item request");
                var jsonErrorResponse = new ErrorResponseDto("Invalid JSON format")
                {
                    ErrorDetails = new Dictionary<string, object> { { "JsonError", ex.Message } }
                };
                var jsonErrorHttpResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                jsonErrorHttpResponse.Headers.Add("Content-Type", "application/json; charset=utf-8");
                await jsonErrorHttpResponse.WriteStringAsync(JsonSerializer.Serialize(jsonErrorResponse, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }));
                return jsonErrorHttpResponse;
            }

            if (request == null)
            {
                var nullRequestErrorResponse = new ErrorResponseDto("Invalid request format");
                var nullRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                nullRequestResponse.Headers.Add("Content-Type", "application/json; charset=utf-8");
                await nullRequestResponse.WriteStringAsync(JsonSerializer.Serialize(nullRequestErrorResponse, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }));
                return nullRequestResponse;
            }

            // Validate the request DTO
            var (isValid, validationErrors) = ValidationHelper.TryValidateDto(request);
            if (!isValid)
            {
                var validationErrorResponse = new ErrorResponseDto("Invalid request data")
                {
                    ValidationErrors = validationErrors
                };

                return await req.CreateStandardResponseAsync<object>(success: false, data: null, errorMessage: "Validation failed", statusCode: HttpStatusCode.BadRequest);
            }

            // Publish the item
            var result = await _publishedService.PublishItemAsync(request, userId, identityProvider);

            if (!result.Success)
            {
                var errorResponse = new ErrorResponseDto(result.Message);
                var errorHttpResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                errorHttpResponse.Headers.Add("Content-Type", "application/json; charset=utf-8");
                await errorHttpResponse.WriteStringAsync(JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }));
                return errorHttpResponse;
            }

            // Return the proper DTO response
            return await req.CreateStandardResponseAsync(success: true, data: result, errorMessage: null, statusCode: HttpStatusCode.OK);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing item");
            return await CreateErrorResponse(req, HttpStatusCode.InternalServerError, "Internal server error", ex.Message);
        }
    }

    /// <summary>
    /// Deletes a published item (owner or admin only)
    /// </summary>
    [Function("DeletePublishedItem")]
    public async Task<HttpResponseData> DeletePublishedItem(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = RouteConstants.PublishedById)] HttpRequestData req,
        string itemId)
    {
        _logger.LogInformation("Processing DeletePublishedItem request for item: {ItemId}", itemId);

        try
        {
            // Validate itemId parameter
            if (string.IsNullOrWhiteSpace(itemId))
            {
                var validationErrorResponse = new ErrorResponseDto("Item ID is required")
                {
                    Details = "The itemId parameter cannot be null or empty"
                };
                return await req.CreateStandardResponseAsync<object>(success: false, data: null, errorMessage: "Validation failed", statusCode: HttpStatusCode.BadRequest);
            }

            // Get client principal for authorization
            var clientPrincipal = StaticWebAppsAuth.Parse(req);
            var isLocalEnvironment = Environment.GetEnvironmentVariable("APP_ENVIRONMENT") == "local";
            var bypassAuth = Environment.GetEnvironmentVariable("BYPASS_AUTH") == "true";

            // Check if user has valid authentication first
            if (!isLocalEnvironment && !bypassAuth && (clientPrincipal == null || string.IsNullOrEmpty(clientPrincipal.UserId)))
            {
                _logger.LogWarning("DeletePublishedItem request without valid authentication");
                return await CreateErrorResponse(req, HttpStatusCode.Unauthorized, "Authentication required");
            }

            // For local development, use default values if auth is bypassed
            var userId = clientPrincipal?.UserId ?? "default-user-id";
            var identityProvider = clientPrincipal?.IdentityProvider ?? "default-provider";

            // Create request object from path parameter
            var request = new DeletePublishedItemRequest
            {
                ItemId = itemId
            };

            // Validate the request DTO
            var (isValid, validationErrors) = ValidationHelper.TryValidateDto(request);
            if (!isValid)
            {
                var validationErrorResponse = new ErrorResponseDto("Invalid request data")
                {
                    ValidationErrors = validationErrors
                };

                return await req.CreateStandardResponseAsync<object>(success: false, data: null, errorMessage: "Validation failed", statusCode: HttpStatusCode.BadRequest);
            }

            // Delete the item
            var result = await _publishedService.DeletePublishedItemAsync(request, userId, identityProvider);

            if (!result.Success)
            {
                var errorResponse = new ErrorResponseDto(result.Message);
                var errorHttpResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                errorHttpResponse.Headers.Add("Content-Type", "application/json; charset=utf-8");
                await errorHttpResponse.WriteStringAsync(JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }));
                return errorHttpResponse;
            }

            // Return the proper DTO response
            return await req.CreateStandardResponseAsync(success: true, data: result, errorMessage: null, statusCode: HttpStatusCode.OK);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting published item");
            return await CreateErrorResponse(req, HttpStatusCode.InternalServerError, "Internal server error", ex.Message);
        }
    }

    private async Task<HttpResponseData> CreateErrorResponse(HttpRequestData req, HttpStatusCode statusCode, string error, string? message = null)
    {
        var response = req.CreateResponse(statusCode);
        var errorResponse = new ErrorResponseDto(error)
        {
            Details = message
        };
        response.Headers.Add("Content-Type", "application/json; charset=utf-8");
        await response.WriteStringAsync(JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
        return response;
    }

}