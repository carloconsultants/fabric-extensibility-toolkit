using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using PowerBITips.Api.Models.Constants;
using PowerBITips.Api.Models.DTOs.Requests;
using PowerBITips.Api.Models.DTOs.Responses;
using PowerBITips.Api.Models.DTOs.Extensions;
using PowerBITips.Api.Models.DTOs.Common;
using PowerBITips.Api.Services;
using PowerBITips.Api.Utilities.Helpers;
using PowerBITips.Api.Middleware;
using PowerBITips.Api.Extensions;
using System.Net;
using System.Text.Json;

namespace PowerBITips.Api.Controllers;

public class ThemeController
{
    private readonly ThemeService _themeService;
    private readonly ILogger<ThemeController> _logger;

    public ThemeController(
        ThemeService themeService,
        ILogger<ThemeController> logger)
    {
        _themeService = themeService;
        _logger = logger;
    }

    [Function("GetUserThemes")]
    public async Task<HttpResponseData> GetUserThemes(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RouteConstants.UserThemes)] HttpRequestData req,
        string userId)
    {
        _logger.LogInformation("Processing GetUserThemes request for user: {UserId}", userId);

        try
        {
            // Get client principal for authorization
            var clientPrincipal = StaticWebAppsAuth.Parse(req);
            var isLocalEnvironment = Environment.GetEnvironmentVariable("APP_ENVIRONMENT") == "local";
            var bypassAuth = Environment.GetEnvironmentVariable("BYPASS_AUTH") == "true";

            // Check if user has valid authentication first
            if (string.IsNullOrEmpty(clientPrincipal.UserId) && !isLocalEnvironment && !bypassAuth)
            {
                var unauthorizedErrorResponse = new ErrorResponseDto("Authentication required");
                var unauthorizedResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteAsJsonAsync(unauthorizedErrorResponse);
                return unauthorizedResponse;
            }

            // Check authorization - users can only access their own themes unless Admin or local
            var hasAccess = isLocalEnvironment ||
                           bypassAuth ||
                           clientPrincipal.UserId == userId ||
                           clientPrincipal.UserRoles?.Contains("Admin") == true;

            if (!hasAccess)
            {
                var forbiddenErrorResponse = new ErrorResponseDto("You can only access your own themes")
                {
                    Details = $"Requested userId: {userId}, Authenticated userId: {clientPrincipal.UserId}"
                };
                var forbiddenResponse = req.CreateResponse(HttpStatusCode.Forbidden);
                await forbiddenResponse.WriteAsJsonAsync(forbiddenErrorResponse);
                return forbiddenResponse;
            }

            var result = await _themeService.GetUserThemesAsync(clientPrincipal);

            if (result.IsSuccess && result.Data != null)
            {
                // Use standardized response with camelCase formatting
                return await req.CreateStandardResponseAsync(
                    success: true,
                    data: result.Data,
                    statusCode: HttpStatusCode.OK
                );
            }
            else
            {
                // Use standardized error response
                return await req.CreateStandardResponseAsync(
                    success: false,
                    data: (object?)null,
                    errorMessage: result.ErrorMessage ?? "Error getting user themes",
                    statusCode: result.Status
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetUserThemes for user: {UserId}", userId);
            // Use standardized error response for exceptions
            return await req.CreateStandardResponseAsync(
                success: false,
                data: (object?)null,
                errorMessage: "Internal server error",
                statusCode: HttpStatusCode.InternalServerError
            );
        }
    }

    [Function("SaveTheme")]
    public async Task<HttpResponseData> SaveTheme(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RouteConstants.Themes)] HttpRequestData req)
    {
        _logger.LogInformation("💾 Processing SaveTheme request");
        _logger.LogInformation("💾 Request URL: {RequestUrl}", req.Url);

        try
        {
            // Get client principal for authorization
            var clientPrincipal = StaticWebAppsAuth.Parse(req);

            // For local development, allow anonymous access
            var isDevelopment = Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT") == "Development"
                               || Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development"
                               || string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME"));

            if (!isDevelopment && string.IsNullOrEmpty(clientPrincipal.UserId))
            {
                var unauthorizedErrorResponse = new ErrorResponseDto("Authentication required");
                var unauthorizedResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteAsJsonAsync(unauthorizedErrorResponse);
                return unauthorizedResponse;
            }

            _logger.LogInformation("💾 SaveTheme - isDevelopment: {IsDevelopment}, UserId: {UserId}",
                isDevelopment, clientPrincipal.UserId ?? "anonymous");

            // Parse and validate request body
            string requestBody = await req.ReadAsStringAsync() ?? string.Empty;

            _logger.LogInformation("💾 SaveTheme Request Body: {RequestBody}", requestBody);

            if (string.IsNullOrEmpty(requestBody))
            {
                var emptyBodyErrorResponse = new ErrorResponseDto("Request body is required");
                var emptyBodyResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await emptyBodyResponse.WriteAsJsonAsync(emptyBodyErrorResponse);
                return emptyBodyResponse;
            }

            SaveThemeRequest? saveThemeRequest;
            try
            {
                saveThemeRequest = JsonSerializer.Deserialize<SaveThemeRequest>(requestBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Invalid JSON in save theme request");
                var jsonErrorResponse = new ErrorResponseDto("Invalid JSON format")
                {
                    ErrorDetails = new Dictionary<string, object> { { "JsonError", ex.Message } }
                };
                var jsonErrorHttpResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await jsonErrorHttpResponse.WriteAsJsonAsync(jsonErrorResponse);
                return jsonErrorHttpResponse;
            }

            if (saveThemeRequest == null)
            {
                var nullRequestErrorResponse = new ErrorResponseDto("Invalid request format");
                var nullRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await nullRequestResponse.WriteAsJsonAsync(nullRequestErrorResponse);
                return nullRequestResponse;
            }

            // Validate the request DTO
            var (isValid, validationErrors) = ValidationHelper.TryValidateDto(saveThemeRequest);
            if (!isValid)
            {
                var validationErrorResponse = new ErrorResponseDto("Invalid request data")
                {
                    ValidationErrors = validationErrors
                };

                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteAsJsonAsync(validationErrorResponse);
                return badRequestResponse;
            }

            var result = await _themeService.SaveThemeAsync(clientPrincipal, saveThemeRequest);

            if (result.IsSuccess && result.Data != null)
            {
                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(result.Data);
                return response;
            }
            else
            {
                var errorResponse = new ErrorResponseDto(result.ErrorMessage ?? "Error saving theme");
                var httpErrorResponse = req.CreateResponse(result.Status);
                await httpErrorResponse.WriteAsJsonAsync(errorResponse);
                return httpErrorResponse;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SaveTheme");
            var errorResponse = new ErrorResponseDto("Internal server error")
            {
                Details = ex.Message
            };
            var httpErrorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await httpErrorResponse.WriteAsJsonAsync(errorResponse);
            return httpErrorResponse;
        }
    }

    [Function("UpdateTheme")]
    public async Task<HttpResponseData> UpdateTheme(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = RouteConstants.ThemeById)] HttpRequestData req,
        string themeId)
    {
        _logger.LogInformation("🔄 Processing UpdateTheme request for theme: {ThemeId}", themeId);

        try
        {
            // Validate themeId parameter
            if (string.IsNullOrWhiteSpace(themeId))
            {
                var validationErrorResponse = new ErrorResponseDto("Theme ID is required")
                {
                    Details = "The themeId parameter cannot be null or empty"
                };
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteAsJsonAsync(validationErrorResponse);
                return badRequestResponse;
            }

            // Get client principal for authorization
            var clientPrincipal = StaticWebAppsAuth.Parse(req);

            // For local development, allow anonymous access
            var isDevelopment = Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT") == "Development"
                               || Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development"
                               || string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME"));

            if (!isDevelopment && string.IsNullOrEmpty(clientPrincipal.UserId))
            {
                var unauthorizedErrorResponse = new ErrorResponseDto("Authentication required");
                var unauthorizedResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteAsJsonAsync(unauthorizedErrorResponse);
                return unauthorizedResponse;
            }

            _logger.LogInformation("🔄 UpdateTheme - isDevelopment: {IsDevelopment}, UserId: {UserId}",
                isDevelopment, clientPrincipal.UserId ?? "anonymous");

            // Parse and validate request body
            string requestBody = await req.ReadAsStringAsync() ?? string.Empty;

            _logger.LogInformation("🔄 UpdateTheme Request Body: {RequestBody}", requestBody);

            if (string.IsNullOrEmpty(requestBody))
            {
                var emptyBodyErrorResponse = new ErrorResponseDto("Request body is required");
                var emptyBodyResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await emptyBodyResponse.WriteAsJsonAsync(emptyBodyErrorResponse);
                return emptyBodyResponse;
            }

            SaveThemeRequest? saveThemeRequest;
            try
            {
                saveThemeRequest = JsonSerializer.Deserialize<SaveThemeRequest>(requestBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Invalid JSON in update theme request");
                var jsonErrorResponse = new ErrorResponseDto("Invalid JSON format")
                {
                    ErrorDetails = new Dictionary<string, object> { { "JsonError", ex.Message } }
                };
                var jsonErrorHttpResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await jsonErrorHttpResponse.WriteAsJsonAsync(jsonErrorResponse);
                return jsonErrorHttpResponse;
            }

            if (saveThemeRequest == null)
            {
                var nullRequestErrorResponse = new ErrorResponseDto("Invalid request format");
                var nullRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await nullRequestResponse.WriteAsJsonAsync(nullRequestErrorResponse);
                return nullRequestResponse;
            }

            // Ensure the ID in the request matches the URL parameter
            if (!string.IsNullOrEmpty(saveThemeRequest.Id) && saveThemeRequest.Id != themeId)
            {
                var idMismatchErrorResponse = new ErrorResponseDto("Theme ID in request body does not match URL parameter")
                {
                    Details = $"URL themeId: {themeId}, Request body ID: {saveThemeRequest.Id}"
                };
                var idMismatchResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await idMismatchResponse.WriteAsJsonAsync(idMismatchErrorResponse);
                return idMismatchResponse;
            }

            // Set the ID from the URL if not provided in the request
            if (string.IsNullOrEmpty(saveThemeRequest.Id))
            {
                saveThemeRequest.Id = themeId;
            }

            // Validate the request DTO
            var (isValid, validationErrors) = ValidationHelper.TryValidateDto(saveThemeRequest);
            if (!isValid)
            {
                var validationErrorResponse = new ErrorResponseDto("Invalid request data")
                {
                    ValidationErrors = validationErrors
                };

                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteAsJsonAsync(validationErrorResponse);
                return badRequestResponse;
            }

            var result = await _themeService.SaveThemeAsync(clientPrincipal, saveThemeRequest);

            if (result.IsSuccess && result.Data != null)
            {
                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(result.Data);
                return response;
            }
            else
            {
                var errorResponse = new ErrorResponseDto(result.ErrorMessage ?? "Error updating theme");
                var httpErrorResponse = req.CreateResponse(result.Status);
                await httpErrorResponse.WriteAsJsonAsync(errorResponse);
                return httpErrorResponse;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in UpdateTheme for theme: {ThemeId}", themeId);
            var errorResponse = new ErrorResponseDto("Internal server error")
            {
                Details = ex.Message
            };
            var httpErrorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await httpErrorResponse.WriteAsJsonAsync(errorResponse);
            return httpErrorResponse;
        }
    }

    [Function("DeleteTheme")]
    public async Task<HttpResponseData> DeleteTheme(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = RouteConstants.ThemeById)] HttpRequestData req,
        string themeId)
    {
        _logger.LogInformation("Processing DeleteTheme request for theme: {ThemeId}", themeId);

        try
        {
            // Validate themeId parameter
            if (string.IsNullOrWhiteSpace(themeId))
            {
                var validationErrorResponse = new ErrorResponseDto("Theme ID is required")
                {
                    Details = "The themeId parameter cannot be null or empty"
                };
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteAsJsonAsync(validationErrorResponse);
                return badRequestResponse;
            }

            // Get client principal for authorization
            var clientPrincipal = StaticWebAppsAuth.Parse(req);
            if (string.IsNullOrEmpty(clientPrincipal.UserId))
            {
                var unauthorizedErrorResponse = new ErrorResponseDto("Authentication required");
                var unauthorizedResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteAsJsonAsync(unauthorizedErrorResponse);
                return unauthorizedResponse;
            }

            var result = await _themeService.DeleteThemeAsync(clientPrincipal, themeId);

            if (result.IsSuccess)
            {
                var successResponse = new SuccessResponseDto("Theme deleted successfully")
                {
                    Data = new Dictionary<string, object> { { "success", result.Data } }
                };
                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(successResponse);
                return response;
            }
            else
            {
                var errorResponse = new ErrorResponseDto(result.ErrorMessage ?? "Error deleting theme");
                var httpErrorResponse = req.CreateResponse(result.Status);
                await httpErrorResponse.WriteAsJsonAsync(errorResponse);
                return httpErrorResponse;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DeleteTheme for theme: {ThemeId}", themeId);
            var errorResponse = new ErrorResponseDto("Internal server error")
            {
                Details = ex.Message
            };
            var httpErrorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await httpErrorResponse.WriteAsJsonAsync(errorResponse);
            return httpErrorResponse;
        }
    }

    /// <summary>
    /// Get a theme for the currently authenticated user (no userId in path)
    /// </summary>
    [Function("GetMyTheme")]
    public async Task<HttpResponseData> GetMyTheme(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RouteConstants.ThemeById)] HttpRequestData req,
        string themeId)
    {
        _logger.LogInformation("🎨 Processing GetMyTheme request for theme: {ThemeId}", themeId);

        try
        {
            // Validate themeId parameter
            if (string.IsNullOrWhiteSpace(themeId))
            {
                var validationErrorResponse = new ErrorResponseDto("Theme ID is required")
                {
                    Details = "The themeId parameter cannot be null or empty"
                };
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteAsJsonAsync(validationErrorResponse);
                return badRequestResponse;
            }

            // Get client principal for authorization
            var clientPrincipal = StaticWebAppsAuth.Parse(req);

            // For local development, allow anonymous access
            var isDevelopment = Environment.GetEnvironmentVariable("APP_ENVIRONMENT") == "local" ||
                               Environment.GetEnvironmentVariable("BYPASS_AUTH") == "true";

            if (!isDevelopment && string.IsNullOrEmpty(clientPrincipal.UserId))
            {
                var unauthorizedErrorResponse = new ErrorResponseDto("Authentication required");
                var unauthorizedResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteAsJsonAsync(unauthorizedErrorResponse);
                return unauthorizedResponse;
            }

            _logger.LogInformation("🎨 GetMyTheme - UserId: {UserId}, ThemeId: {ThemeId}",
                clientPrincipal.UserId ?? "anonymous (dev)", themeId);

            // Get the raw JSON string directly to avoid double serialization
            var rawThemeResult = await _themeService.GetRawThemeJsonAsync(themeId);

            if (rawThemeResult.IsSuccess && rawThemeResult.Data != null)
            {
                // Use standardized API response format
                var responseData = new
                {
                    theme = rawThemeResult.Data.ThemeJson,
                    project = rawThemeResult.Data.ProjectJson,
                    images = rawThemeResult.Data.ImagesJson,
                    ScrimsFamilyName = rawThemeResult.Data.ThemeName
                };

                var apiResponse = new ApiResponseDto<object>
                {
                    Success = true,
                    ResultObject = responseData
                };

                var httpResponse = req.CreateResponse(HttpStatusCode.OK);
                await httpResponse.WriteAsJsonAsync(apiResponse);
                return httpResponse;
            }
            else
            {
                _logger.LogWarning(
                    "⚠️ RESOURCE_NOT_FOUND - Theme not found. Status: {Status}, ThemeId: {ThemeId}, UserId: {UserId} | The route exists but the resource doesn't",
                    rawThemeResult.Status,
                    themeId,
                    clientPrincipal.UserId ?? "anonymous (dev)"
                );

                var errorResponse = ValidationHelper.CreateResourceNotFoundResponse(
                    "Theme",
                    themeId,
                    $"Theme with ID '{themeId}' was not found. This may be a new theme that hasn't been saved yet."
                );
                var httpErrorResponse = req.CreateResponse(rawThemeResult.Status);
                await httpErrorResponse.WriteAsJsonAsync(errorResponse);
                return httpErrorResponse;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetMyTheme for theme: {ThemeId}", themeId);
            var errorResponse = new ErrorResponseDto("Internal server error")
            {
                Details = ex.Message
            };
            var httpErrorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await httpErrorResponse.WriteAsJsonAsync(errorResponse);
            return httpErrorResponse;
        }
    }

    [Function("CreateUserTheme")]
    public async Task<HttpResponseData> CreateUserTheme(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RouteConstants.UserThemes)] HttpRequestData req,
        string userId)
    {
        _logger.LogInformation("💾 Processing CreateUserTheme request for user: {UserId}", userId);

        try
        {
            // Get client principal for authorization
            var clientPrincipal = StaticWebAppsAuth.Parse(req);
            var isLocalEnvironment = Environment.GetEnvironmentVariable("APP_ENVIRONMENT") == "local";
            var bypassAuth = Environment.GetEnvironmentVariable("BYPASS_AUTH") == "true";

            // Check if user has valid authentication first
            if (string.IsNullOrEmpty(clientPrincipal.UserId) && !isLocalEnvironment && !bypassAuth)
            {
                var unauthorizedErrorResponse = new ErrorResponseDto("Authentication required");
                var unauthorizedResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteAsJsonAsync(unauthorizedErrorResponse);
                return unauthorizedResponse;
            }

            // Check authorization - users can only create themes for themselves unless Admin or local
            var hasAccess = isLocalEnvironment ||
                           bypassAuth ||
                           clientPrincipal.UserId == userId ||
                           clientPrincipal.UserRoles?.Contains("Admin") == true;

            if (!hasAccess)
            {
                var forbiddenErrorResponse = new ErrorResponseDto("You can only create themes for yourself")
                {
                    Details = $"Requested userId: {userId}, Authenticated userId: {clientPrincipal.UserId}"
                };
                var forbiddenResponse = req.CreateResponse(HttpStatusCode.Forbidden);
                await forbiddenResponse.WriteAsJsonAsync(forbiddenErrorResponse);
                return forbiddenResponse;
            }

            // Parse and validate request body
            string requestBody = await req.ReadAsStringAsync() ?? string.Empty;

            if (string.IsNullOrEmpty(requestBody))
            {
                var emptyBodyErrorResponse = new ErrorResponseDto("Request body is required");
                var emptyBodyResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await emptyBodyResponse.WriteAsJsonAsync(emptyBodyErrorResponse);
                return emptyBodyResponse;
            }

            SaveThemeRequest? saveThemeRequest;
            try
            {
                saveThemeRequest = JsonSerializer.Deserialize<SaveThemeRequest>(requestBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Invalid JSON in create theme request");
                var jsonErrorResponse = new ErrorResponseDto("Invalid JSON format")
                {
                    ErrorDetails = new Dictionary<string, object> { { "JsonError", ex.Message } }
                };
                var jsonErrorHttpResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await jsonErrorHttpResponse.WriteAsJsonAsync(jsonErrorResponse);
                return jsonErrorHttpResponse;
            }

            if (saveThemeRequest == null)
            {
                var nullRequestErrorResponse = new ErrorResponseDto("Invalid request format");
                var nullRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await nullRequestResponse.WriteAsJsonAsync(nullRequestErrorResponse);
                return nullRequestResponse;
            }

            // Ensure the request doesn't have an ID for creation
            if (!string.IsNullOrEmpty(saveThemeRequest.Id))
            {
                var hasIdErrorResponse = new ErrorResponseDto("ID should not be provided when creating a new theme");
                var hasIdResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await hasIdResponse.WriteAsJsonAsync(hasIdErrorResponse);
                return hasIdResponse;
            }

            // Validate the request DTO
            var (isValid, validationErrors) = ValidationHelper.TryValidateDto(saveThemeRequest);
            if (!isValid)
            {
                var validationErrorResponse = new ErrorResponseDto("Invalid request data")
                {
                    ValidationErrors = validationErrors
                };

                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteAsJsonAsync(validationErrorResponse);
                return badRequestResponse;
            }

            var result = await _themeService.SaveThemeAsync(clientPrincipal, saveThemeRequest);

            if (result.IsSuccess && result.Data != null)
            {
                var response = req.CreateResponse(HttpStatusCode.Created);
                await response.WriteAsJsonAsync(result.Data);
                return response;
            }
            else
            {
                var errorResponse = new ErrorResponseDto(result.ErrorMessage ?? "Error creating theme");
                var httpErrorResponse = req.CreateResponse(result.Status);
                await httpErrorResponse.WriteAsJsonAsync(errorResponse);
                return httpErrorResponse;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CreateUserTheme for user: {UserId}", userId);
            var errorResponse = new ErrorResponseDto("Internal server error")
            {
                Details = ex.Message
            };
            var httpErrorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await httpErrorResponse.WriteAsJsonAsync(errorResponse);
            return httpErrorResponse;
        }
    }

    [Function("UpdateUserTheme")]
    public async Task<HttpResponseData> UpdateUserTheme(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = RouteConstants.UserThemeById)] HttpRequestData req,
        string userId,
        string themeId)
    {
        _logger.LogInformation("🔄 Processing UpdateUserTheme request for user: {UserId}, theme: {ThemeId}", userId, themeId);

        try
        {
            // Validate parameters
            if (string.IsNullOrWhiteSpace(userId))
            {
                var userIdErrorResponse = new ErrorResponseDto("User ID is required");
                var userIdResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await userIdResponse.WriteAsJsonAsync(userIdErrorResponse);
                return userIdResponse;
            }

            if (string.IsNullOrWhiteSpace(themeId))
            {
                var themeIdErrorResponse = new ErrorResponseDto("Theme ID is required");
                var themeIdResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await themeIdResponse.WriteAsJsonAsync(themeIdErrorResponse);
                return themeIdResponse;
            }

            // Get client principal for authorization
            var clientPrincipal = StaticWebAppsAuth.Parse(req);
            var isLocalEnvironment = Environment.GetEnvironmentVariable("APP_ENVIRONMENT") == "local";
            var bypassAuth = Environment.GetEnvironmentVariable("BYPASS_AUTH") == "true";

            // Check if user has valid authentication first
            if (string.IsNullOrEmpty(clientPrincipal.UserId) && !isLocalEnvironment && !bypassAuth)
            {
                var unauthorizedErrorResponse = new ErrorResponseDto("Authentication required");
                var unauthorizedResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteAsJsonAsync(unauthorizedErrorResponse);
                return unauthorizedResponse;
            }

            // Check authorization - users can only update their own themes unless Admin or local
            var hasAccess = isLocalEnvironment ||
                           bypassAuth ||
                           clientPrincipal.UserId == userId ||
                           clientPrincipal.UserRoles?.Contains("Admin") == true;

            if (!hasAccess)
            {
                var forbiddenErrorResponse = new ErrorResponseDto("You can only update your own themes")
                {
                    Details = $"Requested userId: {userId}, Authenticated userId: {clientPrincipal.UserId}"
                };
                var forbiddenResponse = req.CreateResponse(HttpStatusCode.Forbidden);
                await forbiddenResponse.WriteAsJsonAsync(forbiddenErrorResponse);
                return forbiddenResponse;
            }

            // Parse and validate request body
            string requestBody = await req.ReadAsStringAsync() ?? string.Empty;

            if (string.IsNullOrEmpty(requestBody))
            {
                var emptyBodyErrorResponse = new ErrorResponseDto("Request body is required");
                var emptyBodyResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await emptyBodyResponse.WriteAsJsonAsync(emptyBodyErrorResponse);
                return emptyBodyResponse;
            }

            SaveThemeRequest? saveThemeRequest;
            try
            {
                saveThemeRequest = JsonSerializer.Deserialize<SaveThemeRequest>(requestBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Invalid JSON in update theme request");
                var jsonErrorResponse = new ErrorResponseDto("Invalid JSON format")
                {
                    ErrorDetails = new Dictionary<string, object> { { "JsonError", ex.Message } }
                };
                var jsonErrorHttpResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await jsonErrorHttpResponse.WriteAsJsonAsync(jsonErrorResponse);
                return jsonErrorHttpResponse;
            }

            if (saveThemeRequest == null)
            {
                var nullRequestErrorResponse = new ErrorResponseDto("Invalid request format");
                var nullRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await nullRequestResponse.WriteAsJsonAsync(nullRequestErrorResponse);
                return nullRequestResponse;
            }

            // Ensure the ID in the request matches the URL parameter
            if (!string.IsNullOrEmpty(saveThemeRequest.Id) && saveThemeRequest.Id != themeId)
            {
                var idMismatchErrorResponse = new ErrorResponseDto("Theme ID in request body does not match URL parameter")
                {
                    Details = $"URL themeId: {themeId}, Request body ID: {saveThemeRequest.Id}"
                };
                var idMismatchResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await idMismatchResponse.WriteAsJsonAsync(idMismatchErrorResponse);
                return idMismatchResponse;
            }

            // Set the ID from the URL if not provided in the request
            if (string.IsNullOrEmpty(saveThemeRequest.Id))
            {
                saveThemeRequest.Id = themeId;
            }

            // Validate the request DTO
            var (isValid, validationErrors) = ValidationHelper.TryValidateDto(saveThemeRequest);
            if (!isValid)
            {
                var validationErrorResponse = new ErrorResponseDto("Invalid request data")
                {
                    ValidationErrors = validationErrors
                };

                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteAsJsonAsync(validationErrorResponse);
                return badRequestResponse;
            }

            var result = await _themeService.SaveThemeAsync(clientPrincipal, saveThemeRequest);

            if (result.IsSuccess && result.Data != null)
            {
                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(result.Data);
                return response;
            }
            else
            {
                var errorResponse = new ErrorResponseDto(result.ErrorMessage ?? "Error updating theme");
                var httpErrorResponse = req.CreateResponse(result.Status);
                await httpErrorResponse.WriteAsJsonAsync(errorResponse);
                return httpErrorResponse;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in UpdateUserTheme for user: {UserId}, theme: {ThemeId}", userId, themeId);
            var errorResponse = new ErrorResponseDto("Internal server error")
            {
                Details = ex.Message
            };
            var httpErrorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await httpErrorResponse.WriteAsJsonAsync(errorResponse);
            return httpErrorResponse;
        }
    }

    [Function("GetUserThemeById")]
    public async Task<HttpResponseData> GetUserThemeById(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RouteConstants.UserThemeById)] HttpRequestData req,
        string userId,
        string themeId)
    {
        _logger.LogInformation("🎨 Processing GetUserThemeById request for user: {UserId}, theme: {ThemeId}", userId, themeId);

        try
        {
            // Validate parameters
            if (string.IsNullOrWhiteSpace(userId))
            {
                var userIdErrorResponse = new ErrorResponseDto("User ID is required");
                var userIdResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await userIdResponse.WriteAsJsonAsync(userIdErrorResponse);
                return userIdResponse;
            }

            if (string.IsNullOrWhiteSpace(themeId))
            {
                var themeIdErrorResponse = new ErrorResponseDto("Theme ID is required");
                var themeIdResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await themeIdResponse.WriteAsJsonAsync(themeIdErrorResponse);
                return themeIdResponse;
            }

            // Get client principal for authorization
            var clientPrincipal = StaticWebAppsAuth.Parse(req);
            var isLocalEnvironment = Environment.GetEnvironmentVariable("APP_ENVIRONMENT") == "local";
            var bypassAuth = Environment.GetEnvironmentVariable("BYPASS_AUTH") == "true";

            // Check if user has valid authentication first
            if (string.IsNullOrEmpty(clientPrincipal.UserId) && !isLocalEnvironment && !bypassAuth)
            {
                var unauthorizedErrorResponse = new ErrorResponseDto("Authentication required");
                var unauthorizedResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteAsJsonAsync(unauthorizedErrorResponse);
                return unauthorizedResponse;
            }

            // Check authorization - users can only access their own themes unless Admin or local
            var hasAccess = isLocalEnvironment ||
                           bypassAuth ||
                           clientPrincipal.UserId == userId ||
                           clientPrincipal.UserRoles?.Contains("Admin") == true;

            if (!hasAccess)
            {
                var forbiddenErrorResponse = new ErrorResponseDto("You can only access your own themes")
                {
                    Details = $"Requested userId: {userId}, Authenticated userId: {clientPrincipal.UserId}"
                };
                var forbiddenResponse = req.CreateResponse(HttpStatusCode.Forbidden);
                await forbiddenResponse.WriteAsJsonAsync(forbiddenErrorResponse);
                return forbiddenResponse;
            }

            // Get the raw JSON string directly to avoid double serialization
            var rawThemeResult = await _themeService.GetRawThemeJsonAsync(themeId);

            if (rawThemeResult.IsSuccess && rawThemeResult.Data != null)
            {
                // Use standardized API response format
                var responseData = new
                {
                    theme = rawThemeResult.Data.ThemeJson,
                    project = rawThemeResult.Data.ProjectJson,
                    images = rawThemeResult.Data.ImagesJson,
                    ScrimsFamilyName = rawThemeResult.Data.ThemeName
                };

                var apiResponse = new ApiResponseDto<object>
                {
                    Success = true,
                    ResultObject = responseData
                };

                var httpResponse = req.CreateResponse(HttpStatusCode.OK);
                await httpResponse.WriteAsJsonAsync(apiResponse);
                return httpResponse;
            }
            else
            {
                _logger.LogWarning(
                    "⚠️ RESOURCE_NOT_FOUND - Theme not found for user. Status: {Status}, ThemeId: {ThemeId}, UserId: {UserId}",
                    rawThemeResult.Status,
                    themeId,
                    userId
                );

                var errorResponse = ValidationHelper.CreateResourceNotFoundResponse(
                    "Theme",
                    themeId,
                    $"Theme with ID '{themeId}' was not found for user '{userId}'"
                );
                var httpErrorResponse = req.CreateResponse(rawThemeResult.Status);
                await httpErrorResponse.WriteAsJsonAsync(errorResponse);
                return httpErrorResponse;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetUserThemeById for user: {UserId}, theme: {ThemeId}", userId, themeId);
            var errorResponse = new ErrorResponseDto("Internal server error")
            {
                Details = ex.Message
            };
            var httpErrorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await httpErrorResponse.WriteAsJsonAsync(errorResponse);
            return httpErrorResponse;
        }
    }

}