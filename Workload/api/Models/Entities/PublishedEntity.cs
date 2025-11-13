using Azure;
using Azure.Data.Tables;
using PowerBITips.Api.Models.Enums;
using PowerBITips.Api.Models.Constants;

namespace PowerBITips.Api.Models.Entities;

/// <summary>
/// Azure Table Storage entity for published items
/// PartitionKey = PublishedItemType (theme, layout, project, scrims)
/// RowKey = PublishedGuid (unique identifier for the published item)
/// </summary>
public class PublishedEntity : ITableEntity
{
    public string PartitionKey { get; set; } = string.Empty;
    public string RowKey { get; set; } = string.Empty;
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    // Core published item properties
    public string PublishedGuid { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Downloads { get; set; } = 0;
    public int FavoriteCount { get; set; } = 0;
    public double? CreatedDate { get; set; } // Unix timestamp stored as double in table
    public double? UpdatedDate { get; set; } // Unix timestamp stored as double in table

    // Owner information
    public string OwnerId { get; set; } = string.Empty;
    public string OwnerIdentityProvider { get; set; } = string.Empty;

    // Optional properties
    public string ItemLink { get; set; } = string.Empty;
    public string PreviewImage { get; set; } = string.Empty;
    public bool Restricted { get; set; } = false;

    // Layout data (for layout type items, JSON stringified)
    public string Layout { get; set; } = string.Empty;

    // Theme-related properties (for theme/project items)
    public string UserThemeId { get; set; } = string.Empty;

    // Calculated properties
    public string ItemId => RowKey;
    public string ItemType => PartitionKey;
}