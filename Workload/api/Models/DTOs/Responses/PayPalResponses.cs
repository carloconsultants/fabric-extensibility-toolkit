using PowerBITips.Api.Models.PayPal;
using PowerBITips.Api.Models.Enums;

namespace PowerBITips.Api.Models.DTOs.Responses;

public class PayPalSubscriptionResponse
{
    public string Id { get; set; } = string.Empty;
    public string PlanId { get; set; } = string.Empty;
    public PayPalSubscriptionStatus Status { get; set; }
    public string StatusUpdateReason { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime CreateTime { get; set; }
    public List<PayPalLink> Links { get; set; } = new();
    public PayPalSubscriber? Subscriber { get; set; }
    public PayPalBillingInfo? BillingInfo { get; set; }
    public Dictionary<string, object>? CustomMetadata { get; set; }
    public string? ApprovalUrl { get; set; }
}

public class PayPalSubscriptionPlansResponse
{
    public List<PayPalPlan> Plans { get; set; } = new();
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
    public List<PayPalLink> Links { get; set; } = new();
}

public class PayPalSubscriptionUpdateResponse
{
    public string SubscriptionId { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public PayPalSubscriptionStatus NewStatus { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public List<string> UpdatedFields { get; set; } = new();
    public Dictionary<string, object>? UpdateDetails { get; set; }
}

public class PayPalSubscriptionCancellationResponse
{
    public string SubscriptionId { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime CancelledAt { get; set; } = DateTime.UtcNow;
    public string CancellationReason { get; set; } = string.Empty;
    public DateTime? EffectiveDate { get; set; }
    public PayPalRefundInfo? RefundInfo { get; set; }
}

public class PayPalWebhookResponse
{
    public string EventId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public bool Processed { get; set; }
    public string ProcessingStatus { get; set; } = string.Empty; // "Success", "Failed", "Retry"
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object>? ProcessingDetails { get; set; }
}

public class PayPalTransactionHistoryResponse
{
    public string SubscriptionId { get; set; } = string.Empty;
    public List<PayPalTransaction> Transactions { get; set; } = new();
    public int TotalTransactions { get; set; }
    public PayPalAmount TotalAmount { get; set; } = new();
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public Dictionary<PayPalTransactionStatus, int>? StatusCounts { get; set; }
}

public class PayPalTransaction
{
    public string Id { get; set; } = string.Empty;
    public PayPalTransactionStatus Status { get; set; }
    public PayPalAmount Amount { get; set; } = new();
    public DateTime TransactionTime { get; set; }
    public string? Description { get; set; }
    public Dictionary<string, object>? Details { get; set; }
}

public class PayPalRefundInfo
{
    public string? RefundId { get; set; }
    public PayPalAmount? RefundAmount { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? Status { get; set; }
}

public class PayPalApiErrorResponse
{
    public string Name { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? DebugId { get; set; }
    public List<PayPalErrorDetail> Details { get; set; } = new();
    public List<PayPalLink> Links { get; set; } = new();
}

public class PayPalErrorDetail
{
    public string Issue { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? Value { get; set; }
}