using Microsoft.Azure.Functions.Worker.Http;
using PowerBITips.Api.Models.DTOs.Common;
using System.Net;
using System.Text.Json;

namespace PowerBITips.Api.Extensions;

/// <summary>
/// Extension methods for HttpResponseData to ensure consistent JSON serialization
/// </summary>
public static class HttpResponseExtensions
{
    /// <summary>
    /// Default JSON serialization options with camelCase naming policy
    /// </summary>
    private static readonly JsonSerializerOptions DefaultJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Writes JSON response with consistent camelCase formatting
    /// </summary>
    /// <typeparam name="T">Type of object to serialize</typeparam>
    /// <param name="response">HTTP response</param>
    /// <param name="value">Object to serialize</param>
    /// <param name="statusCode">HTTP status code (optional, defaults to current status)</param>
    /// <returns>Task</returns>
    public static async Task WriteJsonAsync<T>(this HttpResponseData response, T value, HttpStatusCode? statusCode = null)
    {
        if (statusCode.HasValue)
        {
            response.StatusCode = statusCode.Value;
        }

        response.Headers.Add("Content-Type", "application/json; charset=utf-8");

        var jsonString = JsonSerializer.Serialize(value, DefaultJsonOptions);
        await response.WriteStringAsync(jsonString);
    }

    /// <summary>
    /// Creates a standardized API response with camelCase formatting
    /// </summary>
    /// <typeparam name="T">Type of data to include in response</typeparam>
    /// <param name="req">HTTP request data</param>
    /// <param name="success">Whether the operation was successful</param>
    /// <param name="data">Data to include in response</param>
    /// <param name="errorMessage">Error message if applicable</param>
    /// <param name="statusCode">HTTP status code</param>
    /// <returns>HTTP response with standardized format</returns>
    public static async Task<HttpResponseData> CreateStandardResponseAsync<T>(
        this HttpRequestData req,
        bool success,
        T? data = default,
        string? errorMessage = null,
        HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var response = req.CreateResponse(statusCode);

        // Use the actual ApiResponseDto<T> for consistency
        var standardResponse = new ApiResponseDto<T>
        {
            Success = success,
            ResultObject = data!,
            ErrorMessage = errorMessage
        };

        await response.WriteJsonAsync(standardResponse);
        return response;
    }
}