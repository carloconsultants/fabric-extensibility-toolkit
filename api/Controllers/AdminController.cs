using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using PowerBITips.Api.Extensions;
using PowerBITips.Api.Models.Constants;
using PowerBITips.Api.Models.DTOs.Requests;
using PowerBITips.Api.Models.DTOs.Responses;
using PowerBITips.Api.Models.DTOs.Common;
using PowerBITips.Api.Models.Enums;
using PowerBITips.Api.Services.Interfaces;
using PowerBITips.Api.Utilities.Helpers;
using PowerBITips.Api.Middleware;
using System.Net;
using System.Text.Json;

namespace PowerBITips.Api.Controllers;

public class AdminController
{
    private readonly IUserService _userService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        IUserService userService,
        ILogger<AdminController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Gets a paginated list of all users in the system.
    /// Admin-only operation.
    /// </summary>
    [Function("GetAllUsers")]
    public async Task<HttpResponseData> GetAllUsers(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RouteConstants.AdminUsers)] HttpRequestData req)
    {
        _logger.LogInformation("Processing GetAllUsers request");

        try
        {
            // Get client principal for authorization
            var clientPrincipal = StaticWebAppsAuth.Parse(req);
            var userInfo = await _userService.GetUserAsync(clientPrincipal);

            // Check if user has access (Admin or local environment allows all users)
            var isLocalEnvironment = Environment.GetEnvironmentVariable("APP_ENVIRONMENT") == "local";
            var userHasAccess = isLocalEnvironment ||
                               (userInfo.IsSuccess && userInfo.Data?.UserRole == UserRole.Admin);

            if (!userHasAccess)
            {
                return await req.CreateStandardResponseAsync<object>(
                    success: false,
                    data: null,
                    errorMessage: "Unauthorized access - Admin role required",
                    statusCode: HttpStatusCode.Unauthorized);
            }

            // Parse query parameters into GetUsersRequest DTO
            var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
            var getUsersRequest = new GetUsersRequest
            {
                SearchQuery = query["search"],
                PageSize = int.TryParse(query["pageSize"], out var pageSize) ? Math.Min(pageSize, 100) : 50,
                PageNumber = int.TryParse(query["page"], out var page) ? Math.Max(page, 1) : 1,
                RoleFilter = Enum.TryParse<UserRole>(query["role"], out var role) ? role : null,
                IncludeSubscriptionInfo = bool.TryParse(query["includeSubscription"], out var includeSub) && includeSub,
                CreatedAfter = DateTime.TryParse(query["createdAfter"], out var after) ? after : null,
                CreatedBefore = DateTime.TryParse(query["createdBefore"], out var before) ? before : null,
                CorrelationId = Guid.NewGuid().ToString(),
                RequestTimestamp = DateTime.UtcNow
            };

            // Validate the request
            var (isValid, validationErrors) = ValidationHelper.TryValidateDto(getUsersRequest);
            if (!isValid)
            {
                return await req.CreateStandardResponseAsync<object>(
                    success: false,
                    data: null,
                    errorMessage: "Validation failed",
                    statusCode: HttpStatusCode.BadRequest);
            }

            // Call service with pagination
            var result = await _userService.GetUsersAsync(
                getUsersRequest.SearchQuery ?? string.Empty,
                getUsersRequest.PageSize);

            if (result.IsSuccess && result.Data != null)
            {
                return await req.CreateStandardResponseAsync(
                    success: true,
                    data: result.Data,
                    errorMessage: null,
                    statusCode: HttpStatusCode.OK);
            }
            else
            {
                return await req.CreateStandardResponseAsync<object>(
                    success: false,
                    data: null,
                    errorMessage: result.ErrorMessage ?? "Error retrieving users",
                    statusCode: result.Status);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetAllUsers");
            return await req.CreateStandardResponseAsync<object>(
                success: false,
                data: null,
                errorMessage: "Internal server error",
                statusCode: HttpStatusCode.InternalServerError);
        }
    }

    /// <summary>
    /// Updates a user's role in the system.
    /// Admin-only operation.
    /// </summary>
    [Function("UpdateUserRole")]
    public async Task<HttpResponseData> UpdateUserRole(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = RouteConstants.AdminUserRole)] HttpRequestData req,
        string userId)
    {
        _logger.LogInformation("Processing UpdateUserRole request for user: {UserId}", userId);

        try
        {
            // Validate userId parameter
            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out _))
            {
                return await req.CreateStandardResponseAsync<object>(
                    success: false,
                    data: null,
                    errorMessage: "Valid user ID is required",
                    statusCode: HttpStatusCode.BadRequest);
            }

            // Check authorization - only admins can update roles (or local environment)
            var clientPrincipal = StaticWebAppsAuth.Parse(req);
            var userInfo = await _userService.GetUserAsync(clientPrincipal);
            var isLocalEnvironment = Environment.GetEnvironmentVariable("APP_ENVIRONMENT") == "local";

            var hasPermission = isLocalEnvironment ||
                               (userInfo.IsSuccess && userInfo.Data?.UserRole == UserRole.Admin);

            if (!hasPermission)
            {
                return await req.CreateStandardResponseAsync<object>(
                    success: false,
                    data: null,
                    errorMessage: "Unauthorized access - Admin role required",
                    statusCode: HttpStatusCode.Unauthorized);
            }

            // Validate and parse request body
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(requestBody))
            {
                var errorResponse = ValidationHelper.CreateValidationErrorResponse(
                    new List<ValidationError>
                    {
                        new ValidationError
                        {
                            Field = "Request",
                            Message = "Request body is required",
                            Code = "EMPTY_REQUEST"
                        }
                    });

                return await req.CreateStandardResponseAsync<object>(success: false, data: null, errorMessage: "Validation failed", statusCode: HttpStatusCode.BadRequest);
            }

            UpdateUserRoleRequest? updateRoleRequest;
            try
            {
                updateRoleRequest = JsonSerializer.Deserialize<UpdateUserRoleRequest>(requestBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            }
            catch (JsonException ex)
            {
                var errorResponse = ValidationHelper.CreateErrorResponse(
                    "Invalid JSON format in request body",
                    "JSON_PARSE_ERROR",
                    ex);

                return await req.CreateStandardResponseAsync<object>(success: false, data: null, errorMessage: "Validation failed", statusCode: HttpStatusCode.BadRequest);
            }

            if (updateRoleRequest == null)
            {
                var errorResponse = ValidationHelper.CreateErrorResponse(
                    "Invalid request body",
                    "NULL_REQUEST");

                return await req.CreateStandardResponseAsync<object>(success: false, data: null, errorMessage: "Validation failed", statusCode: HttpStatusCode.BadRequest);
            }

            // Validate the DTO
            var (isValid, validationErrors) = ValidationHelper.TryValidateDto(updateRoleRequest);
            if (!isValid)
            {
                var validationErrorResponse = ValidationHelper.CreateValidationErrorResponse(validationErrors);
                return await req.CreateStandardResponseAsync<object>(success: false, data: null, errorMessage: "Validation failed", statusCode: HttpStatusCode.BadRequest);
            }

            // Get current user to track previous role
            var currentUserResult = await _userService.GetUserByIdAsync(userId);
            var previousRole = currentUserResult.Data?.UserRole ?? UserRole.User;

            var result = await _userService.UpdateUserRoleAsync(userId, updateRoleRequest);

            if (result.IsSuccess && result.Data != null)
            {
                // Create role update response DTO
                var updateRoleResponse = new UpdateUserRoleResponse
                {
                    UserId = result.Data.UserId,
                    PreviousRole = previousRole,
                    NewRole = result.Data.UserRole ?? UserRole.User,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = userInfo.Data?.UserName ?? "System",
                    Success = true,
                    Message = "User role updated successfully",
                    Timestamp = DateTime.UtcNow
                };

                return await req.CreateStandardResponseAsync(success: true, data: updateRoleResponse, errorMessage: null, statusCode: HttpStatusCode.OK);
            }
            else
            {
                var errorResponse = ValidationHelper.CreateErrorResponse(
                    result.ErrorMessage ?? "Error updating user role",
                    "USER_ROLE_UPDATE_ERROR");

                return await req.CreateStandardResponseAsync<object>(success: false, data: null, errorMessage: result.ErrorMessage ?? "Operation failed", statusCode: result.Status);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in UpdateUserRole for user: {UserId}", userId);
            var errorResponse = ValidationHelper.CreateErrorResponse(
                "Internal server error",
                "INTERNAL_ERROR",
                ex);

            return await req.CreateStandardResponseAsync<object>(success: false, data: null, errorMessage: "Internal server error", statusCode: HttpStatusCode.InternalServerError);
        }
    }

    /// <summary>
    /// Updates a user's trial subscription status.
    /// Admin-only operation. Moves functionality from former user endpoint.
    /// </summary>
    [Function("UpdateTrialSubscription")]
    public async Task<HttpResponseData> UpdateTrialSubscription(
        [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = RouteConstants.AdminUserTrial)] HttpRequestData req,
        string userId)
    {
        _logger.LogInformation("Processing UpdateTrialSubscription request for user: {UserId}", userId);

        try
        {
            // Validate userId parameter
            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out _))
            {
                var errorResponse = ValidationHelper.CreateValidationErrorResponse(
                    new List<ValidationError>
                    {
                        new ValidationError
                        {
                            Field = "userId",
                            Message = "Valid user ID is required",
                            Code = "INVALID_USER_ID"
                        }
                    });

                return await req.CreateStandardResponseAsync<object>(success: false, data: null, errorMessage: "Validation failed", statusCode: HttpStatusCode.BadRequest);
            }

            // Authorization check (admin or local environment)
            var clientPrincipal = StaticWebAppsAuth.Parse(req);
            var userInfo = await _userService.GetUserAsync(clientPrincipal);
            var isLocalEnvironment = Environment.GetEnvironmentVariable("APP_ENVIRONMENT") == "local";
            var hasPermission = isLocalEnvironment || (userInfo.IsSuccess && userInfo.Data?.UserRole == UserRole.Admin);

            if (!hasPermission)
            {
                var errorResponse = ValidationHelper.CreateErrorResponse(
                    "Unauthorized access - Admin role required",
                    "UNAUTHORIZED");

                return await req.CreateStandardResponseAsync<object>(success: false, data: null, errorMessage: "Unauthorized access", statusCode: HttpStatusCode.Unauthorized);
            }

            // Read request body
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            if (string.IsNullOrWhiteSpace(requestBody))
            {
                var errorResponse = ValidationHelper.CreateValidationErrorResponse(
                    new List<ValidationError>
                    {
                        new ValidationError
                        {
                            Field = "Request",
                            Message = "Request body is required",
                            Code = "EMPTY_REQUEST"
                        }
                    });

                return await req.CreateStandardResponseAsync<object>(success: false, data: null, errorMessage: "Validation failed", statusCode: HttpStatusCode.BadRequest);
            }

            UpdateTrialSubscriptionRequest? updateTrialRequest;
            try
            {
                updateTrialRequest = JsonSerializer.Deserialize<UpdateTrialSubscriptionRequest>(requestBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            }
            catch (JsonException ex)
            {
                var errorResponse = ValidationHelper.CreateErrorResponse(
                    "Invalid JSON format in request body",
                    "JSON_PARSE_ERROR",
                    ex);

                return await req.CreateStandardResponseAsync<object>(success: false, data: null, errorMessage: "Validation failed", statusCode: HttpStatusCode.BadRequest);
            }

            if (updateTrialRequest == null)
            {
                var errorResponse = ValidationHelper.CreateErrorResponse(
                    "Invalid request body",
                    "NULL_REQUEST");

                return await req.CreateStandardResponseAsync<object>(success: false, data: null, errorMessage: "Validation failed", statusCode: HttpStatusCode.BadRequest);
            }

            // Validate DTO
            var (isValid, validationErrors) = ValidationHelper.TryValidateDto(updateTrialRequest);
            if (!isValid)
            {
                var validationErrorResponse = ValidationHelper.CreateValidationErrorResponse(validationErrors);
                return await req.CreateStandardResponseAsync<object>(success: false, data: null, errorMessage: "Validation failed", statusCode: HttpStatusCode.BadRequest);
            }

            // Capture previous trial subscription state
            var currentUserResult = await _userService.GetUserByIdAsync(userId);
            var previousSubscription = currentUserResult.Data?.TrialSubscription ?? string.Empty;

            var result = await _userService.UpdateTrialSubscriptionAsync(userId, updateTrialRequest);

            if (result.IsSuccess && result.Data != null)
            {
                // Map to response DTO using extension if available
                // Map using entity-based extension (result.Data is UserResponse). Build manually since extension expects User entity.
                var updateResponse = new UpdateTrialSubscriptionResponse
                {
                    UserId = result.Data.UserId,
                    PreviousSubscription = previousSubscription,
                    NewSubscription = result.Data.TrialSubscription ?? string.Empty,
                    UpdatedAt = DateTime.UtcNow,
                    ExpiresAt = result.Data.TrialSubscription switch
                    {
                        "30-day" => DateTime.UtcNow.AddDays(30),
                        "14-day" => DateTime.UtcNow.AddDays(14),
                        "7-day" => DateTime.UtcNow.AddDays(7),
                        _ => null
                    },
                    Success = true,
                    Message = "Trial subscription updated successfully",
                    Timestamp = DateTime.UtcNow,
                    UpdatedBy = userInfo.Data?.UserName ?? "System"
                };
                updateResponse.Timestamp = DateTime.UtcNow;
                updateResponse.Success = true;
                updateResponse.Message = "Trial subscription updated successfully";

                return await req.CreateStandardResponseAsync(success: true, data: updateResponse, errorMessage: null, statusCode: HttpStatusCode.OK);
            }
            else
            {
                var errorResponse = ValidationHelper.CreateErrorResponse(
                    result.ErrorMessage ?? "Error updating trial subscription",
                    "TRIAL_SUBSCRIPTION_UPDATE_ERROR");

                return await req.CreateStandardResponseAsync<object>(success: false, data: null, errorMessage: result.ErrorMessage ?? "Operation failed", statusCode: result.Status);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in UpdateTrialSubscription for user: {UserId}", userId);
            var errorResponse = ValidationHelper.CreateErrorResponse(
                "Internal server error",
                "INTERNAL_ERROR",
                ex);

            return await req.CreateStandardResponseAsync<object>(success: false, data: null, errorMessage: "Internal server error", statusCode: HttpStatusCode.InternalServerError);
        }
    }
}
