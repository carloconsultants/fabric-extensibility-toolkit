using Azure;
using Azure.Data.Tables;
using PowerBITips.Api.Models.Enums;

namespace PowerBITips.Api.Models.Entities;

public class User : ITableEntity
{
    public string PartitionKey { get; set; } = string.Empty;
    public string RowKey { get; set; } = string.Empty;
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    // User properties matching TypeScript model
    public string Environment { get; set; } = string.Empty;
    public string IdentityProvider { get; set; } = string.Empty;
    public string IDPUserName { get; set; } = string.Empty;
    public string IDPUserId { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Liked { get; set; } // Keeping as string for now, as noted in TS model
    public string? TrialSubscription { get; set; }
    public string? UserRole { get; set; }
    public string? AzureSubscriptionId { get; set; }

    // Serialized JSON properties for complex objects
    public string? SubscriptionJson { get; set; }
    public string? ThemesJson { get; set; }
    public string? PublishedJson { get; set; }
}

public class UserSubscription
{
    public string Id { get; set; } = string.Empty;
    public string PlanId { get; set; } = string.Empty;
    public PayPalSubscriptionStatus Status { get; set; }
    public string LastStatusCheckDate { get; set; } = string.Empty;
    public string EndDate { get; set; } = string.Empty;
}

public class UserTheme
{
    public string BlobUrl { get; set; } = string.Empty;
    public string? ProjectBlobUrl { get; set; }
    public string? ProjectImagesBlobUrl { get; set; }
    public string ThemeId { get; set; } = string.Empty;
    public string ThemeName { get; set; } = string.Empty;
    public List<string>? PublishOptions { get; set; }
    public string? ScrimsFamilyName { get; set; }
}

public class PublishedUserItem
{
    public string PublishedItemGuid { get; set; } = string.Empty;
    public string PublishedItemType { get; set; } = string.Empty;
}

public class PublishedUserTheme
{
    public string PublishedItemGuid { get; set; } = string.Empty;
    public ThemeType PublishedItemType { get; set; }
}

public class Box
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
}

public class VisualLayout
{
    public int PageWidth { get; set; }
    public int PageHeight { get; set; }
    public List<Box> Boxes { get; set; } = new();
    public string Id { get; set; } = string.Empty;
    public string? Description { get; set; }
}