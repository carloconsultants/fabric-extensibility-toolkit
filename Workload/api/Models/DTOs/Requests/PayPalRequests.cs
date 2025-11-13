using System.ComponentModel.DataAnnotations;
using PowerBITips.Api.Models.PayPal;

namespace PowerBITips.Api.Models.DTOs.Requests;

public class CreatePayPalSubscriptionRequest
{
    [Required]
    public string PlanId { get; set; } = string.Empty;
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    public SubscriberInfo Subscriber { get; set; } = new();
    
    public ApplicationContext? ApplicationContext { get; set; }
    
    public Dictionary<string, object>? CustomMetadata { get; set; }
    
    public string? ReturnUrl { get; set; }
    
    public string? CancelUrl { get; set; }
}

public class UpdatePayPalSubscriptionRequest
{
    [Required]
    public string SubscriptionId { get; set; } = string.Empty;
    
    public string? NewPlanId { get; set; }
    
    public List<PatchOperation>? Operations { get; set; }
    
    public string? Reason { get; set; }
    
    public Dictionary<string, object>? UpdatedMetadata { get; set; }
}

public class CancelPayPalSubscriptionRequest
{
    [Required]
    public string SubscriptionId { get; set; } = string.Empty;
    
    [Required]
    public string Reason { get; set; } = string.Empty;
    
    public string? UserId { get; set; }
    
    public bool? CancelImmediately { get; set; } = false;
    
    public Dictionary<string, object>? AdditionalInfo { get; set; }
}

public class PayPalWebhookRequest
{
    [Required]
    public string EventType { get; set; } = string.Empty;
    
    [Required]
    public PayPalWebhookResource Resource { get; set; } = new();
    
    public string? EventId { get; set; }
    
    public DateTime? CreateTime { get; set; }
    
    public string? ResourceType { get; set; }
    
    public Dictionary<string, object>? AdditionalData { get; set; }
}

public class PayPalWebhookResource
{
    public string? Id { get; set; }
    public string? Status { get; set; }
    public Dictionary<string, object>? Properties { get; set; }
}

public class PatchOperation
{
    [Required]
    public string Op { get; set; } = string.Empty; // "replace", "add", "remove"
    
    [Required]
    public string Path { get; set; } = string.Empty;
    
    public object? Value { get; set; }
}

public class SubscriberInfo
{
    [Required]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    public string EmailAddress { get; set; } = string.Empty;
    
    public Address? ShippingAddress { get; set; }
}

public class Address
{
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? CountryCode { get; set; }
}

public class ApplicationContext
{
    public string? BrandName { get; set; }
    public string? Locale { get; set; }
    public string? ShippingPreference { get; set; }
    public string? UserAction { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
}

public class PaymentMethod
{
    public string? PayerSelected { get; set; }
    public string? PayeePreferred { get; set; }
}