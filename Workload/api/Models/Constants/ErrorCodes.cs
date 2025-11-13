namespace PowerBITips.Api.Models.Constants;

/// <summary>
/// Standard error codes for API error responses.
/// 
/// IMPORTANT: Distinguishing Route vs Resource 404s
/// 
/// - ROUTE_NOT_FOUND: The URL/endpoint doesn't exist at all in our API.
///   Example: GET /api/nonexistent-endpoint
///   This means the route pattern doesn't match any function.
/// 
/// - RESOURCE_NOT_FOUND: The route exists, but the specific resource (entity) doesn't.
///   Example: GET /api/themes/12345 (route exists, but theme with ID 12345 doesn't)
///   This means the endpoint was reached, but the business logic couldn't find the resource.
/// 
/// This distinction helps debug whether:
/// 1. The API call is reaching our API layer (RESOURCE_NOT_FOUND = yes, ROUTE_NOT_FOUND = no)
/// 2. The issue is with the URL vs the data
/// </summary>
public static class ErrorCodes
{
    // Route/Endpoint Errors (4xx)
    /// <summary>
    /// The requested route/endpoint does not exist.
    /// Use this when the URL pattern doesn't match any function in our API.
    /// This indicates the request never reached our business logic.
    /// </summary>
    public const string RouteNotFound = "ROUTE_NOT_FOUND";
    
    /// <summary>
    /// The requested resource (entity) does not exist.
    /// Use this when the route exists and was executed, but the specific resource wasn't found.
    /// This indicates the request reached our API and business logic, but the data doesn't exist.
    /// </summary>
    public const string ResourceNotFound = "RESOURCE_NOT_FOUND";
    
    /// <summary>
    /// The request method (GET, POST, etc.) is not allowed for this route
    /// </summary>
    public const string MethodNotAllowed = "METHOD_NOT_ALLOWED";
    
    // Authentication/Authorization Errors (4xx)
    /// <summary>
    /// User is not authenticated
    /// </summary>
    public const string Unauthorized = "UNAUTHORIZED";
    
    /// <summary>
    /// User is authenticated but doesn't have permission
    /// </summary>
    public const string Forbidden = "FORBIDDEN";
    
    // Validation Errors (4xx)
    /// <summary>
    /// Request validation failed
    /// </summary>
    public const string ValidationError = "VALIDATION_ERROR";
    
    /// <summary>
    /// Request body is missing or invalid
    /// </summary>
    public const string BadRequest = "BAD_REQUEST";
    
    // Server Errors (5xx)
    /// <summary>
    /// Internal server error
    /// </summary>
    public const string InternalServerError = "INTERNAL_ERROR";
    
    /// <summary>
    /// Service is temporarily unavailable
    /// </summary>
    public const string ServiceUnavailable = "SERVICE_UNAVAILABLE";
}

