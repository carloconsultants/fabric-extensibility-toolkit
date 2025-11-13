using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using PowerBITips.Api.Utilities.Helpers;

namespace PowerBITips.Api.Middleware;

/// <summary>
/// Extracts and caches client principal from Static Web Apps authentication headers.
/// Follows entelexos pattern for centralized auth context retrieval.
/// </summary>
public class RetrieveUserClientPrincipalMiddleware : IFunctionsWorkerMiddleware
{
    private readonly ILogger<RetrieveUserClientPrincipalMiddleware> _logger;

    public RetrieveUserClientPrincipalMiddleware(ILogger<RetrieveUserClientPrincipalMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var httpContext = context.GetHttpContext();

        if (httpContext != null)
        {
            try
            {
                // Parse client principal from request headers
                var clientPrincipal = StaticWebAppsAuth.Parse(httpContext.Request);

                // Store in function context items for downstream access
                context.Items["ClientPrincipal"] = clientPrincipal;

                if (clientPrincipal.UserId != null)
                {
                    _logger.LogDebug("Retrieved client principal for user: {UserId}",
                        clientPrincipal.UserId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse client principal");
                // Continue execution even if auth parsing fails (anonymous allowed)
            }
        }

        await next(context);
    }

    /// <summary>
    /// Determines if middleware should run (always true for auth context retrieval)
    /// </summary>
    public static bool ShouldRun(FunctionContext context)
    {
        return true;
    }
}
