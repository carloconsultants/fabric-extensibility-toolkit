using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PowerBITips.Api.Models.DTOs;
using PowerBITips.Api.Models.PayPal;
using PowerBITips.Api.Services.Interfaces;
using PowerBITips.Api.Services.Common;
using PowerBITips.Api.Utilities.Helpers;
using System.Net;
using System.Text;

namespace PowerBITips.Api.Services
{
    public class PayPalService : IPayPalService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PayPalService> _logger;
        private readonly string _paypalUrl;
        private readonly string _clientId;
        private readonly string _clientSecret;

        public PayPalService(HttpClient httpClient, IConfiguration configuration, ILogger<PayPalService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _paypalUrl = ConfigurationHelper.GetRequiredEnvironmentVariable("PAYPAL_URL");
            _clientId = ConfigurationHelper.GetRequiredEnvironmentVariable("PAYPAL_CLIENT_ID");
            _clientSecret = ConfigurationHelper.GetRequiredEnvironmentVariable("PAYPAL_CLIENT_SECRET");
        }

        public async Task<ServiceResponse<List<SubscriptionPlan>>> GetActiveSubscriptionPlansAsync()
        {
            try
            {
                var token = await GetPaypalTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    return ServiceResponse<List<SubscriptionPlan>>.Error(
                        HttpStatusCode.Unauthorized,
                        "Failed to get PayPal access token");
                }

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                _httpClient.DefaultRequestHeaders.Add("Prefer", "return=representation");

                var response = await _httpClient.GetAsync($"{_paypalUrl}/v1/billing/plans");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to get PayPal plans. Status: {StatusCode}", response.StatusCode);
                    return ServiceResponse<List<SubscriptionPlan>>.Error(
                        HttpStatusCode.InternalServerError,
                        "Failed to fetch PayPal plans");
                }

                var content = await response.Content.ReadAsStringAsync();
                var plansResponse = JsonConvert.DeserializeObject<PayPalListPlansResponse>(content);

                if (plansResponse?.Plans == null)
                {
                    return ServiceResponse<List<SubscriptionPlan>>.Error(
                        HttpStatusCode.InternalServerError,
                        "Invalid response from PayPal API");
                }

                var activePlans = plansResponse.Plans
                    .Where(plan => plan.Status == PayPalSubscriptionPlanStatus.ACTIVE)
                    .ToList();

                var subscriptionPlans = activePlans.Select(plan => new SubscriptionPlan
                {
                    PlanId = plan.Id,
                    Name = plan.Name,
                    Description = plan.Description,
                    IntervalType = plan.BillingCycles?.FirstOrDefault()?.Frequency?.IntervalUnit ?? PayPalBillingInterval.MONTH,
                    Price = plan.BillingCycles?.FirstOrDefault()?.PricingScheme?.FixedPrice?.Value,
                    CurrencyCode = plan.BillingCycles?.FirstOrDefault()?.PricingScheme?.FixedPrice?.CurrencyCode,
                    TaxesPercentage = plan.Taxes?.Percentage,
                    TaxesInclusive = plan.Taxes?.Inclusive ?? false
                }).ToList();

                return ServiceResponse<List<SubscriptionPlan>>.Success(subscriptionPlans);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching PayPal subscription plans");
                return ServiceResponse<List<SubscriptionPlan>>.Error(
                    HttpStatusCode.InternalServerError,
                    "An error occurred trying to fetch PayPal plans");
            }
        }

        public async Task<ServiceResponse<PayPalSubscription>> GetSubscriptionAsync(string subscriptionId)
        {
            try
            {
                var token = await GetPaypalTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    return ServiceResponse<PayPalSubscription>.Error(
                        HttpStatusCode.Unauthorized,
                        "Failed to get PayPal access token");
                }

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

                var response = await _httpClient.GetAsync($"{_paypalUrl}/v1/billing/subscriptions/{subscriptionId}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to get PayPal subscription {SubscriptionId}. Status: {StatusCode}",
                        subscriptionId, response.StatusCode);
                    return ServiceResponse<PayPalSubscription>.Error(
                        response.StatusCode,
                        $"An error occurred trying to fetch PayPal subscription: {subscriptionId}");
                }

                var content = await response.Content.ReadAsStringAsync();
                var subscription = JsonConvert.DeserializeObject<PayPalSubscription>(content);

                if (subscription == null)
                {
                    return ServiceResponse<PayPalSubscription>.Error(
                        HttpStatusCode.InternalServerError,
                        "Failed to deserialize PayPal subscription response");
                }

                return ServiceResponse<PayPalSubscription>.Success(subscription);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching PayPal subscription {SubscriptionId}", subscriptionId);
                return ServiceResponse<PayPalSubscription>.Error(
                    HttpStatusCode.InternalServerError,
                    $"An error occurred trying to fetch PayPal subscription: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<bool>> ActivateSubscriptionAsync(string subscriptionId)
        {
            try
            {
                var token = await GetPaypalTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    return ServiceResponse<bool>.Error(
                        HttpStatusCode.Unauthorized,
                        "Failed to get PayPal access token");
                }

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

                var requestBody = new { reason = "Customer reactivated previous subscription" };
                var json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(
                    $"{_paypalUrl}/v1/billing/subscriptions/{subscriptionId}/activate",
                    content);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to activate PayPal subscription {SubscriptionId}. Status: {StatusCode}",
                        subscriptionId, response.StatusCode);
                    return ServiceResponse<bool>.Error(
                        HttpStatusCode.InternalServerError,
                        $"Could not reactivate subscription: {subscriptionId}.");
                }

                return ServiceResponse<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while activating PayPal subscription {SubscriptionId}", subscriptionId);
                return ServiceResponse<bool>.Error(
                    HttpStatusCode.InternalServerError,
                    "An error occurred trying to activate PayPal subscription");
            }
        }

        public async Task<ServiceResponse<bool>> SuspendSubscriptionAsync(string subscriptionId, string reason)
        {
            try
            {
                var token = await GetPaypalTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    return ServiceResponse<bool>.Error(
                        HttpStatusCode.Unauthorized,
                        "Failed to get PayPal access token");
                }

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

                var requestBody = new { reason };
                var json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(
                    $"{_paypalUrl}/v1/billing/subscriptions/{subscriptionId}/suspend",
                    content);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to suspend PayPal subscription {SubscriptionId}. Status: {StatusCode}",
                        subscriptionId, response.StatusCode);
                    return ServiceResponse<bool>.Error(
                        HttpStatusCode.InternalServerError,
                        $"Could not suspend subscription {subscriptionId}.");
                }

                return ServiceResponse<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while suspending PayPal subscription {SubscriptionId}", subscriptionId);
                return ServiceResponse<bool>.Error(
                    HttpStatusCode.InternalServerError,
                    "An error occurred trying to suspend PayPal subscription");
            }
        }

        public async Task<ServiceResponse<bool>> CancelSubscriptionAsync(string subscriptionId)
        {
            try
            {
                var token = await GetPaypalTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    return ServiceResponse<bool>.Error(
                        HttpStatusCode.Unauthorized,
                        "Failed to get PayPal access token");
                }

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

                var requestBody = new { reason = "Customer wanted to cancel." };
                var json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(
                    $"{_paypalUrl}/v1/billing/subscriptions/{subscriptionId}/cancel",
                    content);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to cancel PayPal subscription {SubscriptionId}. Status: {StatusCode}",
                        subscriptionId, response.StatusCode);
                    return ServiceResponse<bool>.Error(
                        HttpStatusCode.InternalServerError,
                        $"Could not cancel subscription {subscriptionId}.");
                }

                return ServiceResponse<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while canceling PayPal subscription {SubscriptionId}", subscriptionId);
                return ServiceResponse<bool>.Error(
                    HttpStatusCode.InternalServerError,
                    $"An error occurred trying to cancel PayPal subscription: {ex.Message}");
            }
        }

        private async Task<string?> GetPaypalTokenAsync()
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Clear();

                var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_clientId}:{_clientSecret}"));
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {credentials}");

                var requestContent = new StringContent("grant_type=client_credentials",
                    Encoding.UTF8, "application/x-www-form-urlencoded");

                var response = await _httpClient.PostAsync($"{_paypalUrl}/v1/oauth2/token", requestContent);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to get PayPal token. Status: {StatusCode}", response.StatusCode);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonConvert.DeserializeObject<PayPalTokenResponse>(content);

                return tokenResponse?.AccessToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting PayPal token");
                return null;
            }
        }
    }
}