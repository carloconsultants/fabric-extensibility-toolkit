using PowerBITips.Api.Models.DTOs.Requests;
using PowerBITips.Api.Models.DTOs.Responses;
using PowerBITips.Api.Models.Entities;
using PowerBITips.Api.Models.Enums;

namespace PowerBITips.Api.Models.DTOs.Extensions;

/// <summary>
/// Extension methods for mapping between User entities and DTOs
/// </summary>
public static class UserMappingExtensions
{
    /// <summary>
    /// Maps a User entity to UserResponse DTO
    /// </summary>
    public static UserResponse ToDto(this User user)
    {
        return new UserResponse
        {
            UserId = user.IDPUserId,
            Environment = user.Environment,
            IdentityProvider = user.IdentityProvider,
            IDPUserName = user.IDPUserName,
            IDPUserId = user.IDPUserId,
            TenantId = user.TenantId,
            UserName = user.UserName,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Liked = user.Liked,
            TrialSubscription = user.TrialSubscription,
            UserRole = Enum.TryParse<UserRole>(user.UserRole, out var role) ? role : (UserRole?)null,
            AzureSubscriptionId = user.AzureSubscriptionId
        };
    }

    /// <summary>
    /// Maps a CreateUserRequest to User entity
    /// </summary>
    public static User ToEntity(this CreateUserRequest request)
    {
        return new User
        {
            PartitionKey = request.Environment,
            RowKey = request.IDPUserId,
            Environment = request.Environment,
            IdentityProvider = request.IdentityProvider,
            IDPUserName = request.IDPUserName,
            IDPUserId = request.IDPUserId,
            TenantId = request.TenantId,
            UserName = request.UserName,
            FirstName = request.FirstName,
            LastName = request.LastName,
            UserRole = PowerBITips.Api.Models.Enums.UserRole.User.ToString() // Default role as string
        };
    }

    /// <summary>
    /// Maps a collection of User entities to UsersListResponse DTO
    /// </summary>
    public static UsersListResponse ToDto(this IEnumerable<User> users, int totalCount, int pageNumber, int pageSize)
    {
        var usersList = users.ToList();
        
        return new UsersListResponse
        {
            Items = usersList.Select(u => u.ToDto()).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            RoleDistribution = usersList.GroupBy(u => u.UserRole)
                .Where(g => g.Key != null)
                .ToDictionary(g => Enum.TryParse<UserRole>(g.Key, out var role) ? role : UserRole.User, g => g.Count()),
            EnvironmentDistribution = usersList.GroupBy(u => u.Environment)
                .ToDictionary(g => g.Key ?? "Unknown", g => g.Count()),
            ActiveUsersCount = usersList.Count(u => !string.IsNullOrEmpty(u.UserRole)),
            InactiveUsersCount = usersList.Count(u => string.IsNullOrEmpty(u.UserRole)),
            Success = true,
            Message = "Users retrieved successfully",
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a CreateUserResponse from User entity
    /// </summary>
    public static CreateUserResponse ToCreateResponse(this User user)
    {
        return new CreateUserResponse
        {
            UserId = user.IDPUserId ?? string.Empty,
            UserName = user.UserName ?? string.Empty,
            Environment = user.Environment ?? string.Empty,
            CreatedAt = user.Timestamp?.DateTime ?? DateTime.UtcNow,
            DefaultRole = Enum.TryParse<UserRole>(user.UserRole, out var role) ? role : UserRole.User,
            Success = true,
            Message = "User created successfully",
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates an UpdateUserRoleResponse from role change information
    /// </summary>
    public static UpdateUserRoleResponse ToRoleUpdateResponse(this User user, UserRole previousRole, string updatedBy)
    {
        return new UpdateUserRoleResponse
        {
            UserId = user.IDPUserId ?? string.Empty,
            PreviousRole = previousRole,
            NewRole = Enum.TryParse<UserRole>(user.UserRole, out var role) ? role : UserRole.User,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = updatedBy,
            Success = true,
            Message = "User role updated successfully",
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates an UpdateTrialSubscriptionResponse from subscription change information
    /// </summary>
    public static UpdateTrialSubscriptionResponse ToTrialUpdateResponse(this User user, string previousSubscription)
    {
        return new UpdateTrialSubscriptionResponse
        {
            UserId = user.IDPUserId ?? string.Empty,
            PreviousSubscription = previousSubscription,
            NewSubscription = user.TrialSubscription ?? string.Empty,
            UpdatedAt = DateTime.UtcNow,
            ExpiresAt = CalculateTrialExpiration(user.TrialSubscription),
            Success = true,
            Message = "Trial subscription updated successfully",
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Helper method to calculate trial expiration date
    /// </summary>
    private static DateTime? CalculateTrialExpiration(string? trialSubscription)
    {
        return trialSubscription switch
        {
            "30-day" => DateTime.UtcNow.AddDays(30),
            "14-day" => DateTime.UtcNow.AddDays(14),
            "7-day" => DateTime.UtcNow.AddDays(7),
            _ => null
        };
    }
}