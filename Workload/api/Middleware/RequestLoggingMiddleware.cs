using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace PowerBITips.Api.Middleware;

/// <summary>
/// Logs incoming requests and execution time, following entelexos pattern.
/// </summary>
public class RequestLoggingMiddleware : IFunctionsWorkerMiddleware
{
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(ILogger<RequestLoggingMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var stopwatch = Stopwatch.StartNew();
        var functionName = context.FunctionDefinition.Name;

        _logger.LogInformation("Function {FunctionName} started at {StartTime}",
            functionName,
            DateTime.UtcNow);

        try
        {
            await next(context);
        }
        finally
        {
            stopwatch.Stop();
            _logger.LogInformation("Function {FunctionName} completed in {ElapsedMs}ms",
                functionName,
                stopwatch.ElapsedMilliseconds);
        }
    }

    /// <summary>
    /// Determines if middleware should run (always true for logging)
    /// </summary>
    public static bool ShouldRun(FunctionContext context)
    {
        return true;
    }
}
