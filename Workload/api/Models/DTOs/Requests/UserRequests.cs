using System.ComponentModel.DataAnnotations;
using PowerBITips.Api.Models.Enums;
using PowerBITips.Api.Models.DTOs.Common;
using PowerBITips.Api.Models.DTOs.Validation;

namespace PowerBITips.Api.Models.DTOs.Requests;

/// <summary>
/// Request to create a new user in the system
/// </summary>
public class CreateUserRequest : BaseRequestDto
{
    [Required(ErrorMessage = "Environment is required")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "Environment must be between 1 and 50 characters")]
    public string Environment { get; set; } = string.Empty;

    [Required(ErrorMessage = "Identity provider is required")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Identity provider must be between 1 and 100 characters")]
    public string IdentityProvider { get; set; } = string.Empty;

    [Required(ErrorMessage = "IDP username is required")]
    [StringLength(255, MinimumLength = 1, ErrorMessage = "IDP username must be between 1 and 255 characters")]
    public string IDPUserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "IDP user ID is required")]
    [ValidGuid(ErrorMessage = "IDP user ID must be a valid GUID")]
    public string IDPUserId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tenant ID is required")]
    [ValidGuid(ErrorMessage = "Tenant ID must be a valid GUID")]
    public string TenantId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Username is required")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Username must be between 1 and 100 characters")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "First name is required")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "First name must be between 1 and 100 characters")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Last name must be between 1 and 100 characters")]
    public string LastName { get; set; } = string.Empty;
}

/// <summary>
/// Request to update a user's role
/// </summary>
public class UpdateUserRoleRequest : BaseRequestDto
{
    [Required(ErrorMessage = "User role is required")]
    [EnumDataType(typeof(UserRole), ErrorMessage = "Invalid user role")]
    public UserRole UserRole { get; set; }
}

/// <summary>
/// Request to update a user's trial subscription status
/// </summary>
public class UpdateTrialSubscriptionRequest : BaseRequestDto
{
    [Required(ErrorMessage = "Trial subscription status is required")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "Trial subscription must be between 1 and 50 characters")]
    public string TrialSubscription { get; set; } = string.Empty;
}

/// <summary>
/// Request to log a user login event
/// </summary>
public class LoginEventRequest : BaseRequestDto
{
    [Required(ErrorMessage = "User ID is required")]
    [ValidGuid(ErrorMessage = "User ID must be a valid GUID")]
    public string UserId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Event type is required")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Event type must be between 1 and 100 characters")]
    public string EventType { get; set; } = string.Empty;

    [Required(ErrorMessage = "Timestamp is required")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Request to get user information
/// </summary>
public class GetUserRequest : BaseRequestDto
{
    [Required(ErrorMessage = "User ID is required")]
    [ValidGuid(ErrorMessage = "User ID must be a valid GUID")]
    public string UserId { get; set; } = string.Empty;

    public bool IncludeSubscriptionInfo { get; set; } = false;
    public bool IncludeRoleInfo { get; set; } = true;
}

/// <summary>
/// Request to get multiple users with pagination
/// </summary>
public class GetUsersRequest : BaseRequestDto
{
    [Range(1, 1000, ErrorMessage = "Page size must be between 1 and 1000")]
    public int PageSize { get; set; } = 50;

    [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0")]
    public int PageNumber { get; set; } = 1;

    [StringLength(100, ErrorMessage = "Search query cannot exceed 100 characters")]
    public string? SearchQuery { get; set; }

    [EnumDataType(typeof(UserRole), ErrorMessage = "Invalid user role filter")]
    public UserRole? RoleFilter { get; set; }

    public bool IncludeSubscriptionInfo { get; set; } = false;
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
}