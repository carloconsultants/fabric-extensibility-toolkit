using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using PowerBITips.Api.Models.Constants;
using PowerBITips.Api.Models.DTOs.Common;
using PowerBITips.Api.Models.Embed;
using PowerBITips.Api.Services;
using PowerBITips.Api.Utilities.Helpers;
using PowerBITips.Api.Middleware;
using PowerBITips.Api.Extensions;

namespace PowerBITips.Api.Controllers;

public class AuthController
{
    private readonly ILogger<AuthController> _logger;
    private readonly IPowerBiEmbedService _embedService;

    public AuthController(
        ILogger<AuthController> logger,
        IPowerBiEmbedService embedService)
    {
        _logger = logger;
        _embedService = embedService;
    }

    [Function("UserOboExchange")]
    public async Task<HttpResponseData> UserOboExchange(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RouteConstants.AuthTokenPowerBI)] HttpRequestData req)
    {
        try
        {
            _logger.LogInformation("Processing user OBO exchange request for PowerBI");

            // Read and deserialize request body
            var requestBody = await req.ReadAsStringAsync();
            if (string.IsNullOrEmpty(requestBody))
            {
                _logger.LogWarning("Request body is empty");
                var errorResponse = ValidationHelper.CreateErrorResponse(
                    "Request body is required",
                    "EMPTY_REQUEST");

                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteAsJsonAsync(errorResponse);
                return badRequestResponse;
            }

            OboExchangeRequest? oboRequest;
            try
            {
                oboRequest = JsonSerializer.Deserialize<OboExchangeRequest>(requestBody, new JsonSerializerOptions
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

                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteAsJsonAsync(errorResponse);
                return badRequestResponse;
            }

            if (oboRequest == null)
            {
                var errorResponse = ValidationHelper.CreateErrorResponse(
                    "Invalid request body",
                    "NULL_REQUEST");

                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteAsJsonAsync(errorResponse);
                return badRequestResponse;
            }

            // Validate the DTO
            var (isValid, validationErrors) = ValidationHelper.TryValidateDto(oboRequest);
            if (!isValid)
            {
                var validationErrorResponse = ValidationHelper.CreateValidationErrorResponse(validationErrors);
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteAsJsonAsync(validationErrorResponse);
                return badRequestResponse;
            }

            if (string.IsNullOrEmpty(oboRequest.Token))
            {
                _logger.LogWarning("Token is missing from request");
                var errorResponse = ValidationHelper.CreateErrorResponse(
                    "token is required",
                    "MISSING_TOKEN");

                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteAsJsonAsync(errorResponse);
                return badRequestResponse;
            }

            // Exchange token for PowerBI (using scope instead of resource)
            var exchangeResult = await _embedService.ExchangeUserTokenAsync(oboRequest.Token);

            if (!exchangeResult.IsSuccess)
            {
                _logger.LogWarning("Could not exchange user token for PowerBI. Error: {ErrorCode}, Description: {Description}",
                    exchangeResult.ErrorCode, exchangeResult.ErrorDescription);

                // Determine appropriate HTTP status code based on error type
                var statusCode = DetermineHttpStatusCode(exchangeResult.ErrorCode);
                var errorResponse = ValidationHelper.CreateErrorResponse(
                    exchangeResult.ErrorDescription ?? "Could not exchange user token",
                    exchangeResult.ErrorCode ?? "TOKEN_EXCHANGE_FAILED");

                // Add Azure AD error details if available
                if (!string.IsNullOrEmpty(exchangeResult.AzureAdErrorCode))
                {
                    errorResponse.ErrorDetails = new Dictionary<string, object>
                    {
                        { "azureAdErrorCode", exchangeResult.AzureAdErrorCode },
                        { "azureAdErrorDescription", exchangeResult.AzureAdErrorDescription ?? string.Empty }
                    };
                }

                var exchangeErrorResponse = req.CreateResponse(statusCode);
                await exchangeErrorResponse.WriteAsJsonAsync(errorResponse);
                return exchangeErrorResponse;
            }

            var response = new OboExchangeResponse { Token = exchangeResult.AccessToken! };

            _logger.LogInformation("Successfully exchanged user token for PowerBI");
            return await req.CreateStandardResponseAsync(
                success: true,
                data: response,
                statusCode: HttpStatusCode.OK
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing user OBO exchange request for PowerBI");

            return await req.CreateStandardResponseAsync(
                success: false,
                data: (object?)null,
                errorMessage: "Internal server error occurred",
                statusCode: HttpStatusCode.InternalServerError
            );
        }
    }

    [Function("UserOneLakeOboExchange")]
    public async Task<HttpResponseData> UserOneLakeOboExchange(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RouteConstants.AuthTokenOneLake)] HttpRequestData req)
    {
        try
        {
            _logger.LogInformation("Processing user OneLake OBO exchange request");

            // Read and deserialize request body
            var requestBody = await req.ReadAsStringAsync();
            if (string.IsNullOrEmpty(requestBody))
            {
                _logger.LogWarning("Request body is empty");
                var errorResponse = ValidationHelper.CreateErrorResponse(
                    "Request body is required",
                    "EMPTY_REQUEST");

                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteAsJsonAsync(errorResponse);
                return badRequestResponse;
            }

            OboExchangeRequest? oboRequest;
            try
            {
                oboRequest = JsonSerializer.Deserialize<OboExchangeRequest>(requestBody, new JsonSerializerOptions
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

                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteAsJsonAsync(errorResponse);
                return badRequestResponse;
            }

            if (oboRequest == null)
            {
                var errorResponse = ValidationHelper.CreateErrorResponse(
                    "Invalid request body",
                    "NULL_REQUEST");

                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteAsJsonAsync(errorResponse);
                return badRequestResponse;
            }

            // Validate the DTO
            var (isValid, validationErrors) = ValidationHelper.TryValidateDto(oboRequest);
            if (!isValid)
            {
                var validationErrorResponse = ValidationHelper.CreateValidationErrorResponse(validationErrors);
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteAsJsonAsync(validationErrorResponse);
                return badRequestResponse;
            }

            if (string.IsNullOrEmpty(oboRequest.Token))
            {
                _logger.LogWarning("Token is missing from request");
                var errorResponse = ValidationHelper.CreateErrorResponse(
                    "token is required",
                    "MISSING_TOKEN");

                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteAsJsonAsync(errorResponse);
                return badRequestResponse;
            }

            // Exchange token for Azure Storage resource (OneLake)
            var exchangeResult = await _embedService.ExchangeUserTokenForResourceAsync(oboRequest.Token, "https://storage.azure.com/");

            if (!exchangeResult.IsSuccess)
            {
                _logger.LogWarning("Could not exchange user token for OneLake. Error: {ErrorCode}, Description: {Description}",
                    exchangeResult.ErrorCode, exchangeResult.ErrorDescription);

                // Determine appropriate HTTP status code based on error type
                var statusCode = DetermineHttpStatusCode(exchangeResult.ErrorCode);
                var errorResponse = ValidationHelper.CreateErrorResponse(
                    exchangeResult.ErrorDescription ?? "Could not exchange user token for OneLake",
                    exchangeResult.ErrorCode ?? "TOKEN_EXCHANGE_FAILED");

                // Add Azure AD error details if available
                if (!string.IsNullOrEmpty(exchangeResult.AzureAdErrorCode))
                {
                    errorResponse.ErrorDetails = new Dictionary<string, object>
                    {
                        { "azureAdErrorCode", exchangeResult.AzureAdErrorCode },
                        { "azureAdErrorDescription", exchangeResult.AzureAdErrorDescription ?? string.Empty }
                    };
                }

                var exchangeErrorResponse = req.CreateResponse(statusCode);
                await exchangeErrorResponse.WriteAsJsonAsync(errorResponse);
                return exchangeErrorResponse;
            }

            var response = new OboExchangeResponse { Token = exchangeResult.AccessToken! };

            _logger.LogInformation("Successfully exchanged user token for OneLake access");
            return await req.CreateStandardResponseAsync(
                success: true,
                data: response,
                statusCode: HttpStatusCode.OK
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing user OneLake OBO exchange request");

            return await req.CreateStandardResponseAsync(
                success: false,
                data: (object?)null,
                errorMessage: "Internal server error occurred",
                statusCode: HttpStatusCode.InternalServerError
            );
        }
    }

    /// <summary>
    /// Determines the appropriate HTTP status code based on the error code from token exchange
    /// </summary>
    private static HttpStatusCode DetermineHttpStatusCode(string? errorCode)
    {
        if (string.IsNullOrEmpty(errorCode))
        {
            return HttpStatusCode.BadRequest;
        }

        // Authorization/authentication errors should return 401 Unauthorized
        if (errorCode == "CONSENT_REQUIRED" ||
            errorCode == "UNAUTHORIZED_CLIENT" ||
            errorCode == "INVALID_TOKEN" ||
            errorCode == "INVALID_SCOPE")
        {
            return HttpStatusCode.Unauthorized;
        }

        // Configuration errors should return 500 Internal Server Error
        if (errorCode == "CONFIGURATION_ERROR" ||
            errorCode == "APPLICATION_NOT_FOUND" ||
            errorCode == "INVALID_CLIENT_SECRET")
        {
            return HttpStatusCode.InternalServerError;
        }

        // All other errors are treated as 400 Bad Request
        return HttpStatusCode.BadRequest;
    }
}
