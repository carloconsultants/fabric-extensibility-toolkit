using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using PowerBITips.Api.Extensions;
using PowerBITips.Api.Middleware;
using PowerBITips.Api.Models.Constants;
using PowerBITips.Api.Models.DTOs.Common;
using PowerBITips.Api.Models.DTOs.Responses;
using PowerBITips.Api.Models.Embed;
using PowerBITips.Api.Services;
using PowerBITips.Api.Utilities.Helpers;

namespace PowerBITips.Api.Controllers;

public class EmbedController
{
    private readonly ILogger<EmbedController> _logger;
    private readonly IPowerBiEmbedService _embedService;

    public EmbedController(
        ILogger<EmbedController> logger,
        IPowerBiEmbedService embedService)
    {
        _logger = logger;
        _embedService = embedService;
    }

    [Function("EmbedAccess")]
    public async Task<HttpResponseData> EmbedAccess(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RouteConstants.EmbedReportToken)] HttpRequestData req,
        string reportId)
    {
        try
        {
            _logger.LogInformation("Processing embed access request for report: {ReportId}", reportId);

            // Validate reportId parameter
            if (string.IsNullOrWhiteSpace(reportId))
            {
                _logger.LogWarning("ReportId parameter is missing or empty");
                return await req.CreateStandardResponseAsync(
                    success: false,
                    data: (object?)null,
                    errorMessage: "Report ID is required",
                    statusCode: HttpStatusCode.BadRequest
                );
            }

            // Get configuration from environment variables
            string pbiWorkspaceId, tenantId, clientId, clientSecret;
            try
            {
                var envVars = ConfigurationHelper.GetRequiredEnvironmentVariables(
                    "EXAMPLE_PBI_WORKSPACE_ID", "TENANT_ID", "EXAMPLE_PBI_CLIENT_ID", "EXAMPLE_PBI_CLIENT_SECRET");

                pbiWorkspaceId = envVars["EXAMPLE_PBI_WORKSPACE_ID"];
                tenantId = envVars["TENANT_ID"];
                clientId = envVars["EXAMPLE_PBI_CLIENT_ID"];
                clientSecret = envVars["EXAMPLE_PBI_CLIENT_SECRET"];
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Power BI configuration missing: {Message}", ex.Message);
                return await req.CreateStandardResponseAsync(
                    success: false,
                    data: (object?)null,
                    errorMessage: "Power BI embedding is not configured for cross-tenant access. This demo report requires service principal authentication.",
                    statusCode: HttpStatusCode.ServiceUnavailable
                );
            }

            // Get service principal token for accessing PowerBI-tips tenant reports
            var clientTenantTokenRequest = new ClientTenantTokenRequest
            {
                TenantId = tenantId,
                AppRegistration = new AppRegistration { ClientId = clientId, ClientSecret = clientSecret }
            };

            var clientTenantToken = await _embedService.GetClientTenantTokenAsync(clientTenantTokenRequest);
            if (string.IsNullOrEmpty(clientTenantToken))
            {
                _logger.LogWarning("Could not get service principal token for PowerBI-tips tenant");
                return await req.CreateStandardResponseAsync(
                    success: false,
                    data: (object?)null,
                    errorMessage: "Could not authenticate to Power BI service",
                    statusCode: HttpStatusCode.ServiceUnavailable
                );
            }

            // Get report info using service principal token
            var reportInfoRequest = new ReportInfoRequest
            {
                Token = clientTenantToken,
                PbiWorkspaceId = pbiWorkspaceId,
                PbiReportId = reportId
            };

            var reportInfo = await _embedService.GetReportInfoAsync(reportInfoRequest);
            if (reportInfo == null)
            {
                _logger.LogWarning("Could not get report info for report {ReportId} from PowerBI-tips tenant", reportId);
                return await req.CreateStandardResponseAsync(
                    success: false,
                    data: (object?)null,
                    errorMessage: "Could not access the demo report from PowerBI-tips tenant",
                    statusCode: HttpStatusCode.BadRequest
                );
            }

            // Get embed token using service principal token
            var embedTokenRequest = new EmbedTokenRequest
            {
                Token = clientTenantToken,
                PbiWorkspaceId = pbiWorkspaceId,
                PbiReportId = reportId
            };

            var embedToken = await _embedService.GetEmbedTokenAsync(embedTokenRequest);
            if (string.IsNullOrEmpty(embedToken))
            {
                _logger.LogWarning("Could not get embed token for report {ReportId} from PowerBI-tips tenant", reportId);
                return await req.CreateStandardResponseAsync(
                    success: false,
                    data: (object?)null,
                    errorMessage: "Could not get embed token for demo report",
                    statusCode: HttpStatusCode.ServiceUnavailable
                );
            }

            // Return successful response with embed token
            var response = new Models.DTOs.Responses.EmbedAccessResponse
            {
                EmbedUrl = reportInfo.EmbedUrl,
                AccessToken = embedToken
            };

            _logger.LogInformation("Successfully generated embed access for report {ReportId} using service principal authentication", reportId);

            return await req.CreateStandardResponseAsync(
                success: true,
                data: response,
                statusCode: HttpStatusCode.OK
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing embed access request");
            return await req.CreateStandardResponseAsync(
                success: false,
                data: (object?)null,
                errorMessage: "Internal server error occurred",
                statusCode: HttpStatusCode.InternalServerError
            );
        }
    }
}
