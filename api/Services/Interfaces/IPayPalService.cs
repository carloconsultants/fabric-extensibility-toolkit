using PowerBITips.Api.Models.DTOs;
using PowerBITips.Api.Models.PayPal;
using PowerBITips.Api.Services.Common;

namespace PowerBITips.Api.Services.Interfaces
{
    public interface IPayPalService
    {
        Task<ServiceResponse<List<SubscriptionPlan>>> GetActiveSubscriptionPlansAsync();
        Task<ServiceResponse<PayPalSubscription>> GetSubscriptionAsync(string subscriptionId);
        Task<ServiceResponse<bool>> ActivateSubscriptionAsync(string subscriptionId);
        Task<ServiceResponse<bool>> SuspendSubscriptionAsync(string subscriptionId, string reason);
        Task<ServiceResponse<bool>> CancelSubscriptionAsync(string subscriptionId);
    }
}