using System.ComponentModel.DataAnnotations;
using PowerBITips.Api.Models.Enums;

namespace PowerBITips.Api.Models.PayPal
{
    // Enums
    public enum PayPalBillingInterval
    {
        DAY,
        WEEK,
        MONTH,
        YEAR
    }

    public enum PayPalBillingTenureType
    {
        REGULAR,
        TRIAL
    }

    public enum PayPalPricingModel
    {
        VOLUME,
        TIERED
    }

    public enum PayPalSubscriptionPlanStatus
    {
        CREATED,
        INACTIVE,
        ACTIVE
    }

    // Using existing PayPalSubscriptionStatus from Models.Enums

    public enum PayPalSetupFeeFailureAction
    {
        CONTINUE,
        CANCEL
    }

    public enum PayPalReasonCode
    {
        PAYMENT_DENIED
    }

    public enum PayPalPhoneType
    {
        FAX,
        HOME,
        MOBILE,
        OTHER,
        PAGER
    }

    // Base Models
    public class PayPalMoney
    {
        public string? CurrencyCode { get; set; }
        public string? Value { get; set; }
    }

    public class PayPalTaxes
    {
        public string? Percentage { get; set; }
        public bool Inclusive { get; set; }
    }

    public class PayPalLinkDescription
    {
        public string? Href { get; set; }
        public string? Rel { get; set; }
        public string? Method { get; set; }
    }

    public class PayPalBillingCycle
    {
        public PayPalFrequency? Frequency { get; set; }
        public PayPalTenureType? TenureType { get; set; }
        public int Sequence { get; set; }
        public int TotalCycles { get; set; }
        public PayPalPricingScheme? PricingScheme { get; set; }
    }

    public class PayPalFrequency
    {
        public PayPalBillingInterval IntervalUnit { get; set; }
        public int IntervalCount { get; set; }
    }

    public class PayPalTenureType
    {
        public PayPalBillingTenureType Type { get; set; }
    }

    public class PayPalPricingScheme
    {
        public PayPalMoney? FixedPrice { get; set; }
    }

    public class PayPalPaymentPreferences
    {
        public bool AutoBillOutstanding { get; set; }
        public PayPalMoney? SetupFee { get; set; }
        public PayPalSetupFeeFailureAction SetupFeeFailureAction { get; set; }
        public int PaymentFailureThreshold { get; set; }
    }

    // Subscription Models
    public class PayPalSubscriptionPlan
    {
        public string? Id { get; set; }
        public string? ProductId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public PayPalSubscriptionPlanStatus Status { get; set; }
        public List<PayPalBillingCycle>? BillingCycles { get; set; }
        public PayPalPaymentPreferences? PaymentPreferences { get; set; }
        public PayPalTaxes? Taxes { get; set; }
        public bool QuantitySupported { get; set; }
        public string? CreateTime { get; set; }
        public string? UpdateTime { get; set; }
        public List<PayPalLinkDescription>? Links { get; set; }
    }

    public class PayPalSubscriber
    {
        public string? Name { get; set; }
        public string? EmailAddress { get; set; }
        public string? PayerId { get; set; }
    }

    public class PayPalSubscriptionBillingInfo
    {
        public PayPalMoney? OutstandingBalance { get; set; }
        public List<PayPalBillingCycle>? CycleExecutions { get; set; }
        public string? LastPayment { get; set; }
        public string? NextBillingTime { get; set; }
        public int FailedPaymentsCount { get; set; }
    }

    public class PayPalSubscription
    {
        public string? Id { get; set; }
        public string? PlanId { get; set; }
        public string? StartTime { get; set; }
        public string? Quantity { get; set; }
        public PayPalMoney? ShippingAmount { get; set; }
        public PayPalSubscriber? Subscriber { get; set; }
        public PayPalSubscriptionBillingInfo? BillingInfo { get; set; }
        public string? CreateTime { get; set; }
        public string? UpdateTime { get; set; }
        public string? CustomId { get; set; }
        public PayPalSubscriptionPlan? Plan { get; set; }
        public PayPalSubscriptionStatus Status { get; set; }
        public string? StatusUpdateTime { get; set; }
        public string? StatusChangeNote { get; set; }
        public bool PlanOverridden { get; set; }
        public List<PayPalLinkDescription>? Links { get; set; }
    }

    // API Response Models
    public class PayPalTokenResponse
    {
        public string? Scope { get; set; }
        public string? AccessToken { get; set; }
        public string? TokenType { get; set; }
        public string? AppId { get; set; }
        public int ExpiresIn { get; set; }
        public string? Nonce { get; set; }
    }

    public class PayPalListPlansResponse
    {
        public List<PayPalSubscriptionPlan>? Plans { get; set; }
    }

    // DTOs for API responses
    public class SubscriptionPlan
    {
        public string? PlanId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public PayPalBillingInterval IntervalType { get; set; }
        public string? Price { get; set; }
        public string? CurrencyCode { get; set; }
        public string? TaxesPercentage { get; set; }
        public bool TaxesInclusive { get; set; }
    }

    // Request Models
    public class CreatePayPalSubscriptionRequest
    {
        [Required]
        public string? SubscriptionId { get; set; }
    }

    public class UpdatePayPalSubscriptionRequest
    {
        [Required]
        public PayPalSubscriptionStatus Status { get; set; }
    }

    // Additional PayPal types used in DTOs
    public class PayPalLink
    {
        public string Href { get; set; } = string.Empty;
        public string Rel { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
    }



    public class PayPalName
    {
        public string GivenName { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
    }

    public class PayPalAddress
    {
        public string AddressLine1 { get; set; } = string.Empty;
        public string AddressLine2 { get; set; } = string.Empty;
        public string AdminArea1 { get; set; } = string.Empty;
        public string AdminArea2 { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string CountryCode { get; set; } = string.Empty;
    }

    public class PayPalBillingInfo
    {
        public PayPalMoney? OutstandingBalance { get; set; }
        public List<PayPalCycleExecution> CycleExecutions { get; set; } = new();
        public DateTime LastPayment { get; set; }
        public DateTime NextBillingTime { get; set; }
        public int FinalPaymentTime { get; set; }
        public int FailedPaymentsCount { get; set; }
    }

    public class PayPalCycleExecution
    {
        public PayPalBillingTenureType TenureType { get; set; }
        public int Sequence { get; set; }
        public int CyclesCompleted { get; set; }
        public int CyclesRemaining { get; set; }
        public int CurrentPricingSchemeVersion { get; set; }
        public int TotalCycles { get; set; }
    }

    public class PayPalPlan
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public PayPalSubscriptionPlanStatus Status { get; set; }
        public List<PayPalBillingCycle> BillingCycles { get; set; } = new();
        public PayPalPaymentPreferences? PaymentPreferences { get; set; }
        public PayPalTaxes? Taxes { get; set; }
        public bool QuantitySupported { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
        public List<PayPalLink> Links { get; set; } = new();
    }



    public class PayPalAmount
    {
        public string CurrencyCode { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    public enum PayPalTransactionStatus
    {
        COMPLETED,
        DECLINED,
        PARTIALLY_REFUNDED,
        PENDING,
        REFUNDED,
        CANCELLED,
        FAILED
    }
}