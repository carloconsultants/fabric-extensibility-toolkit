using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PowerBITips.Api.Core.Azure;
using PowerBITips.Api.Core.Interfaces;
using PowerBITips.Api.Services;
using PowerBITips.Api.Services.Interfaces;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using System.Security.Authentication;

namespace PowerBITips.Api.Extensions;

/// <summary>
/// Extension methods to organize dependency injection registrations following entelexos pattern.
/// Separates core infrastructure, domain services, resilience, and telemetry concerns.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers core infrastructure services (storage, key vault access, etc.)
    /// </summary>
    public static IServiceCollection AddCoreInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Azure Table Storage (singleton for connection pooling)
        services.AddSingleton<IAzureTableStorage, AzureTableStorage>();

        // Azure Blob Storage (singleton for connection pooling)
        services.AddSingleton<IBlobStorageClient, BlobStorageClient>();

        // TODO: Add KeyVaultAccess singleton if needed

        return services;
    }

    /// <summary>
    /// Configures global JSON serialization options for consistent camelCase API responses
    /// </summary>
    public static IServiceCollection AddJsonConfiguration(this IServiceCollection services)
    {
        // Configure global JSON serialization options
        services.Configure<JsonSerializerOptions>(options =>
        {
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
            options.PropertyNameCaseInsensitive = true;
            options.WriteIndented = false; // Compact JSON for production
            options.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        });

        return services;
    }

    /// <summary>
    /// Registers domain/business services (scoped to function invocation)
    /// </summary>
    public static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ThemeService>();
        services.AddScoped<IPublishedService, PublishedService>();
        services.AddScoped<IPowerBiEmbedService, PowerBiEmbedService>();
        services.AddScoped<IContentManagementService, ContentManagementService>();
        services.AddScoped<IAnalyticsManagementService, AnalyticsManagementService>();
        services.AddScoped<IPayPalService, PayPalService>();
        services.AddScoped<IWorkloadService, WorkloadService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();

        return services;
    }

    /// <summary>
    /// Registers HTTP clients with resilience policies (retry, timeout, circuit breaker)
    /// </summary>
    public static IServiceCollection AddResilience(this IServiceCollection services, IConfiguration configuration)
    {
        // Basic HttpClient registrations
        // TODO: Add Polly policies (retry with exponential backoff, circuit breaker, timeout)
        services.AddHttpClient<IPowerBiEmbedService, PowerBiEmbedService>();
        services.AddHttpClient<IAnalyticsManagementService, AnalyticsManagementService>();
        services.AddHttpClient<IPayPalService, PayPalService>();
        services.AddHttpClient<IAuthenticationService, AuthenticationService>();

        // Configure WorkloadService HttpClient to match Node.js simplicity
        services.AddHttpClient<IWorkloadService, WorkloadService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("User-Agent", "PowerBI-Tips-Workload/1.0");
        });
        // Use default HttpClientHandler - let .NET handle SSL/TLS automatically like Node.js fetch()

        return services;
    }

    /// <summary>
    /// Registers Application Insights and telemetry services
    /// </summary>
    public static IServiceCollection AddTelemetry(this IServiceCollection services, IConfiguration configuration)
    {
        // Application Insights integration (requires Microsoft.Azure.Functions.Worker.ApplicationInsights)
        services.AddApplicationInsightsTelemetryWorkerService();

        // Note: ConfigureFunctionsApplicationInsights is a host-level configuration,
        // handled in Program.cs or via host.json settings, not here in DI.

        return services;
    }
}
