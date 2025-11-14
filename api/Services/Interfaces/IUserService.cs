using PowerBITips.Api.Models.DTOs.Requests;
using PowerBITips.Api.Models.DTOs.Responses;
using PowerBITips.Api.Models.Entities;
using PowerBITips.Api.Services.Common;
using PowerBITips.Api.Utilities.Helpers;

namespace PowerBITips.Api.Services.Interfaces;

public interface IUserService
{
    Task<ServiceResponse<UserResponse>> GetUserAsync(StaticWebAppsAuth.ClientPrincipal clientPrincipal);
    Task<ServiceResponse<UserResponse>> GetUserByIdAsync(string userId);
    Task<ServiceResponse<UserResponse>> GetUserByIDPUserNameAsync(string idpUserName);
    Task<ServiceResponse<UsersListResponse>> GetUsersAsync(string searchParam = "", int maxResults = 20);
    Task<ServiceResponse<UsersListResponse>> GetContributorsAsync();
    Task<ServiceResponse<UserResponse>> CreateUserAsync(CreateUserRequest request);
    Task<ServiceResponse<UserResponse>> UpdateUserAsync(UserResponse user);
    Task<ServiceResponse<UserResponse>> UpsertUserAsync(UserResponse user);
    Task<ServiceResponse<UserResponse>> UpdateUserRoleAsync(string userId, UpdateUserRoleRequest request);
    Task<ServiceResponse<UserResponse>> UpdateTrialSubscriptionAsync(string userId, UpdateTrialSubscriptionRequest request);
    Task<ServiceResponse<LoginEventResponse>> PostLoginEventAsync(LoginEventRequest request);
    Task<ServiceResponse<UserResponse>> UpsertPublishedItemAsync(StaticWebAppsAuth.ClientPrincipal clientPrincipal, string guid, string type);
    Task<ServiceResponse<UserResponse>> UpsertUserThemeAsync(StaticWebAppsAuth.ClientPrincipal clientPrincipal, string themeId, string themeName);
    Task<ServiceResponse<UserResponse>> DeleteUserThemeAsync(StaticWebAppsAuth.ClientPrincipal clientPrincipal, string themeId);
    Task<ServiceResponse<UserResponse>> UpdateUserSubscriptionAsync(StaticWebAppsAuth.ClientPrincipal clientPrincipal, UserSubscription subscription);
    Task<ServiceResponse<UserResponse>> UpdateUserSubscriptionAsync(StaticWebAppsAuth.ClientPrincipal clientPrincipal, Models.PayPal.PayPalSubscription paypalSubscription);
}