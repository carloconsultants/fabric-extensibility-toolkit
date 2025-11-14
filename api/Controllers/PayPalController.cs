using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PowerBITips.Api.Core.Interfaces;
using PowerBITips.Api.Extensions;
using PowerBITips.Api.Middleware;
using PowerBITips.Api.Models.DTOs.Common;
using PowerBITips.Api.Models.DTOs.Responses;
using PowerBITips.Api.Models.PayPal;
using PowerBITips.Api.Models.Enums;
using PowerBITips.Api.Services.Interfaces;
using PowerBITips.Api.Utilities.Helpers;
using System.Net;
using PowerBITips.Api.Models.Constants;

namespace PowerBITips.Api.Controllers;

public class PayPalController
{
    private readonly IPayPalService _paypalService;
    private readonly IUserService _userService;
    private readonly ILogger<PayPalController> _logger;

    public PayPalController(
        IPayPalService paypalService,
        IUserService userService,
        ILogger<PayPalController> logger)
    {
        _paypalService = paypalService;
        _userService = userService;
        _logger = logger;
    }

    [Function("GetPayPalSubscriptionPlans")]
    public async Task<HttpResponseData> GetSubscriptionPlans(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RouteConstants.SubscriptionPlans)] HttpRequestData req)
    {
        try
        {
            _logger.LogInformation("Getting PayPal subscription plans");

            var result = await _paypalService.GetActiveSubscriptionPlansAsync();

            if (result.IsSuccess && result.Data != null)
            {
                return await req.CreateStandardResponseAsync(
                    success: true,
                    data: result.Data,
                    statusCode: HttpStatusCode.OK
                );
            }

            return await req.CreateStandardResponseAsync(
                success: false,
                data: (object?)null,
                errorMessage: result.ErrorMessage ?? "Failed to get subscription plans",
                statusCode: result.Status
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetSubscriptionPlans");
            return await req.CreateStandardResponseAsync(
                success: false,
                data: (object?)null,
                errorMessage: "An unexpected error occurred",
                statusCode: HttpStatusCode.InternalServerError
            );
        }
    }

    [Function("CreatePayPalSubscription")]
    public async Task<HttpResponseData> CreateSubscription(
    [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RouteConstants.Subscriptions)] HttpRequestData req)
    {
        try
        {
            // Parse authentication
            var clientPrincipal = StaticWebAppsAuth.Parse(req);
            if (string.IsNullOrEmpty(clientPrincipal.UserId))
            {
                return await req.CreateStandardResponseAsync(
                    success: false,
                    data: (object?)null,
                    errorMessage: "User must be authenticated",
                    statusCode: HttpStatusCode.Unauthorized
                );
            }

            // Parse and validate request body
            var body = await req.ReadAsStringAsync();
            if (string.IsNullOrEmpty(body))
            {
                return await req.CreateStandardResponseAsync(
                    success: false,
                    data: (object?)null,
                    errorMessage: "Request body is required",
                    statusCode: HttpStatusCode.BadRequest
                );
            }

            var request = JsonConvert.DeserializeObject<CreatePayPalSubscriptionRequest>(body);
            if (request == null)
            {
                return await req.CreateStandardResponseAsync(
                    success: false,
                    data: (object?)null,
                    errorMessage: "Invalid request format",
                    statusCode: HttpStatusCode.BadRequest
                );
            }

            // Validate the request DTO
            var (isValid, validationErrors) = ValidationHelper.TryValidateDto(request);
            if (!isValid)
            {
                return await req.CreateStandardResponseAsync(
                    success: false,
                    data: new { ValidationErrors = validationErrors },
                    errorMessage: "Invalid request parameters",
                    statusCode: HttpStatusCode.BadRequest
                );
            }

            _logger.LogInformation("Creating PayPal subscription for user {UserId} with subscription {SubscriptionId}",
                clientPrincipal.UserId, request.SubscriptionId);

            // Verify subscription exists in PayPal
            var subscriptionResult = await _paypalService.GetSubscriptionAsync(request.SubscriptionId ?? string.Empty);
            if (!subscriptionResult.IsSuccess || subscriptionResult.Data == null)
            {
                _logger.LogWarning("RESOURCE_NOT_FOUND - Subscription not found: {SubscriptionId}", request.SubscriptionId);
                return await req.CreateStandardResponseAsync(
                    success: false,
                    data: (object?)null,
                    errorMessage: $"Could not find subscription: {request.SubscriptionId}",
                    statusCode: HttpStatusCode.NotFound
                );
            }

            // Update user subscription
            var updateResult = await _userService.UpdateUserSubscriptionAsync(clientPrincipal, subscriptionResult.Data);
            if (!updateResult.IsSuccess)
            {
                return await req.CreateStandardResponseAsync(
                    success: false,
                    data: (object?)null,
                    errorMessage: updateResult.ErrorMessage ?? "Failed to update user subscription",
                    statusCode: updateResult.Status
                );
            }

            return await req.CreateStandardResponseAsync(
                success: true,
                data: new { success = true },
                statusCode: HttpStatusCode.OK
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CreateSubscription");
            return await req.CreateStandardResponseAsync(
                success: false,
                data: (object?)null,
                errorMessage: "An unexpected error occurred",
                statusCode: HttpStatusCode.InternalServerError
            );
        }
    }

    [Function("UpdatePayPalSubscription")]
    public async Task<HttpResponseData> UpdateSubscription(
    [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = RouteConstants.SubscriptionById)] HttpRequestData req,
    string subscriptionId)
    {
        try
        {
            // Parse authentication
            var clientPrincipal = StaticWebAppsAuth.Parse(req);
            if (string.IsNullOrEmpty(clientPrincipal.UserId))
            {
                return await req.CreateStandardResponseAsync(
                    success: false,
                    data: (object?)null,
                    errorMessage: "User must be authenticated",
                    statusCode: HttpStatusCode.Unauthorized
                );
            }

            // Parse and validate request body
            var body = await req.ReadAsStringAsync();
            if (string.IsNullOrEmpty(body))
            {
                return await req.CreateStandardResponseAsync(
                    success: false,
                    data: (object?)null,
                    errorMessage: "Request body is required",
                    statusCode: HttpStatusCode.BadRequest
                );
            }

            var request = JsonConvert.DeserializeObject<UpdatePayPalSubscriptionRequest>(body);
            if (request == null)
            {
                return await req.CreateStandardResponseAsync(
                    success: false,
                    data: (object?)null,
                    errorMessage: "Invalid request format",
                    statusCode: HttpStatusCode.BadRequest
                );
            }

            // Validate the request DTO
            var (isValid, validationErrors) = ValidationHelper.TryValidateDto(request);
            if (!isValid)
            {
                return await req.CreateStandardResponseAsync(
                    success: false,
                    data: new { ValidationErrors = validationErrors },
                    errorMessage: "Invalid request parameters",
                    statusCode: HttpStatusCode.BadRequest
                );
            }

            _logger.LogInformation("Updating PayPal subscription for user {UserId} to status {Status}",
                clientPrincipal.UserId, request.Status);

            // Get user to validate subscription ownership
            var userResult = await _userService.GetUserAsync(clientPrincipal);
            if (!userResult.IsSuccess || userResult.Data?.Subscription?.Id != subscriptionId)
            {
                return await req.CreateStandardResponseAsync(
                    success: false,
                    data: (object?)null,
                    errorMessage: "Subscription not found or unauthorized",
                    statusCode: HttpStatusCode.NotFound
                );
            }
            // Update subscription status in PayPal
            if (request.Status == PayPalSubscriptionStatus.Active)
            {
                var activateResult = await _paypalService.ActivateSubscriptionAsync(subscriptionId);
                if (!activateResult.IsSuccess)
                {
                    return await req.CreateStandardResponseAsync(
                        success: false,
                        data: (object?)null,
                        errorMessage: activateResult.ErrorMessage ?? "Failed to activate subscription",
                        statusCode: activateResult.Status
                    );
                }
            }
            else if (request.Status == PayPalSubscriptionStatus.Suspended)
            {
                var suspendResult = await _paypalService.SuspendSubscriptionAsync(subscriptionId,
                    $"User {userResult.Data.IDPUserId} suspended subscription");
                if (!suspendResult.IsSuccess)
                {
                    return await req.CreateStandardResponseAsync(
                        success: false,
                        data: (object?)null,
                        errorMessage: suspendResult.ErrorMessage ?? "Failed to suspend subscription",
                        statusCode: suspendResult.Status
                    );
                }
            }

            // Get updated subscription details and update user
            var updatedSubscriptionResult = await _paypalService.GetSubscriptionAsync(subscriptionId);
            if (updatedSubscriptionResult.IsSuccess && updatedSubscriptionResult.Data != null)
            {
                await _userService.UpdateUserSubscriptionAsync(clientPrincipal, updatedSubscriptionResult.Data);
            }

            return await req.CreateStandardResponseAsync(
                success: true,
                data: new { success = true },
                statusCode: HttpStatusCode.OK
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in UpdateSubscription");
            return await req.CreateStandardResponseAsync(
                success: false,
                data: (object?)null,
                errorMessage: "An unexpected error occurred",
                statusCode: HttpStatusCode.InternalServerError
            );
        }
    }

    [Function("CancelPayPalSubscription")]
    public async Task<HttpResponseData> CancelSubscription(
    [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = RouteConstants.SubscriptionById)] HttpRequestData req,
    string subscriptionId)
    {
        try
        {
            // Parse authentication
            var clientPrincipal = StaticWebAppsAuth.Parse(req);
            if (string.IsNullOrEmpty(clientPrincipal.UserId))
            {
                return await req.CreateStandardResponseAsync(
                    success: false,
                    data: (object?)null,
                    errorMessage: "User must be authenticated",
                    statusCode: HttpStatusCode.Unauthorized
                );
            }

            _logger.LogInformation("Canceling PayPal subscription for user {UserId}", clientPrincipal.UserId);

            // Get user to validate subscription ownership
            var userResult = await _userService.GetUserAsync(clientPrincipal);
            if (!userResult.IsSuccess || userResult.Data?.Subscription?.Id != subscriptionId)
            {
                return await req.CreateStandardResponseAsync(
                    success: false,
                    data: (object?)null,
                    errorMessage: "Subscription not found or unauthorized",
                    statusCode: HttpStatusCode.NotFound
                );
            }

            // Cancel subscription in PayPal
            var cancelResult = await _paypalService.CancelSubscriptionAsync(subscriptionId);
            if (!cancelResult.IsSuccess)
            {
                return await req.CreateStandardResponseAsync(
                    success: false,
                    data: (object?)null,
                    errorMessage: cancelResult.ErrorMessage ?? "Failed to cancel subscription",
                    statusCode: cancelResult.Status
                );
            }

            // Get updated subscription details and update user
            var updatedSubscriptionResult = await _paypalService.GetSubscriptionAsync(subscriptionId);
            if (updatedSubscriptionResult.IsSuccess && updatedSubscriptionResult.Data != null)
            {
                await _userService.UpdateUserSubscriptionAsync(clientPrincipal, updatedSubscriptionResult.Data);
            }

            return await req.CreateStandardResponseAsync(
                success: true,
                data: new { success = true },
                statusCode: HttpStatusCode.OK
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CancelSubscription");
            return await req.CreateStandardResponseAsync(
                success: false,
                data: (object?)null,
                errorMessage: "An unexpected error occurred",
                statusCode: HttpStatusCode.InternalServerError
            );
        }
    }
}