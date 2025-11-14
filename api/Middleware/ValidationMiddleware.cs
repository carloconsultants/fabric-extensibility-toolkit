using Microsoft.Extensions.Logging;
using PowerBITips.Api.Models.DTOs.Common;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace PowerBITips.Api.Middleware;

/// <summary>
/// Azure Functions compatible validation service for DTOs
/// </summary>
public class ValidationService
{
    private readonly ILogger<ValidationService> _logger;

    public ValidationService(ILogger<ValidationService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Creates a standardized error response for validation exceptions
    /// </summary>
    public ErrorResponseDto HandleValidationException(ValidationException ex)
    {
        return new ErrorResponseDto
        {
            Success = false,
            Message = "Validation failed",
            ErrorCode = "VALIDATION_ERROR",
            Timestamp = DateTime.UtcNow,
            Errors = new List<string>
            {
                $"Request validation failed: {ex.Message}"
            }
        };
    }

    /// <summary>
    /// Creates a standardized error response for generic exceptions
    /// </summary>
    public ErrorResponseDto HandleGenericException(Exception ex)
    {
        var response = new ErrorResponseDto
        {
            Success = false,
            Message = "An error occurred while processing your request",
            ErrorCode = "INTERNAL_ERROR",
            Timestamp = DateTime.UtcNow
        };

        // Don't expose internal error details in production
        if (IsDevEnvironment())
        {
            response.Message = ex.Message;
            response.Details = ex.StackTrace;
        }

        return response;
    }

    private static bool IsDevEnvironment()
    {
        return Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT") == "Development" ||
               Environment.GetEnvironmentVariable("APP_ENVIRONMENT") == "local";
    }
}

/// <summary>
/// Helper class for DTO validation
/// </summary>
public static class ValidationHelper
{
    /// <summary>
    /// Validates a DTO and throws ValidationException if invalid
    /// </summary>
    public static void ValidateDto<T>(T dto) where T : class
    {
        if (dto == null)
        {
            throw new ValidationException("Request body cannot be null");
        }

        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(dto);

        if (!Validator.TryValidateObject(dto, validationContext, validationResults, true))
        {
            var errors = validationResults.Select(vr => new ValidationError
            {
                Field = string.Join(", ", vr.MemberNames),
                Message = vr.ErrorMessage ?? "Validation failed",
                Code = "VALIDATION_FAILED"
            }).ToList();

            var exception = new ValidationException("Validation failed for one or more fields");
            exception.Data["ValidationErrors"] = errors;
            throw exception;
        }
    }

    /// <summary>
    /// Validates a DTO and returns validation results without throwing
    /// </summary>
    public static (bool IsValid, List<ValidationError> Errors) TryValidateDto<T>(T dto) where T : class
    {
        if (dto == null)
        {
            return (false, new List<ValidationError>
            {
                new ValidationError
                {
                    Field = "Request",
                    Message = "Request body cannot be null",
                    Code = "NULL_REQUEST"
                }
            });
        }

        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(dto);
        var isValid = Validator.TryValidateObject(dto, validationContext, validationResults, true);

        var errors = validationResults.Select(vr => new ValidationError
        {
            Field = string.Join(", ", vr.MemberNames),
            Message = vr.ErrorMessage ?? "Validation failed",
            Code = "VALIDATION_FAILED"
        }).ToList();

        return (isValid, errors);
    }

    /// <summary>
    /// Creates a standardized error response for validation failures
    /// </summary>
    public static ErrorResponseDto CreateValidationErrorResponse(List<ValidationError> errors, string message = "Validation failed")
    {
        return new ErrorResponseDto
        {
            Success = false,
            Message = message,
            ErrorCode = "VALIDATION_ERROR",
            Timestamp = DateTime.UtcNow,
            Errors = errors.Select(e => $"{e.Field}: {e.Message}").ToList(),
            ValidationErrors = errors
        };
    }

    /// <summary>
    /// Creates a standardized error response for general errors
    /// </summary>
    public static ErrorResponseDto CreateErrorResponse(string message, string errorCode = "ERROR", Exception? exception = null)
    {
        var response = new ErrorResponseDto
        {
            Success = false,
            Message = message,
            ErrorCode = errorCode,
            Timestamp = DateTime.UtcNow
        };

        if (exception != null && IsDevEnvironment())
        {
            response.Details = exception.StackTrace;
        }

        return response;
    }

    /// <summary>
    /// Creates a standardized error response for resource not found (404)
    /// </summary>
    public static ErrorResponseDto CreateResourceNotFoundResponse(string resourceType, string resourceId, string? additionalDetails = null)
    {
        return new ErrorResponseDto(
            $"{resourceType} with ID '{resourceId}' was not found.",
            PowerBITips.Api.Models.Constants.ErrorCodes.ResourceNotFound
        )
        {
            Details = additionalDetails ?? $"The requested {resourceType.ToLower()} does not exist or you may not have access to it.",
            ErrorDetails = new Dictionary<string, object>
            {
                { "resourceType", resourceType },
                { "resourceId", resourceId },
                { "suggestion", "Verify the ID is correct and that the resource exists" }
            }
        };
    }

    /// <summary>
    /// Creates a standardized error response for route not found (404)
    /// </summary>
    public static ErrorResponseDto CreateRouteNotFoundResponse(string method, string path)
    {
        return new ErrorResponseDto(
            $"The requested endpoint '{path}' does not exist.",
            PowerBITips.Api.Models.Constants.ErrorCodes.RouteNotFound
        )
        {
            Details = $"No route found for {method} {path}. Please check the API documentation for available endpoints.",
            ErrorDetails = new Dictionary<string, object>
            {
                { "requestedPath", path },
                { "requestedMethod", method },
                { "suggestion", "Check that the URL is correct and the endpoint exists" }
            }
        };
    }

    /// <summary>
    /// Creates a standardized success response
    /// </summary>
    public static SuccessResponseDto CreateSuccessResponse(string message = "Operation completed successfully")
    {
        return new SuccessResponseDto
        {
            Success = true,
            Message = message,
            Timestamp = DateTime.UtcNow
        };
    }

    private static bool IsDevEnvironment()
    {
        return Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
    }
}

/// <summary>
/// Extension methods for Azure Functions HTTP request validation
/// </summary>
public static class HttpRequestDataExtensions
{
    /// <summary>
    /// Validates a DTO from the request body and returns a standardized response if invalid
    /// </summary>
    public static async Task<(bool IsValid, T? Dto, ErrorResponseDto? ErrorResponse)> ValidateRequestAsync<T>(
        this Stream requestBody) where T : class
    {
        try
        {
            var body = await new StreamReader(requestBody).ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(body))
            {
                var error = ValidationHelper.CreateValidationErrorResponse(
                    new List<ValidationError>
                    {
                        new ValidationError
                        {
                            Field = "Request",
                            Message = "Request body is required",
                            Code = "EMPTY_REQUEST"
                        }
                    },
                    "Request body is required"
                );
                return (false, null, error);
            }

            var dto = JsonSerializer.Deserialize<T>(body, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            });

            if (dto == null)
            {
                var error = ValidationHelper.CreateValidationErrorResponse(
                    new List<ValidationError>
                    {
                        new ValidationError
                        {
                            Field = "Request",
                            Message = "Failed to deserialize request body",
                            Code = "DESERIALIZATION_ERROR"
                        }
                    },
                    "Failed to deserialize request body"
                );
                return (false, null, error);
            }

            var (isValid, errors) = ValidationHelper.TryValidateDto(dto);

            if (!isValid)
            {
                var errorResponse = ValidationHelper.CreateValidationErrorResponse(errors);
                return (false, null, errorResponse);
            }

            return (true, dto, null);
        }
        catch (JsonException ex)
        {
            var error = ValidationHelper.CreateErrorResponse(
                "Invalid JSON format in request body",
                "JSON_PARSE_ERROR",
                ex
            );
            return (false, null, error);
        }
        catch (Exception ex)
        {
            var error = ValidationHelper.CreateErrorResponse(
                "Error processing request body",
                "REQUEST_PROCESSING_ERROR",
                ex
            );
            return (false, null, error);
        }
    }
}