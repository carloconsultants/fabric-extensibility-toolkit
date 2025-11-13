using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PowerBITips.Api.Extensions;
using PowerBITips.Api.Middleware;
using System.Text.Json;

// Upgraded to Worker 2.0 + AspNetCore integration following entelexos pattern
var builder = FunctionsApplication.CreateBuilder(args);

// Enable ASP.NET Core integration for full HttpContext support
builder.ConfigureFunctionsWebApplication();

// Configure JSON serialization globally for camelCase responses
builder.Services.Configure<JsonSerializerOptions>(options =>
{
    options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
    options.PropertyNameCaseInsensitive = true;
    options.WriteIndented = false; // Compact JSON for production
    options.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
});

// Log startup configuration status (using Console for early startup before logger is available)
Console.WriteLine("Starting PowerBITips API...");
var environment = builder.Configuration["APP_ENVIRONMENT"] ?? "Not set";
Console.WriteLine($"Environment: {environment}");

// Check critical configuration
var azureWebJobsStorage = builder.Configuration.GetConnectionString("AzureWebJobsStorage")
    ?? Environment.GetEnvironmentVariable("AzureWebJobsStorage")
    ?? builder.Configuration["AzureWebJobsStorage"];

if (string.IsNullOrWhiteSpace(azureWebJobsStorage))
{
    Console.WriteLine("WARNING: AzureWebJobsStorage not found - checking STORAGE_CONNECTION_STRING fallback...");
    var storageConnectionString = builder.Configuration.GetConnectionString("STORAGE_CONNECTION_STRING")
        ?? Environment.GetEnvironmentVariable("STORAGE_CONNECTION_STRING")
        ?? builder.Configuration["STORAGE_CONNECTION_STRING"];

    if (string.IsNullOrWhiteSpace(storageConnectionString))
    {
        Console.WriteLine("CRITICAL: No storage connection string found. Azure Functions may fail to start.");
        Console.WriteLine("   Please add 'AzureWebJobsStorage' to local.settings.json Values section");
    }
    else
    {
        Console.WriteLine("WARNING: Found STORAGE_CONNECTION_STRING but not AzureWebJobsStorage. Consider adding AzureWebJobsStorage to avoid runtime warnings.");
    }
}
else
{
    Console.WriteLine("AzureWebJobsStorage connection string found");
}

// Middleware pipeline: order matters (logging -> exception -> auth)
builder.UseWhen<RequestLoggingMiddleware>((context) => RequestLoggingMiddleware.ShouldRun(context));
builder.UseWhen<ExceptionHandlingMiddleware>((context) => ExceptionHandlingMiddleware.ShouldRun(context));
builder.UseWhen<RetrieveUserClientPrincipalMiddleware>((context) => RetrieveUserClientPrincipalMiddleware.ShouldRun(context));

// Register layered services via extension methods
Console.WriteLine("Registering services...");
try
{
    builder.Services
        .AddCoreInfrastructure(builder.Configuration)
        .AddDomainServices()
        .AddResilience(builder.Configuration)
        .AddTelemetry(builder.Configuration);

    Console.WriteLine("Services registered successfully");
}
catch (Exception ex)
{
    Console.WriteLine($"Failed to register services: {ex.Message}");
    Console.WriteLine($"   Stack trace: {ex.StackTrace}");
    throw;
}

var app = builder.Build();
Console.WriteLine("Application built successfully");
Console.WriteLine("Starting HTTP server...");
app.Run();