using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PowerBITips.Api.Core.Interfaces;
using PowerBITips.Api.Models.Constants;
using PowerBITips.Api.Models.DTOs.Requests;
using PowerBITips.Api.Models.DTOs.Responses;
using PowerBITips.Api.Models.Entities;
using PowerBITips.Api.Models.Enums;
using PowerBITips.Api.Services.Common;
using PowerBITips.Api.Services.Interfaces;
using PowerBITips.Api.Utilities.Helpers;
using System.Net;

namespace PowerBITips.Api.Services;

public class UserService : IUserService
{
    private readonly IAzureTableStorage _tableStorage;
    private readonly ILogger<UserService> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _appEnvironment;

    public UserService(
        IAzureTableStorage tableStorage,
        ILogger<UserService> logger,
        IConfiguration configuration)
    {
        _tableStorage = tableStorage;
        _logger = logger;
        _configuration = configuration;
        _appEnvironment = _configuration["APP_ENVIRONMENT"] ?? "local";

        _logger.LogInformation("✅ UserService initialized - table name will be retrieved from UserConstants.TableName");
    }

    public async Task<ServiceResponse<UserResponse>> GetUserAsync(StaticWebAppsAuth.ClientPrincipal clientPrincipal)
    {
        try
        {
            _logger.LogInformation("🔍 [UserService.GetUserAsync] Starting user lookup - UserDetails: {UserDetails}, UserId: {UserId}, IdentityProvider: {IdentityProvider}",
                clientPrincipal?.UserDetails ?? "null",
                clientPrincipal?.UserId ?? "null",
                clientPrincipal?.IdentityProvider ?? "null");

            if (clientPrincipal?.UserDetails == null)
            {
                _logger.LogWarning("⚠️ [UserService.GetUserAsync] Invalid client principal - UserDetails is null");
                return ServiceResponse<UserResponse>.BadRequest("Invalid client principal");
            }

            _logger.LogInformation("📋 [UserService.GetUserAsync] Querying table storage - Table: {Table}, Partition: {Partition}, RowKey: {RowKey}",
                UserConstants.TableName, _appEnvironment, clientPrincipal.UserDetails);

            var user = await _tableStorage.GetEntityAsync<User>(
                UserConstants.TableName,
                _appEnvironment,
                clientPrincipal.UserDetails);

            if (user == null)
            {
                _logger.LogWarning("⚠️ [UserService.GetUserAsync] User not found in table storage - RowKey: {RowKey}",
                    clientPrincipal.UserDetails);
                return ServiceResponse<UserResponse>.NotFound(ErrorConstants.UserNotFound);
            }

            _logger.LogInformation("✅ [UserService.GetUserAsync] User found - IDPUserId: {IDPUserId}, UserName: {UserName}, Role: {Role}, HasSubscription: {HasSubscription}",
                user.IDPUserId,
                user.UserName,
                user.UserRole,
                user.SubscriptionJson != null);

            var userResponse = MapUserToResponse(user);
            return ServiceResponse<UserResponse>.Success(userResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 [UserService.GetUserAsync] Exception retrieving user for principal {UserDetails}", clientPrincipal?.UserDetails);
            return ServiceResponse<UserResponse>.InternalServerError($"Error retrieving user: {ex.Message}");
        }
    }

    public async Task<ServiceResponse<UserResponse>> GetUserByIdAsync(string userId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return ServiceResponse<UserResponse>.BadRequest("User ID is required");
            }

            var user = await _tableStorage.GetEntityAsync<User>(
                UserConstants.TableName,
                _appEnvironment,
                userId);

            if (user == null)
            {
                return ServiceResponse<UserResponse>.NotFound(ErrorConstants.UserNotFound);
            }

            var userResponse = MapUserToResponse(user);
            return ServiceResponse<UserResponse>.Success(userResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user {UserId}", userId);
            return ServiceResponse<UserResponse>.InternalServerError($"Error retrieving user: {ex.Message}");
        }
    }

    public async Task<ServiceResponse<UserResponse>> GetUserByIDPUserNameAsync(string idpUserName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(idpUserName))
            {
                return ServiceResponse<UserResponse>.BadRequest("IDP User Name is required");
            }

            var user = await _tableStorage.GetEntityAsync<User>(
                UserConstants.TableName,
                _appEnvironment,
                idpUserName);

            if (user == null)
            {
                return ServiceResponse<UserResponse>.NotFound(ErrorConstants.UserNotFound);
            }

            var userResponse = MapUserToResponse(user);
            return ServiceResponse<UserResponse>.Success(userResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user by IDP name {IDPUserName}", idpUserName);
            return ServiceResponse<UserResponse>.InternalServerError($"Error retrieving user: {ex.Message}");
        }
    }

    public async Task<ServiceResponse<UsersListResponse>> GetUsersAsync(string searchParam = "", int maxResults = 20)
    {
        try
        {
            var users = await _tableStorage.GetEntitiesByPartitionAsync<User>(UserConstants.TableName, _appEnvironment);

            var usersList = users.Select(MapUserToResponse).ToList();

            // Apply search filter if provided
            if (!string.IsNullOrEmpty(searchParam))
            {
                usersList = usersList.Where(u =>
                    u.UserName.Contains(searchParam, StringComparison.OrdinalIgnoreCase) ||
                    u.FirstName.Contains(searchParam, StringComparison.OrdinalIgnoreCase) ||
                    u.LastName.Contains(searchParam, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // Apply limit
            if (maxResults > 0)
            {
                usersList = usersList.Take(maxResults).ToList();
            }

            var response = new UsersListResponse
            {
                Items = usersList,
                TotalCount = usersList.Count,
                PageNumber = 1,
                PageSize = usersList.Count
            };

            return ServiceResponse<UsersListResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users list");
            return ServiceResponse<UsersListResponse>.InternalServerError($"Error retrieving users: {ex.Message}");
        }
    }

    public async Task<ServiceResponse<UsersListResponse>> GetContributorsAsync()
    {
        try
        {
            var users = await _tableStorage.GetEntitiesByPartitionAsync<User>(UserConstants.TableName, _appEnvironment);

            var contributors = users
                .Where(u => u.UserRole == UserConstants.AdminRole || u.UserRole == "Contributor")
                .Select(MapUserToResponse)
                .ToList();

            var response = new UsersListResponse
            {
                Items = contributors,
                TotalCount = contributors.Count,
                PageNumber = 1,
                PageSize = contributors.Count
            };

            return ServiceResponse<UsersListResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving contributors");
            return ServiceResponse<UsersListResponse>.InternalServerError($"Error retrieving contributors: {ex.Message}");
        }
    }

    public async Task<ServiceResponse<UserResponse>> CreateUserAsync(CreateUserRequest request)
    {
        try
        {
            var user = new User
            {
                PartitionKey = _appEnvironment,
                RowKey = request.IDPUserName,
                Environment = request.Environment,
                IdentityProvider = request.IdentityProvider,
                IDPUserName = request.IDPUserName,
                IDPUserId = request.IDPUserId,
                TenantId = request.TenantId,
                UserName = request.UserName,
                FirstName = request.FirstName,
                LastName = request.LastName,
                UserRole = UserConstants.UserRole
            };

            await _tableStorage.CreateEntityAsync(UserConstants.TableName, user);

            var userResponse = MapUserToResponse(user);
            return ServiceResponse<UserResponse>.Success(userResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user {UserName}", request.UserName);
            return ServiceResponse<UserResponse>.InternalServerError($"Error creating user: {ex.Message}");
        }
    }

    public async Task<ServiceResponse<UserResponse>> UpdateUserAsync(UserResponse userResponse)
    {
        try
        {
            var user = MapResponseToUser(userResponse);
            await _tableStorage.UpdateEntityAsync(UserConstants.TableName, user);

            return ServiceResponse<UserResponse>.Success(userResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", userResponse.UserId);
            return ServiceResponse<UserResponse>.InternalServerError($"Error updating user: {ex.Message}");
        }
    }

    public async Task<ServiceResponse<UserResponse>> UpsertUserAsync(UserResponse userResponse)
    {
        try
        {
            // Try to get existing user first
            var existingUser = await _tableStorage.GetEntityAsync<User>(
                UserConstants.TableName,
                _appEnvironment,
                userResponse.UserId);

            User user;
            if (existingUser != null)
            {
                // Update existing
                user = MapResponseToUser(userResponse);
                user.ETag = existingUser.ETag;
                await _tableStorage.UpdateEntityAsync(UserConstants.TableName, user);
            }
            else
            {
                // Create new
                user = MapResponseToUser(userResponse);
                await _tableStorage.CreateEntityAsync(UserConstants.TableName, user);
            }

            var result = MapUserToResponse(user);
            return ServiceResponse<UserResponse>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error upserting user {UserId}", userResponse.UserId);
            return ServiceResponse<UserResponse>.InternalServerError($"Error upserting user: {ex.Message}");
        }
    }

    public async Task<ServiceResponse<UserResponse>> UpdateUserRoleAsync(string userId, UpdateUserRoleRequest request)
    {
        try
        {
            var user = await _tableStorage.GetEntityAsync<User>(
                UserConstants.TableName,
                _appEnvironment,
                userId);

            if (user == null)
            {
                return ServiceResponse<UserResponse>.NotFound(ErrorConstants.UserNotFound);
            }

            user.UserRole = request.UserRole.ToString();
            await _tableStorage.UpdateEntityAsync(UserConstants.TableName, user);

            var userResponse = MapUserToResponse(user);
            return ServiceResponse<UserResponse>.Success(userResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user role for {UserId}", userId);
            return ServiceResponse<UserResponse>.InternalServerError($"Error updating user role: {ex.Message}");
        }
    }

    public async Task<ServiceResponse<UserResponse>> UpdateTrialSubscriptionAsync(string userId, UpdateTrialSubscriptionRequest request)
    {
        try
        {
            var user = await _tableStorage.GetEntityAsync<User>(
                UserConstants.TableName,
                _appEnvironment,
                userId);

            if (user == null)
            {
                return ServiceResponse<UserResponse>.NotFound(ErrorConstants.UserNotFound);
            }

            user.TrialSubscription = request.TrialSubscription;
            await _tableStorage.UpdateEntityAsync(UserConstants.TableName, user);

            var userResponse = MapUserToResponse(user);
            return ServiceResponse<UserResponse>.Success(userResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating trial subscription for {UserId}", userId);
            return ServiceResponse<UserResponse>.InternalServerError($"Error updating trial subscription: {ex.Message}");
        }
    }

    public Task<ServiceResponse<LoginEventResponse>> PostLoginEventAsync(LoginEventRequest request)
    {
        try
        {
            // For now, just log the event - in the future this could be sent to analytics
            _logger.LogInformation("Login event for user {UserId}: {EventType} at {Timestamp} with metadata {Metadata}",
                request.UserId, request.EventType, request.Timestamp,
                request.Metadata != null ? string.Join(", ", request.Metadata.Select(kv => $"{kv.Key}:{kv.Value}")) : "none");

            var response = new LoginEventResponse
            {
                Success = true,
                Message = "Login event recorded successfully"
            };

            return Task.FromResult(ServiceResponse<LoginEventResponse>.Success(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error posting login event for user {UserId}", request.UserId);
            return Task.FromResult(ServiceResponse<LoginEventResponse>.InternalServerError($"Error posting login event: {ex.Message}"));
        }
    }

    public async Task<ServiceResponse<UserResponse>> UpsertPublishedItemAsync(StaticWebAppsAuth.ClientPrincipal clientPrincipal, string guid, string type)
    {
        try
        {
            var userResult = await GetUserAsync(clientPrincipal);
            if (!userResult.IsSuccess || userResult.Data == null)
            {
                return ServiceResponse<UserResponse>.Error(userResult.Status, userResult.ErrorMessage ?? "User not found");
            }

            var userResponse = userResult.Data;
            userResponse.Published ??= new List<PublishedUserItem>();

            // Check if item already exists
            var existingItem = userResponse.Published.FirstOrDefault(p => p.PublishedItemGuid == guid);
            if (existingItem == null)
            {
                userResponse.Published.Add(new PublishedUserItem
                {
                    PublishedItemGuid = guid,
                    PublishedItemType = type
                });

                await UpdateUserAsync(userResponse);
            }

            return ServiceResponse<UserResponse>.Success(userResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error upserting published item {Guid} for user", guid);
            return ServiceResponse<UserResponse>.InternalServerError($"Error upserting published item: {ex.Message}");
        }
    }

    public async Task<ServiceResponse<UserResponse>> UpsertUserThemeAsync(StaticWebAppsAuth.ClientPrincipal clientPrincipal, string themeId, string themeName)
    {
        try
        {
            var userResult = await GetUserAsync(clientPrincipal);
            if (!userResult.IsSuccess || userResult.Data == null)
            {
                return ServiceResponse<UserResponse>.Error(userResult.Status, userResult.ErrorMessage ?? "User not found");
            }

            var userResponse = userResult.Data;
            userResponse.Themes ??= new List<UserTheme>();

            var newTheme = new UserTheme
            {
                ThemeId = themeId,
                ThemeName = themeName,
                BlobUrl = BuildThemeUrl(themeId),
                ProjectBlobUrl = BuildProjectUrl(themeId),
                ProjectImagesBlobUrl = BuildProjectImagesUrl(themeId)
            };

            // Upsert the theme
            var existingIndex = userResponse.Themes.FindIndex(t => t.ThemeId == themeId);
            if (existingIndex >= 0)
            {
                userResponse.Themes[existingIndex] = newTheme;
            }
            else
            {
                userResponse.Themes.Add(newTheme);
            }

            await UpdateUserAsync(userResponse);
            return ServiceResponse<UserResponse>.Success(userResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error upserting theme {ThemeId} for user", themeId);
            return ServiceResponse<UserResponse>.InternalServerError($"Error upserting theme: {ex.Message}");
        }
    }

    public async Task<ServiceResponse<UserResponse>> DeleteUserThemeAsync(StaticWebAppsAuth.ClientPrincipal clientPrincipal, string themeId)
    {
        try
        {
            var userResult = await GetUserAsync(clientPrincipal);
            if (!userResult.IsSuccess || userResult.Data == null)
            {
                return ServiceResponse<UserResponse>.Error(userResult.Status, userResult.ErrorMessage ?? "User not found");
            }

            var userResponse = userResult.Data;
            if (userResponse.Themes != null)
            {
                userResponse.Themes = userResponse.Themes.Where(t => t.ThemeId != themeId).ToList();
                await UpdateUserAsync(userResponse);
            }

            return ServiceResponse<UserResponse>.Success(userResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting theme {ThemeId} for user", themeId);
            return ServiceResponse<UserResponse>.InternalServerError($"Error deleting theme: {ex.Message}");
        }
    }

    public async Task<ServiceResponse<UserResponse>> UpdateUserSubscriptionAsync(StaticWebAppsAuth.ClientPrincipal clientPrincipal, UserSubscription subscription)
    {
        try
        {
            var userResult = await GetUserAsync(clientPrincipal);
            if (!userResult.IsSuccess || userResult.Data == null)
            {
                return ServiceResponse<UserResponse>.Error(userResult.Status, userResult.ErrorMessage ?? "User not found");
            }

            var userResponse = userResult.Data;

            // Check if subscription is expired and cancelled
            var expired = subscription.Status == PayPalSubscriptionStatus.Cancelled &&
                         DateTime.TryParse(userResponse.Subscription?.EndDate, out var endDate) &&
                         endDate < DateTime.UtcNow;

            if (expired)
            {
                userResponse.Subscription = null;
            }
            else
            {
                userResponse.Subscription = new UserSubscription
                {
                    Id = subscription.Id,
                    PlanId = subscription.PlanId,
                    Status = subscription.Status,
                    LastStatusCheckDate = DateTime.UtcNow.ToString("O"),
                    EndDate = subscription.EndDate
                };
            }

            await UpdateUserAsync(userResponse);
            return ServiceResponse<UserResponse>.Success(userResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating subscription for user");
            return ServiceResponse<UserResponse>.InternalServerError($"Error updating subscription: {ex.Message}");
        }
    }

    // Helper methods for mapping between entities and DTOs
    private static UserResponse MapUserToResponse(User user)
    {
        var response = new UserResponse
        {
            UserId = user.RowKey,
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
            AzureSubscriptionId = user.AzureSubscriptionId
        };

        // Parse user role
        if (Enum.TryParse<UserRole>(user.UserRole, out var userRole))
        {
            response.UserRole = userRole;
        }

        // Deserialize complex objects from JSON
        if (!string.IsNullOrEmpty(user.SubscriptionJson))
        {
            response.Subscription = JsonConvert.DeserializeObject<UserSubscription>(user.SubscriptionJson);
        }

        if (!string.IsNullOrEmpty(user.ThemesJson))
        {
            response.Themes = JsonConvert.DeserializeObject<List<UserTheme>>(user.ThemesJson);
        }

        if (!string.IsNullOrEmpty(user.PublishedJson))
        {
            response.Published = JsonConvert.DeserializeObject<List<PublishedUserItem>>(user.PublishedJson);
        }

        return response;
    }

    private static User MapResponseToUser(UserResponse response)
    {
        var user = new User
        {
            PartitionKey = response.Environment,
            RowKey = response.UserId,
            Environment = response.Environment,
            IdentityProvider = response.IdentityProvider,
            IDPUserName = response.IDPUserName,
            IDPUserId = response.IDPUserId,
            TenantId = response.TenantId,
            UserName = response.UserName,
            FirstName = response.FirstName,
            LastName = response.LastName,
            Liked = response.Liked,
            TrialSubscription = response.TrialSubscription,
            UserRole = response.UserRole?.ToString(),
            AzureSubscriptionId = response.AzureSubscriptionId
        };

        // Serialize complex objects to JSON
        if (response.Subscription != null)
        {
            user.SubscriptionJson = JsonConvert.SerializeObject(response.Subscription);
        }

        if (response.Themes != null)
        {
            user.ThemesJson = JsonConvert.SerializeObject(response.Themes);
        }

        if (response.Published != null)
        {
            user.PublishedJson = JsonConvert.SerializeObject(response.Published);
        }

        return user;
    }

    public async Task<ServiceResponse<UserResponse>> UpdateUserSubscriptionAsync(StaticWebAppsAuth.ClientPrincipal clientPrincipal, Models.PayPal.PayPalSubscription paypalSubscription)
    {
        try
        {
            _logger.LogInformation("Updating PayPal subscription for user {UserId}", clientPrincipal.UserId);

            var userResult = await GetUserAsync(clientPrincipal);
            if (!userResult.IsSuccess || userResult.Data == null)
            {
                return ServiceResponse<UserResponse>.NotFound("User not found");
            }

            var userResponse = userResult.Data;

            // Map PayPal subscription to UserSubscription
            if (paypalSubscription == null)
            {
                userResponse.Subscription = null;
            }
            else
            {
                userResponse.Subscription = new UserSubscription
                {
                    Id = paypalSubscription.Id ?? string.Empty,
                    PlanId = paypalSubscription.PlanId ?? string.Empty,
                    Status = paypalSubscription.Status,
                    LastStatusCheckDate = DateTime.UtcNow.ToString("O"),
                    EndDate = string.Empty // PayPal subscriptions don't have explicit end dates
                };
            }

            await UpdateUserAsync(userResponse);
            return ServiceResponse<UserResponse>.Success(userResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating PayPal subscription for user");
            return ServiceResponse<UserResponse>.InternalServerError($"Error updating PayPal subscription: {ex.Message}");
        }
    }

    // Helper methods for building blob URLs (placeholder implementations)
    private static string BuildThemeUrl(string themeId) => $"https://themes.blob.core.windows.net/themes/{themeId}.json";
    private static string BuildProjectUrl(string themeId) => $"https://themes.blob.core.windows.net/projects/{themeId}.json";
    private static string BuildProjectImagesUrl(string themeId) => $"https://themes.blob.core.windows.net/images/{themeId}.zip";
}