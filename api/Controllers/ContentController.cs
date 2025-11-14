using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using PowerBITips.Api.Core;
using PowerBITips.Api.Extensions;
using PowerBITips.Api.Middleware;
using PowerBITips.Api.Models.Content;
using PowerBITips.Api.Models.Constants;
using PowerBITips.Api.Models.DTOs.Common;
using PowerBITips.Api.Models.DTOs.Responses;
using PowerBITips.Api.Services;
using System.Net;
using System.Text.Json;

namespace PowerBITips.Api.Controllers;

public class ContentController
{
    private readonly IContentManagementService _contentManagementService;
    private readonly ILogger<ContentController> _logger;

    public ContentController(
        IContentManagementService contentManagementService,
        ILogger<ContentController> logger)
    {
        _contentManagementService = contentManagementService;
        _logger = logger;
    }

    [Function("GetYoutubeLink")]
    public async Task<HttpResponseData> GetYoutubeLink(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RouteConstants.ResourcesYoutube)] HttpRequestData req)
    {
        _logger.LogInformation("Processing GetYoutubeLink request");

        try
        {
            var result = await _contentManagementService.GetYoutubeLinkAsync();

            if (result != null && !string.IsNullOrEmpty(result.YoutubeUrl))
            {
                _logger.LogInformation("Successfully retrieved YouTube link");
                return await req.CreateStandardResponseAsync(
                    success: true,
                    data: result.YoutubeUrl,
                    statusCode: HttpStatusCode.OK
                );
            }
            else
            {
                _logger.LogWarning("YouTube link not found or empty");
                return await req.CreateStandardResponseAsync(
                    success: false,
                    data: (object?)null,
                    errorMessage: "YouTube link not found",
                    statusCode: HttpStatusCode.NotFound
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetYoutubeLink request");
            return await req.CreateStandardResponseAsync(
                success: false,
                data: (object?)null,
                errorMessage: "Internal server error occurred",
                statusCode: HttpStatusCode.InternalServerError
            );
        }
    }

    [Function("GetSharedResources")]
    public async Task<HttpResponseData> GetSharedResources(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RouteConstants.ResourcesShared)] HttpRequestData req)
    {
        _logger.LogInformation("Processing GetSharedResources request");

        try
        {
            // Parse query parameters
            var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
            var requestedResourcesParam = query["requestedResources"];

            if (string.IsNullOrEmpty(requestedResourcesParam))
            {
                _logger.LogWarning("Missing requestedResources query parameter");
                return await req.CreateStandardResponseAsync(
                    success: false,
                    data: (object?)null,
                    errorMessage: "Missing required parameter: requestedResources",
                    statusCode: HttpStatusCode.BadRequest
                );
            }

            var requestedResources = requestedResourcesParam.Split(',', StringSplitOptions.RemoveEmptyEntries);

            if (!requestedResources.Any())
            {
                _logger.LogWarning("Empty requestedResources parameter");
                return await req.CreateStandardResponseAsync(
                    success: false,
                    data: (object?)null,
                    errorMessage: "RequestedResources parameter cannot be empty",
                    statusCode: HttpStatusCode.BadRequest
                );
            }

            var request = new GetSharedResourcesRequest
            {
                RequestedResources = requestedResources
            };

            // Validate the DTO
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

            _logger.LogInformation("Requested resources: {RequestedResources}", string.Join(", ", requestedResources));

            var result = await _contentManagementService.GetSharedResourcesAsync(request);

            if (result == null || !result.SharedResources.Any())
            {
                _logger.LogInformation("No shared resources found");
                return await req.CreateStandardResponseAsync(
                    success: false,
                    data: (object?)null,
                    errorMessage: "No shared resources found",
                    statusCode: HttpStatusCode.NotFound
                );
            }

            _logger.LogInformation("Successfully retrieved {Count} shared resources", result.SharedResources.Count);
            return await req.CreateStandardResponseAsync(
                success: true,
                data: result,
                statusCode: HttpStatusCode.OK
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetSharedResources request");
            return await req.CreateStandardResponseAsync(
                success: false,
                data: (object?)null,
                errorMessage: "Internal server error occurred",
                statusCode: HttpStatusCode.InternalServerError
            );
        }
    }
}