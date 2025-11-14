namespace PowerBITips.Api.Models.Enums;

public enum UserRole
{
    User,
    Admin
}



public enum PayPalSubscriptionStatus
{
    ApprovalPending,
    Approved,
    Active,
    Suspended,
    Cancelled,
    Expired
}

public enum PayPalBillingInterval
{
    Day,
    Week,
    Month,
    Year
}

public enum PayPalBillingTenureType
{
    Regular,
    Trial
}

public enum PayPalPricingModel
{
    Volume,
    Tiered
}

public enum PayPalSubscriptionPlanStatus
{
    Created,
    Inactive,
    Active
}