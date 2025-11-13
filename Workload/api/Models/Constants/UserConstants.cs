using PowerBITips.Api.Utilities.Helpers;

namespace PowerBITips.Api.Models.Constants;

/// <summary>
/// Constants for user entities. All configurable values are retrieved from environment variables.
/// </summary>
public static class UserConstants
{
    /// <summary>
    /// Gets the table name for users from environment variables.
    /// </summary>
    public static string TableName => ConfigurationHelper.GetRequiredEnvironmentVariable("STORAGE_USER_TABLE");

    public const string DefaultPartitionKey = "user";

    // User roles
    public const string AdminRole = "Admin";
    public const string UserRole = "User";
}

public static class ErrorConstants
{
    public const string UserNotFound = "User not found";
    public const string UnauthorizedAccess = "Unauthorized access";
    public const string InvalidUserData = "Invalid user data";
    public const string UserCreationFailed = "Failed to create user";
    public const string UserUpdateFailed = "Failed to update user";
}