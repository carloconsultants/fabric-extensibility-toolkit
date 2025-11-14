using System.ComponentModel.DataAnnotations;
using PowerBITips.Api.Models.Enums;
using PowerBITips.Api.Models.DTOs.Common;
using PowerBITips.Api.Models.DTOs.Validation;

namespace PowerBITips.Api.Models.DTOs.Requests;

/// <summary>
/// Request model for publishing a new item
/// </summary>
public class PublishItemRequest : BaseRequestDto
{
    [Required(ErrorMessage = "Item name is required")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Item name must be between 1 and 200 characters")]
    public string ItemName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Item ID is required")]
    [ValidGuid(ErrorMessage = "Item ID must be a valid GUID")]
    public string ItemId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Publish type is required")]
    [EnumDataType(typeof(PublishedItemType), ErrorMessage = "Invalid publish type")]
    public PublishedItemType PublishType { get; set; } = PublishedItemType.Theme;

    [ValidGuid(ErrorMessage = "User theme ID must be a valid GUID")]
    public string UserThemeId { get; set; } = string.Empty;

    [ValidUrl(ErrorMessage = "Preview image must be a valid URL")]
    public string PreviewImage { get; set; } = string.Empty;

    [ValidJson(ErrorMessage = "Layout must be valid JSON")]
    public object? Layout { get; set; } // For layout publishing

    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }

    [StringLength(100, ErrorMessage = "Category cannot exceed 100 characters")]
    public string? Category { get; set; }

    [CollectionSize(0, 10, ErrorMessage = "Tags cannot exceed 10 items")]
    public string[]? Tags { get; set; }

    public bool IsPublic { get; set; } = true;

    [StringLength(50, ErrorMessage = "Version cannot exceed 50 characters")]
    public string? Version { get; set; }

    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Request model for deleting a published item
/// </summary>
public class DeletePublishedItemRequest : BaseRequestDto
{
    [Required(ErrorMessage = "Item ID is required")]
    [ValidGuid(ErrorMessage = "Item ID must be a valid GUID")]
    public string ItemId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Item type is required")]
    [EnumDataType(typeof(PublishedItemType), ErrorMessage = "Invalid item type")]
    public PublishedItemType ItemType { get; set; } = PublishedItemType.Theme;

    [StringLength(500, ErrorMessage = "Deletion reason cannot exceed 500 characters")]
    public string? DeletionReason { get; set; }

    public bool ForceDelete { get; set; } = false;
}

/// <summary>
/// Query parameters for getting published items with pagination
/// </summary>
public class GetPublishedItemsRequest : BaseRequestDto
{
    [CollectionSize(0, 5, ErrorMessage = "Item types cannot exceed 5 items")]
    public string[] ItemTypes { get; set; } = Array.Empty<string>();

    [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0")]
    public int Page { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
    public int PageSize { get; set; } = 20;

    [StringLength(200, ErrorMessage = "Search query cannot exceed 200 characters")]
    public string Search { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "Sort field cannot exceed 50 characters")]
    public string SortBy { get; set; } = "CreatedDate";

    [RegularExpression("^(asc|desc)$", ErrorMessage = "Sort order must be 'asc' or 'desc'")]
    public string SortOrder { get; set; } = "desc";

    [StringLength(100, ErrorMessage = "Category filter cannot exceed 100 characters")]
    public string? CategoryFilter { get; set; }

    [StringLength(100, ErrorMessage = "Tag filter cannot exceed 100 characters")]
    public string? TagFilter { get; set; }

    public bool? IsPublicFilter { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }

    [ValidGuid(ErrorMessage = "User ID must be a valid GUID")]
    public string? UserIdFilter { get; set; }

    public bool IncludeMetadata { get; set; } = false;
    public bool IncludeStats { get; set; } = false;
}

/// <summary>
/// Simple query parameters for getting published items from table (no pagination)
/// </summary>
public class GetPublishedItemsFromTableRequest : BaseRequestDto
{
    [CollectionSize(0, 5, ErrorMessage = "Item types cannot exceed 5 items")]
    public string[] ItemTypes { get; set; } = Array.Empty<string>();

    [StringLength(100, ErrorMessage = "Category filter cannot exceed 100 characters")]
    public string? CategoryFilter { get; set; }

    public bool? IsPublicFilter { get; set; }

    [Range(1, 1000, ErrorMessage = "Limit must be between 1 and 1000")]
    public int? Limit { get; set; }

    public bool IncludeMetadata { get; set; } = false;
}

/// <summary>
/// Request to update published item metadata
/// </summary>
public class UpdatePublishedItemRequest : BaseRequestDto
{
    [Required(ErrorMessage = "Item ID is required")]
    [ValidGuid(ErrorMessage = "Item ID must be a valid GUID")]
    public string ItemId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Item name is required")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Item name must be between 1 and 200 characters")]
    public string ItemName { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }

    [StringLength(100, ErrorMessage = "Category cannot exceed 100 characters")]
    public string? Category { get; set; }

    [CollectionSize(0, 10, ErrorMessage = "Tags cannot exceed 10 items")]
    public string[]? Tags { get; set; }

    [ValidUrl(ErrorMessage = "Preview image must be a valid URL")]
    public string? PreviewImage { get; set; }

    public bool? IsPublic { get; set; }

    [StringLength(50, ErrorMessage = "Version cannot exceed 50 characters")]
    public string? Version { get; set; }

    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Request to get published layouts with specific filtering
/// </summary>
public class GetPublishedLayoutsRequest : BaseRequestDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0")]
    public int Page { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
    public int PageSize { get; set; } = 20;

    [StringLength(200, ErrorMessage = "Search query cannot exceed 200 characters")]
    public string? Search { get; set; }

    [StringLength(100, ErrorMessage = "Category filter cannot exceed 100 characters")]
    public string? CategoryFilter { get; set; }

    public bool IncludeLayoutData { get; set; } = false;
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }

    [StringLength(50, ErrorMessage = "Sort field cannot exceed 50 characters")]
    public string SortBy { get; set; } = "CreatedDate";

    [RegularExpression("^(asc|desc)$", ErrorMessage = "Sort order must be 'asc' or 'desc'")]
    public string SortOrder { get; set; } = "desc";
}