using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using PowerBITips.Api.Models.DTOs.Common;
using System.Net;
using Microsoft.Azure.Functions.Worker.Http;
using System.Text;
using System.Text.Json;

namespace PowerBITips.Api.Middleware;

/// <summary>
/// Global exception handler for unified error responses, following entelexos pattern.
/// </summary>
public class ExceptionHandlingMiddleware : IFunctionsWorkerMiddleware
{
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception in function {FunctionName}",
                context.FunctionDefinition.Name);

            // Attempt to set error response if HTTP trigger
            var httpContext = context.GetHttpContext();
            if (httpContext != null)
            {
                var errorResponse = new ErrorResponseDto
                {
                    Success = false,
                    Message = IsDevEnvironment()
                        ? ex.Message
                        : "An error occurred while processing your request",
                    ErrorCode = "INTERNAL_ERROR",
                    Timestamp = DateTime.UtcNow,
                    Details = IsDevEnvironment() ? ex.StackTrace : null
                };

                httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                httpContext.Response.ContentType = "application/json";

                var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                var bodyBytes = Encoding.UTF8.GetBytes(jsonResponse);
                await httpContext.Response.Body.WriteAsync(bodyBytes, 0, bodyBytes.Length);
            }
        }
    }

    private static bool IsDevEnvironment()
    {
        var env = Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT");
        return env == "Development" || env == "Local";
    }

    /// <summary>
    /// Determines if middleware should run (always true for exception handling)
    /// </summary>
    public static bool ShouldRun(FunctionContext context)
    {
        return true;
    }
}
