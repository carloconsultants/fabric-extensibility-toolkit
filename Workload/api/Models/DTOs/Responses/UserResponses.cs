using PowerBITips.Api.Models.Entities;
using PowerBITips.Api.Models.Enums;
using PowerBITips.Api.Models.DTOs.Common;
using PowerBITips.Api.Utilities.Helpers;

namespace PowerBITips.Api.Models.DTOs.Responses;

/// <summary>
/// Response containing user information
/// </summary>
public class UserResponse : BaseResponseDto
{
    public string UserId { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public string IdentityProvider { get; set; } = string.Empty;
    public string IDPUserName { get; set; } = string.Empty;
    public string IDPUserId { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Liked { get; set; }
    public string? TrialSubscription { get; set; }
    public UserRole? UserRole { get; set; }
    public string? AzureSubscriptionId { get; set; }
    public UserSubscription? Subscription { get; set; }
    public List<UserTheme>? Themes { get; set; }
    public List<PublishedUserItem>? Published { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Response containing a paginated list of users
/// </summary>
public class UsersListResponse : PaginatedResponseDto<UserResponse>
{
    public Dictionary<UserRole, int>? RoleDistribution { get; set; }
    public Dictionary<string, int>? EnvironmentDistribution { get; set; }
    public int ActiveUsersCount { get; set; }
    public int InactiveUsersCount { get; set; }
}

/// <summary>
/// Response for user login event operations
/// </summary>
public class LoginEventResponse : SuccessResponseDto
{
    public string EventId { get; set; } = string.Empty;
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    public string EventType { get; set; } = string.Empty;
    public Dictionary<string, object>? ProcessedMetadata { get; set; }
}

/// <summary>
/// Response for user creation operations
/// </summary>
public class CreateUserResponse : SuccessResponseDto
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public UserRole DefaultRole { get; set; } = UserRole.User;
}

/// <summary>
/// Response for user role update operations
/// </summary>
public class UpdateUserRoleResponse : SuccessResponseDto
{
    public string UserId { get; set; } = string.Empty;
    public UserRole PreviousRole { get; set; }
    public UserRole NewRole { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string UpdatedBy { get; set; } = string.Empty;
}

/// <summary>
/// Response for trial subscription update operations
/// </summary>
public class UpdateTrialSubscriptionResponse : SuccessResponseDto
{
    public string UserId { get; set; } = string.Empty;
    public string PreviousSubscription { get; set; } = string.Empty;
    public string NewSubscription { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;
}

/// <summary>
/// Response containing user statistics and metrics
/// </summary>
public class UserStatsResponse : BaseResponseDto
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int NewUsersToday { get; set; }
    public int NewUsersThisWeek { get; set; }
    public int NewUsersThisMonth { get; set; }
    public Dictionary<UserRole, int> UsersByRole { get; set; } = new();
    public Dictionary<string, int> UsersByEnvironment { get; set; } = new();
    public Dictionary<string, int> LoginsByDay { get; set; } = new();
    public DateTime StatsGeneratedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Response for GetUserInfo endpoint - matches the old TypeScript API format
/// </summary>
public class GetUserInfoResponse
{
    public StaticWebAppsAuth.ClientPrincipal? ClientPrincipal { get; set; }
    public UserSubscription? Subscription { get; set; }
    public string? TrialSubscription { get; set; }
    public string? UserRole { get; set; }
    public int? NumberOfThemes { get; set; }
    public int? NumberOfPublishedItems { get; set; }
    public bool? HasActiveAzureSubscription { get; set; }
}