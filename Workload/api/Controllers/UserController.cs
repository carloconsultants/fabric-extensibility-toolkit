using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using PowerBITips.Api.Models.Constants;
using PowerBITips.Api.Models.DTOs.Requests;
using PowerBITips.Api.Models.DTOs.Responses;
using PowerBITips.Api.Models.DTOs.Extensions;
using PowerBITips.Api.Models.DTOs.Common;
using PowerBITips.Api.Models.Enums;
using PowerBITips.Api.Services.Common;
using PowerBITips.Api.Services.Interfaces;
using PowerBITips.Api.Utilities.Helpers;
using PowerBITips.Api.Middleware;
using PowerBITips.Api.Extensions;
using System.Net;
using System.Text.Json;

namespace PowerBITips.Api.Controllers;

public class UserController
{
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;

    public UserController(
        IUserService userService,
        ILogger<UserController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [Function("GetUserMe")]
    public async Task<HttpResponseData> GetUserMe(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = RouteConstants.UserMe)] HttpRequestData req)
    {
        _logger.LogInformation("🚀 [GetUserMe] Starting GetUserMe request - Method: {Method}, URL: {Url}",
            req.Method, req.Url?.ToString() ?? "unknown");

        try
        {
            // Parse client principal with logging
            var clientPrincipal = StaticWebAppsAuth.Parse(req, _logger);

            // Log FULL client principal details
            _logger.LogInformation("👤 [GetUserMe] ========== CLIENT PRINCIPAL DETAILS ==========");
            _logger.LogInformation("   UserId: {UserId}", clientPrincipal?.UserId ?? "NULL");
            _logger.LogInformation("   UserDetails: {UserDetails}", clientPrincipal?.UserDetails ?? "NULL");
            _logger.LogInformation("   IdentityProvider: {IdentityProvider}", clientPrincipal?.IdentityProvider ?? "NULL");
            _logger.LogInformation("   HasUserId: {HasUserId}", !string.IsNullOrEmpty(clientPrincipal?.UserId));
            _logger.LogInformation("   UserRoles Count: {Count}", clientPrincipal?.UserRoles?.Count ?? 0);
            if (clientPrincipal?.UserRoles != null && clientPrincipal.UserRoles.Any())
            {
                _logger.LogInformation("   UserRoles: [{Roles}]", string.Join(", ", clientPrincipal.UserRoles));
            }
            _logger.LogInformation("   Claims Count: {Count}", clientPrincipal?.Claims?.Count ?? 0);
            if (clientPrincipal?.Claims != null && clientPrincipal.Claims.Any())
            {
                foreach (var claim in clientPrincipal.Claims)
                {
                    _logger.LogInformation("     Claim - Type: {Type}, Value: {Value}", claim.Typ ?? "NULL", claim.Val ?? "NULL");
                }
            }
            _logger.LogInformation("👤 [GetUserMe] ================================================");

            // If no client principal, return null response (matches old API behavior)
            if (clientPrincipal == null || string.IsNullOrEmpty(clientPrincipal.UserId))
            {
                _logger.LogWarning("⚠️ [GetUserMe] No client principal found - returning empty response");
                var emptyResponse = new { userId = "anonymous", authenticated = false };

                _logger.LogInformation("✅ [GetUserMe] Returning empty response (no authenticated user)");
                return await req.CreateStandardResponseAsync(
                    success: true,
                    data: emptyResponse,
                    statusCode: HttpStatusCode.OK
                );
            }

            // Get the current user
            // clientPrincipal is guaranteed to be non-null here due to the check above
            _logger.LogInformation("🔍 [GetUserMe] Looking up user - Using client principal: {UserDetails}",
                clientPrincipal!.UserDetails ?? "null");

            var result = await _userService.GetUserAsync(clientPrincipal!);

            _logger.LogInformation("📊 [GetUserMe] User lookup result - Success: {IsSuccess}, Status: {Status}, HasData: {HasData}, ErrorMessage: {ErrorMessage}",
                result.IsSuccess, result.Status, result.Data != null, result.ErrorMessage ?? "none");

            if (result.IsSuccess && result.Data != null)
            {
                // Map to GetUserInfoResponse format (matches old TypeScript API)
                var getUserInfoResponse = new Models.DTOs.Responses.GetUserInfoResponse
                {
                    ClientPrincipal = clientPrincipal,
                    Subscription = result.Data.Subscription,
                    TrialSubscription = result.Data.TrialSubscription,
                    UserRole = result.Data.UserRole?.ToString(),
                    NumberOfThemes = result.Data.Themes?.Count,
                    NumberOfPublishedItems = result.Data.Published?.Count,
                    HasActiveAzureSubscription = !string.IsNullOrEmpty(result.Data.AzureSubscriptionId)
                };

                _logger.LogInformation("✅ [GetUserMe] Successfully retrieved user - UserId: {UserId}, Role: {Role}, Themes: {Themes}, Published: {Published}",
                    result.Data.UserId,
                    getUserInfoResponse.UserRole,
                    getUserInfoResponse.NumberOfThemes,
                    getUserInfoResponse.NumberOfPublishedItems);

                return await req.CreateStandardResponseAsync(
                    success: true,
                    data: getUserInfoResponse,
                    statusCode: HttpStatusCode.OK
                );
            }
            else
            {
                _logger.LogWarning("❌ [GetUserMe] Failed to retrieve user - Status: {Status}, Error: {Error}",
                    result.Status, result.ErrorMessage ?? "Unknown error");

                return await req.CreateStandardResponseAsync(
                    success: false,
                    data: (object?)null,
                    errorMessage: result.ErrorMessage ?? "Error retrieving user info",
                    statusCode: result.Status
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 [GetUserMe] Exception occurred");

            return await req.CreateStandardResponseAsync(
                success: false,
                data: (object?)null,
                errorMessage: "Internal server error",
                statusCode: HttpStatusCode.InternalServerError
            );
        }
    }

    [Function("GetUserInfo")]
    public async Task<HttpResponseData> GetUserInfo(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = RouteConstants.UserById)] HttpRequestData req,
        string userId)
    {
        _logger.LogInformation("🚀 [GetUserInfo] Starting GetUserInfo request - Method: {Method}, UserId: {UserId}, URL: {Url}",
            req.Method, userId, req.Url?.ToString() ?? "unknown");

        try
        {
            // Parse client principal with logging
            var clientPrincipal = StaticWebAppsAuth.Parse(req, _logger);

            // Log FULL client principal details
            _logger.LogInformation("👤 [GetUserInfo] ========== CLIENT PRINCIPAL DETAILS ==========");
            _logger.LogInformation("   UserId: {UserId}", clientPrincipal?.UserId ?? "NULL");
            _logger.LogInformation("   UserDetails: {UserDetails}", clientPrincipal?.UserDetails ?? "NULL");
            _logger.LogInformation("   IdentityProvider: {IdentityProvider}", clientPrincipal?.IdentityProvider ?? "NULL");
            _logger.LogInformation("   HasUserId: {HasUserId}", !string.IsNullOrEmpty(clientPrincipal?.UserId));
            _logger.LogInformation("   UserRoles Count: {Count}", clientPrincipal?.UserRoles?.Count ?? 0);
            if (clientPrincipal?.UserRoles != null && clientPrincipal.UserRoles.Any())
            {
                _logger.LogInformation("   UserRoles: [{Roles}]", string.Join(", ", clientPrincipal.UserRoles));
            }
            _logger.LogInformation("   Claims Count: {Count}", clientPrincipal?.Claims?.Count ?? 0);
            if (clientPrincipal?.Claims != null && clientPrincipal.Claims.Any())
            {
                foreach (var claim in clientPrincipal.Claims)
                {
                    _logger.LogInformation("     Claim - Type: {Type}, Value: {Value}", claim.Typ ?? "NULL", claim.Val ?? "NULL");
                }
            }
            _logger.LogInformation("👤 [GetUserInfo] ================================================");

            // Handle "me" as a special case - use client principal to get current user
            ServiceResponse<UserResponse> result;
            if (userId.Equals("me", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("🔍 [GetUserInfo] 'me' requested - looking up current user from client principal: {UserDetails}",
                    clientPrincipal?.UserDetails ?? "null");

                // If no client principal and requesting "me", return empty response (matches old API behavior)
                if (clientPrincipal == null || string.IsNullOrEmpty(clientPrincipal.UserId))
                {
                    _logger.LogWarning("⚠️ [GetUserInfo] No client principal found for 'me' request - returning empty response");
                    var emptyResponse = new Models.DTOs.Responses.GetUserInfoResponse
                    {
                        ClientPrincipal = null,
                        Subscription = null
                    };

                    // Wrap in ApiResponse format expected by frontend
                    var apiResponse = new
                    {
                        success = true,
                        resultObject = emptyResponse
                    };

                    var response = req.CreateResponse(HttpStatusCode.OK);
                    await response.WriteAsJsonAsync(apiResponse);
                    _logger.LogInformation("✅ [GetUserInfo] Returning empty response (no authenticated user)");
                    return response;
                }

                result = await _userService.GetUserAsync(clientPrincipal);
                _logger.LogInformation("🔍 [GetUserInfo] Looked up current user using client principal");
            }
            else
            {
                // Get user by ID (this endpoint is for specific user IDs)
                _logger.LogInformation("🔍 [GetUserInfo] Looking up user by ID: {UserId}", userId);
                result = await _userService.GetUserByIdAsync(userId);
            }

            _logger.LogInformation("📊 [GetUserInfo] User lookup result - Success: {IsSuccess}, Status: {Status}, HasData: {HasData}, ErrorMessage: {ErrorMessage}, IsMeRequest: {IsMeRequest}",
                result.IsSuccess, result.Status, result.Data != null, result.ErrorMessage ?? "none", userId.Equals("me", StringComparison.OrdinalIgnoreCase));

            if (result.IsSuccess && result.Data != null)
            {
                // Map to GetUserInfoResponse format (matches old TypeScript API)
                var getUserInfoResponse = new Models.DTOs.Responses.GetUserInfoResponse
                {
                    ClientPrincipal = clientPrincipal,
                    Subscription = result.Data.Subscription,
                    TrialSubscription = result.Data.TrialSubscription,
                    UserRole = result.Data.UserRole?.ToString(),
                    NumberOfThemes = result.Data.Themes?.Count,
                    NumberOfPublishedItems = result.Data.Published?.Count,
                    HasActiveAzureSubscription = !string.IsNullOrEmpty(result.Data.AzureSubscriptionId)
                };

                _logger.LogInformation("✅ [GetUserInfo] Successfully retrieved user - UserId: {UserId}, Role: {Role}, Themes: {Themes}, Published: {Published}",
                    result.Data.UserId,
                    getUserInfoResponse.UserRole,
                    getUserInfoResponse.NumberOfThemes,
                    getUserInfoResponse.NumberOfPublishedItems);

                // Wrap in ApiResponse format expected by frontend
                var apiResponse = new
                {
                    success = true,
                    resultObject = getUserInfoResponse
                };

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(apiResponse);
                return response;
            }
            else if (userId.Equals("me", StringComparison.OrdinalIgnoreCase) && result.Status == HttpStatusCode.NotFound)
            {
                // For "me" requests, if user not found, return empty response with client principal (matches old API behavior)
                _logger.LogWarning("⚠️ [GetUserInfo] User not found for 'me' request - returning empty response with client principal");
                var emptyResponse = new Models.DTOs.Responses.GetUserInfoResponse
                {
                    ClientPrincipal = clientPrincipal,
                    Subscription = null,
                    TrialSubscription = null,
                    UserRole = null,
                    NumberOfThemes = 0,
                    NumberOfPublishedItems = 0,
                    HasActiveAzureSubscription = false
                };

                // Wrap in ApiResponse format expected by frontend
                var apiResponse = new
                {
                    success = true,
                    resultObject = emptyResponse
                };

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(apiResponse);
                _logger.LogInformation("✅ [GetUserInfo] Returning empty response for 'me' request (user not in database)");
                return response;
            }
            else
            {
                _logger.LogWarning("❌ [GetUserInfo] Failed to retrieve user - Status: {Status}, Error: {Error}",
                    result.Status, result.ErrorMessage ?? "Unknown error");

                // Wrap error in ApiResponse format expected by frontend
                var errorApiResponse = new
                {
                    success = false,
                    errorMessage = result.ErrorMessage ?? "Error retrieving user info",
                    resultObject = (object?)null
                };

                var response = req.CreateResponse(result.Status);
                await response.WriteAsJsonAsync(errorApiResponse);
                return response;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 [GetUserInfo] Exception occurred for user: {UserId}", userId);

            // Wrap error in ApiResponse format expected by frontend
            var errorApiResponse = new
            {
                success = false,
                errorMessage = "Internal server error",
                resultObject = (object?)null
            };

            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteAsJsonAsync(errorApiResponse);
            return response;
        }
    }

    [Function("CreateUser")]
    public async Task<HttpResponseData> CreateUser(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RouteConstants.Users)] HttpRequestData req)
    {
        _logger.LogInformation("Processing CreateUser request");

        try
        {
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

                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteAsJsonAsync(errorResponse);
                return badRequestResponse;
            }

            CreateUserRequest? createUserRequest;
            try
            {
                createUserRequest = JsonSerializer.Deserialize<CreateUserRequest>(requestBody, new JsonSerializerOptions
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

                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteAsJsonAsync(errorResponse);
                return badRequestResponse;
            }

            if (createUserRequest == null)
            {
                var errorResponse = ValidationHelper.CreateErrorResponse(
                    "Invalid request body",
                    "NULL_REQUEST");

                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteAsJsonAsync(errorResponse);
                return badRequestResponse;
            }

            var (isValid, validationErrors) = ValidationHelper.TryValidateDto(createUserRequest);
            if (!isValid)
            {
                var validationErrorResponse = ValidationHelper.CreateValidationErrorResponse(validationErrors);
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteAsJsonAsync(validationErrorResponse);
                return badRequestResponse;
            }

            var result = await _userService.CreateUserAsync(createUserRequest);

            if (result.IsSuccess && result.Data != null)
            {
                var createUserResponse = new CreateUserResponse
                {
                    UserId = result.Data.IDPUserId,
                    UserName = result.Data.UserName,
                    Environment = result.Data.Environment,
                    CreatedAt = DateTime.UtcNow,
                    DefaultRole = result.Data.UserRole ?? UserRole.User,
                    Success = true,
                    Message = "User created successfully",
                    Timestamp = DateTime.UtcNow
                };

                var response = req.CreateResponse(HttpStatusCode.Created);
                await response.WriteAsJsonAsync(createUserResponse);
                return response;
            }
            else
            {
                var errorResponse = ValidationHelper.CreateErrorResponse(
                    result.ErrorMessage ?? "Error creating user",
                    "USER_CREATION_ERROR");

                var response = req.CreateResponse(result.Status);
                await response.WriteAsJsonAsync(errorResponse);
                return response;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CreateUser");
            var errorResponse = ValidationHelper.CreateErrorResponse(
                "Internal server error",
                "INTERNAL_ERROR",
                ex);

            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteAsJsonAsync(errorResponse);
            return response;
        }
    }

    [Function("PostLoginEvent")]
    public async Task<HttpResponseData> PostLoginEvent(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RouteConstants.UserLoginEvent)] HttpRequestData req)
    {
        _logger.LogInformation("Processing PostLoginEvent request");

        try
        {
            var clientPrincipal = StaticWebAppsAuth.Parse(req);
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var loginEventRequest = JsonSerializer.Deserialize<LoginEventRequest>(requestBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (loginEventRequest == null)
            {
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteStringAsync("Invalid request body");
                return badRequestResponse;
            }

            loginEventRequest.UserId = clientPrincipal.UserId ?? "anonymous";
            var result = await _userService.PostLoginEventAsync(loginEventRequest);

            var response = req.CreateResponse(result.IsSuccess ? HttpStatusCode.OK : result.Status);
            if (result.IsSuccess)
            {
                await response.WriteAsJsonAsync(result.Data);
            }
            else
            {
                await response.WriteStringAsync(result.ErrorMessage ?? "Error posting login event");
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in PostLoginEvent");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync("Internal server error");
            return errorResponse;
        }
    }
}
