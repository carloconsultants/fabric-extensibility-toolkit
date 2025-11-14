using Microsoft.Extensions.DependencyInjection;
using PowerBITips.Api.Middleware;
using PowerBITips.Api.Models.DTOs.Common;

namespace PowerBITips.Api.Extensions;

/// <summary>
/// Extension methods for configuring DTO validation in Azure Functions
/// </summary>
public static class ValidationExtensions
{
    /// <summary>
    /// Adds DTO validation services to the service collection for Azure Functions
    /// </summary>
    public static IServiceCollection AddDtoValidation(this IServiceCollection services)
    {
        // Register validation services
        services.AddSingleton<ValidationService>();
        
        return services;
    }
}

