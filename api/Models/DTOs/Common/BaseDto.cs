using System.ComponentModel.DataAnnotations;

namespace PowerBITips.Api.Models.DTOs.Common;

/// <summary>
/// Base class for all API response DTOs providing standardized response structure
/// </summary>
public abstract class BaseResponseDto
{
    /// <summary>
    /// Indicates if the operation was successful
    /// </summary>
    public bool Success { get; set; } = true;

    /// <summary>
    /// Human-readable message describing the result
    /// </summary>
    public string Message { get; set; } = "Operation completed successfully";

    /// <summary>
    /// Timestamp when the response was generated
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Optional correlation ID for request tracking
    /// </summary>
    public string? CorrelationId { get; set; }
}

/// <summary>
/// Base class for paginated response DTOs
/// </summary>
/// <typeparam name="T">Type of items in the collection</typeparam>
public class PaginatedResponseDto<T> : BaseResponseDto
{
    /// <summary>
    /// Collection of items for the current page
    /// </summary>
    public List<T> Items { get; set; } = new();

    /// <summary>
    /// Total number of items across all pages
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Current page number (1-based)
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;

    /// <summary>
    /// Indicates if there are more pages after the current one
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;

    /// <summary>
    /// Indicates if there are pages before the current one
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;
}

/// <summary>
/// Base class for all API request DTOs providing common validation attributes
/// </summary>
public abstract class BaseRequestDto
{
    /// <summary>
    /// Optional correlation ID for request tracking
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Timestamp when the request was created (client-side)
    /// </summary>
    public DateTime? RequestTimestamp { get; set; }
}

/// <summary>
/// Standard error response DTO for consistent error handling
/// </summary>
public class ErrorResponseDto : BaseResponseDto
{
    public ErrorResponseDto()
    {
        Success = false;
        Message = "An error occurred";
    }

    public ErrorResponseDto(string message) : this()
    {
        Message = message;
    }

    public ErrorResponseDto(string message, string errorCode) : this(message)
    {
        ErrorCode = errorCode;
    }

    /// <summary>
    /// Specific error code for programmatic error handling
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Detailed error information for debugging
    /// </summary>
    public Dictionary<string, object>? ErrorDetails { get; set; }

    /// <summary>
    /// List of validation errors
    /// </summary>
    public List<string>? Errors { get; set; }

    /// <summary>
    /// Additional error details
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// Collection of validation errors
    /// </summary>
    public List<ValidationError>? ValidationErrors { get; set; }

    /// <summary>
    /// Stack trace information (only for development environments)
    /// </summary>
    public string? StackTrace { get; set; }
}

/// <summary>
/// Class to represent validation errors in API responses
/// </summary>
public class ValidationError
{
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public object? Value { get; set; }
}

/// <summary>
/// Standard success response DTO for operations that don't return specific data
/// </summary>
public class SuccessResponseDto : BaseResponseDto
{
    public SuccessResponseDto() : base() { }

    public SuccessResponseDto(string message) : base()
    {
        Message = message;
    }

    /// <summary>
    /// Optional data payload for successful operations
    /// </summary>
    public Dictionary<string, object>? Data { get; set; }
}

/// <summary>
/// Base class for DTOs that include audit information
/// </summary>
public abstract class AuditableDto
{
    /// <summary>
    /// When the entity was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Who created the entity
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// When the entity was last modified
    /// </summary>
    public DateTime? LastModifiedAt { get; set; }

    /// <summary>
    /// Who last modified the entity
    /// </summary>
    public string? LastModifiedBy { get; set; }

    /// <summary>
    /// Version number for optimistic concurrency control
    /// </summary>
    public string? Version { get; set; }
}