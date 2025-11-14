using System.Text.Json.Serialization;

namespace PowerBITips.Api.Models.DTOs.Common;

/// <summary>
/// Standardized API response wrapper that matches frontend ApiResponse<T> interface
/// </summary>
/// <typeparam name="T">The type of data being returned</typeparam>
public class ApiResponseDto<T>
{
    /// <summary>
    /// Indicates if the operation was successful
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; set; } = true;

    /// <summary>
    /// The actual data payload
    /// </summary>
    [JsonPropertyName("resultObject")]
    public T ResultObject { get; set; } = default!;

    /// <summary>
    /// Error message if the operation failed
    /// </summary>
    [JsonPropertyName("errorMessage")]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Create a successful response
    /// </summary>
    public static ApiResponseDto<T> CreateSuccess(T data)
    {
        return new ApiResponseDto<T>
        {
            Success = true,
            ResultObject = data,
            ErrorMessage = null
        };
    }

    /// <summary>
    /// Create an error response
    /// </summary>
    public static ApiResponseDto<T> CreateError(string errorMessage, T? data = default)
    {
        return new ApiResponseDto<T>
        {
            Success = false,
            ResultObject = data!,
            ErrorMessage = errorMessage
        };
    }
}