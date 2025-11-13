using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using PowerBITips.Api.Core;
using PowerBITips.Api.Extensions;
using PowerBITips.Api.Middleware;
using PowerBITips.Api.Models.Analytics;
using PowerBITips.Api.Models.Constants;
using PowerBITips.Api.Models.DTOs.Common;
using PowerBITips.Api.Models.DTOs.Responses;
using PowerBITips.Api.Services;
using System.Net;
using System.Text.Json;

namespace PowerBITips.Api.Controllers;

public class AnalyticsController
{
    private readonly IAnalyticsManagementService _analyticsManagementService;
    private readonly ILogger<AnalyticsController> _logger;

    public AnalyticsController(
        IAnalyticsManagementService analyticsManagementService,
        ILogger<AnalyticsController> logger)
    {
        _analyticsManagementService = analyticsManagementService;
        _logger = logger;
    }

    [Function("PostGoogleAnalytics")]
    public async Task<HttpResponseData> PostGoogleAnalytics(
    [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RouteConstants.AnalyticsEventsGoogle)] HttpRequestData req)
    {
        _logger.LogInformation("Processing PostGoogleAnalytics request");

        try
        {
            // Read and deserialize request body
            var body = await req.ReadAsStringAsync();
            if (string.IsNullOrEmpty(body))
            {
                _logger.LogWarning("Request body is empty");
                var errorResponse = ValidationHelper.CreateErrorResponse(
                    "Request body is required",
                    "EMPTY_REQUEST");

                return await req.CreateStandardResponseAsync<object>(success: false, data: null, errorMessage: "Validation failed", statusCode: HttpStatusCode.BadRequest);
            }

            PostGoogleAnalyticsRequest? request;
            try
            {
                request = JsonSerializer.Deserialize<PostGoogleAnalyticsRequest>(body, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Invalid JSON in request body");
                var errorResponse = ValidationHelper.CreateErrorResponse(
                    "Invalid JSON format",
                    "JSON_PARSE_ERROR",
                    ex);

                return await req.CreateStandardResponseAsync<object>(success: false, data: null, errorMessage: "Validation failed", statusCode: HttpStatusCode.BadRequest);
            }

            if (request == null)
            {
                var errorResponse = ValidationHelper.CreateErrorResponse(
                    "Invalid request body",
                    "NULL_REQUEST");

                return await req.CreateStandardResponseAsync<object>(success: false, data: null, errorMessage: "Validation failed", statusCode: HttpStatusCode.BadRequest);
            }

            // Validate the DTO
            var (isValid, validationErrors) = ValidationHelper.TryValidateDto(request);
            if (!isValid)
            {
                var validationErrorResponse = ValidationHelper.CreateValidationErrorResponse(validationErrors);
                return await req.CreateStandardResponseAsync<object>(success: false, data: null, errorMessage: "Validation failed", statusCode: HttpStatusCode.BadRequest);
            }

            if (request?.EventData == null)
            {
                _logger.LogWarning("EventData is missing from request");
                var errorResponse = ValidationHelper.CreateErrorResponse(
                    "EventData is required",
                    "MISSING_EVENT_DATA");

                return await req.CreateStandardResponseAsync<object>(success: false, data: null, errorMessage: "Validation failed", statusCode: HttpStatusCode.BadRequest);
            }

            var result = await _analyticsManagementService.PostGoogleAnalyticsAsync(request);

            var response = req.CreateResponse(result.Success ? HttpStatusCode.OK : HttpStatusCode.BadRequest);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");
            await response.WriteStringAsync(JsonSerializer.Serialize(result, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));

            _logger.LogInformation("Successfully processed Google Analytics request with result: {Success}", result.Success);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing PostGoogleAnalytics request");

            var errorResponse = ValidationHelper.CreateErrorResponse(
                "Internal server error occurred",
                "INTERNAL_ERROR",
                ex);

            return await req.CreateStandardResponseAsync<object>(success: false, data: null, errorMessage: "Internal server error", statusCode: HttpStatusCode.InternalServerError);
        }
    }

    [Function("TrackEvent")]
    public async Task<HttpResponseData> TrackEvent(
    [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RouteConstants.AnalyticsEventsCustom)] HttpRequestData req)
    {
        _logger.LogInformation("Processing TrackEvent request");

        try
        {
            // Read and deserialize request body
            var body = await req.ReadAsStringAsync();
            if (string.IsNullOrEmpty(body))
            {
                _logger.LogWarning("Request body is empty");
                var errorResponse = ValidationHelper.CreateErrorResponse(
                    "Request body is required",
                    "EMPTY_REQUEST");

                return await req.CreateStandardResponseAsync<object>(success: false, data: null, errorMessage: "Validation failed", statusCode: HttpStatusCode.BadRequest);
            }

            EventTrackingRequest? request;
            try
            {
                request = JsonSerializer.Deserialize<EventTrackingRequest>(body, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Invalid JSON in request body");
                var errorResponse = ValidationHelper.CreateErrorResponse(
                    "Invalid JSON format",
                    "JSON_PARSE_ERROR",
                    ex);

                return await req.CreateStandardResponseAsync<object>(success: false, data: null, errorMessage: "Validation failed", statusCode: HttpStatusCode.BadRequest);
            }

            if (request == null)
            {
                var errorResponse = ValidationHelper.CreateErrorResponse(
                    "Invalid request body",
                    "NULL_REQUEST");

                return await req.CreateStandardResponseAsync<object>(success: false, data: null, errorMessage: "Validation failed", statusCode: HttpStatusCode.BadRequest);
            }

            // Validate the DTO
            var (isValid, validationErrors) = ValidationHelper.TryValidateDto(request);
            if (!isValid)
            {
                var validationErrorResponse = ValidationHelper.CreateValidationErrorResponse(validationErrors);
                return await req.CreateStandardResponseAsync<object>(success: false, data: null, errorMessage: "Validation failed", statusCode: HttpStatusCode.BadRequest);
            }

            if (string.IsNullOrEmpty(request.Action) || string.IsNullOrEmpty(request.Category))
            {
                _logger.LogWarning("Action and Category are required for event tracking");
                var errorResponse = ValidationHelper.CreateErrorResponse(
                    "Action and Category are required",
                    "MISSING_REQUIRED_FIELDS");

                return await req.CreateStandardResponseAsync<object>(success: false, data: null, errorMessage: "Validation failed", statusCode: HttpStatusCode.BadRequest);
            }

            var result = await _analyticsManagementService.TrackEventAsync(request);

            var response = req.CreateResponse(result.Success ? HttpStatusCode.OK : HttpStatusCode.BadRequest);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");
            await response.WriteStringAsync(JsonSerializer.Serialize(result, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));

            _logger.LogInformation("Successfully processed event tracking request with result: {Success}", result.Success);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing TrackEvent request");

            var errorResponse = ValidationHelper.CreateErrorResponse(
                "Internal server error occurred",
                "INTERNAL_ERROR",
                ex);

            return await req.CreateStandardResponseAsync<object>(success: false, data: null, errorMessage: "Internal server error", statusCode: HttpStatusCode.InternalServerError);
        }
    }

    [Function("TrackPageView")]
    public async Task<HttpResponseData> TrackPageView(
    [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RouteConstants.AnalyticsPageviews)] HttpRequestData req)
    {
        _logger.LogInformation("Processing TrackPageView request");

        try
        {
            // Read and deserialize request body
            var body = await req.ReadAsStringAsync();
            if (string.IsNullOrEmpty(body))
            {
                _logger.LogWarning("Request body is empty");
                var errorResponse = ValidationHelper.CreateErrorResponse(
                    "Request body is required",
                    "EMPTY_REQUEST");

                return await req.CreateStandardResponseAsync<object>(success: false, data: null, errorMessage: "Validation failed", statusCode: HttpStatusCode.BadRequest);
            }

            PageViewTrackingRequest? request;
            try
            {
                request = JsonSerializer.Deserialize<PageViewTrackingRequest>(body, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Invalid JSON in request body");
                var errorResponse = ValidationHelper.CreateErrorResponse(
                    "Invalid JSON format",
                    "JSON_PARSE_ERROR",
                    ex);

                return await req.CreateStandardResponseAsync<object>(success: false, data: null, errorMessage: "Validation failed", statusCode: HttpStatusCode.BadRequest);
            }

            if (request == null)
            {
                var errorResponse = ValidationHelper.CreateErrorResponse(
                    "Invalid request body",
                    "NULL_REQUEST");

                return await req.CreateStandardResponseAsync<object>(success: false, data: null, errorMessage: "Validation failed", statusCode: HttpStatusCode.BadRequest);
            }

            // Validate the DTO
            var (isValid, validationErrors) = ValidationHelper.TryValidateDto(request);
            if (!isValid)
            {
                var validationErrorResponse = ValidationHelper.CreateValidationErrorResponse(validationErrors);
                return await req.CreateStandardResponseAsync<object>(success: false, data: null, errorMessage: "Validation failed", statusCode: HttpStatusCode.BadRequest);
            }

            if (string.IsNullOrEmpty(request.Page))
            {
                _logger.LogWarning("Page is required for page view tracking");
                var errorResponse = ValidationHelper.CreateErrorResponse(
                    "Page is required",
                    "MISSING_REQUIRED_FIELD");

                return await req.CreateStandardResponseAsync<object>(success: false, data: null, errorMessage: "Validation failed", statusCode: HttpStatusCode.BadRequest);
            }

            var result = await _analyticsManagementService.TrackPageViewAsync(request);

            var response = req.CreateResponse(result.Success ? HttpStatusCode.OK : HttpStatusCode.BadRequest);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");
            await response.WriteStringAsync(JsonSerializer.Serialize(result, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));

            _logger.LogInformation("Successfully processed page view tracking request with result: {Success}", result.Success);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing TrackPageView request");

            var errorResponse = ValidationHelper.CreateErrorResponse(
                "Internal server error occurred",
                "INTERNAL_ERROR",
                ex);

            return await req.CreateStandardResponseAsync<object>(success: false, data: null, errorMessage: "Internal server error", statusCode: HttpStatusCode.InternalServerError);
        }
    }

    [Function("TrackLoginEvent")]
    public async Task<HttpResponseData> TrackLoginEvent(
    [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RouteConstants.AnalyticsEventsLogin)] HttpRequestData req)
    {
        _logger.LogInformation("Processing TrackLoginEvent request");

        try
        {
            // Read and deserialize request body
            var body = await req.ReadAsStringAsync();
            if (string.IsNullOrEmpty(body))
            {
                _logger.LogWarning("Request body is empty");
                return await req.CreateStandardResponseAsync<object>(
                    success: false,
                    data: null,
                    errorMessage: "Request body is required",
                    statusCode: HttpStatusCode.BadRequest);
            }

            LoginEventRequest? request;
            try
            {
                request = JsonSerializer.Deserialize<LoginEventRequest>(body, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Invalid JSON in request body");
                return await req.CreateStandardResponseAsync<object>(
                    success: false,
                    data: null,
                    errorMessage: "Invalid JSON format",
                    statusCode: HttpStatusCode.BadRequest);
            }

            if (request == null)
            {
                return await req.CreateStandardResponseAsync<object>(
                    success: false,
                    data: null,
                    errorMessage: "Invalid request body",
                    statusCode: HttpStatusCode.BadRequest);
            }

            if (string.IsNullOrEmpty(request.UserToken) || string.IsNullOrEmpty(request.ClientId))
            {
                _logger.LogWarning("UserToken and ClientId are required for login event tracking");
                return await req.CreateStandardResponseAsync<object>(
                    success: false,
                    data: null,
                    errorMessage: "UserToken and ClientId are required",
                    statusCode: HttpStatusCode.BadRequest);
            }

            var result = await _analyticsManagementService.TrackLoginEventAsync(request);

            _logger.LogInformation("Successfully processed login event tracking request with result: {Success}", result.Success);

            return await req.CreateStandardResponseAsync(
                success: result.Success,
                data: result,
                statusCode: result.Success ? HttpStatusCode.OK : HttpStatusCode.BadRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing TrackLoginEvent request");
            return await req.CreateStandardResponseAsync<object>(
                success: false,
                data: null,
                errorMessage: "Internal server error",
                statusCode: HttpStatusCode.InternalServerError);
        }
    }
}