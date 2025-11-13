using System.ComponentModel.DataAnnotations;
using PowerBITips.Api.Models.Enums;
using PowerBITips.Api.Models.DTOs.Common;
using PowerBITips.Api.Models.DTOs.Validation;

namespace PowerBITips.Api.Models.DTOs.Requests;

/// <summary>
/// Request to save a PowerBI theme
/// </summary>
public class SaveThemeRequest : BaseRequestDto
{
    [Required(ErrorMessage = "Theme ID is required")]
    [ValidGuid(ErrorMessage = "Theme ID must be a valid GUID")]
    public string Id { get; set; } = string.Empty;

    [Required(ErrorMessage = "Theme payload is required")]
    [ValidJson(ErrorMessage = "Theme payload must be valid JSON")]
    public string? Payload { get; set; }

    [Required(ErrorMessage = "Theme name is required")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Theme name must be between 1 and 100 characters")]
    public string ThemeName { get; set; } = string.Empty;

    [ValidJson(ErrorMessage = "Project payload must be valid JSON")]
    public string? ProjectPayload { get; set; }

    [CollectionSize(0, 10, ErrorMessage = "Project images cannot exceed 10 items")]
    public string[]? ProjectImages { get; set; }

    [StringLength(100, ErrorMessage = "Scrims family name cannot exceed 100 characters")]
    public string? ScrimsFamilyName { get; set; }

    [CollectionSize(0, 5, ErrorMessage = "Publish options cannot exceed 5 items")]
    public string[]? PublishOptions { get; set; }

    public bool? IsCommunity { get; set; }

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }

    [StringLength(50, ErrorMessage = "Category cannot exceed 50 characters")]
    public string? Category { get; set; }

    public string[]? Tags { get; set; }
}

/// <summary>
/// Request to delete a user theme
/// </summary>
public class DeleteThemeRequest : BaseRequestDto
{
    [Required(ErrorMessage = "Theme GUID is required")]
    [ValidGuid(ErrorMessage = "Theme GUID must be a valid GUID")]
    public string Guid { get; set; } = string.Empty;
}

/// <summary>
/// Request to get a specific user theme
/// </summary>
public class GetThemeRequest : BaseRequestDto
{
    [Required(ErrorMessage = "Theme GUID is required")]
    [ValidGuid(ErrorMessage = "Theme GUID must be a valid GUID")]
    public string Guid { get; set; } = string.Empty;

    public bool IncludePayload { get; set; } = true;
    public bool IncludeProjectData { get; set; } = true;
    public bool IncludeImages { get; set; } = true;
}

/// <summary>
/// Request to get user themes with filtering and pagination
/// </summary>
public class GetUserThemesRequest : BaseRequestDto
{
    [Required(ErrorMessage = "User ID is required")]
    [ValidGuid(ErrorMessage = "User ID must be a valid GUID")]
    public string UserId { get; set; } = string.Empty;

    [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
    public int PageSize { get; set; } = 20;

    [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0")]
    public int PageNumber { get; set; } = 1;

    [StringLength(100, ErrorMessage = "Search query cannot exceed 100 characters")]
    public string? SearchQuery { get; set; }

    [StringLength(50, ErrorMessage = "Category filter cannot exceed 50 characters")]
    public string? CategoryFilter { get; set; }

    public bool? IsCommunityFilter { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
    public bool IncludePayload { get; set; } = false;
    public bool IncludeProjectData { get; set; } = false;

    [StringLength(20, ErrorMessage = "Sort field cannot exceed 20 characters")]
    public string SortBy { get; set; } = "CreatedAt";

    public bool SortDescending { get; set; } = true;
}

/// <summary>
/// Request to update theme metadata (without changing payload)
/// </summary>
public class UpdateThemeMetadataRequest : BaseRequestDto
{
    [Required(ErrorMessage = "Theme GUID is required")]
    [ValidGuid(ErrorMessage = "Theme GUID must be a valid GUID")]
    public string Guid { get; set; } = string.Empty;

    [Required(ErrorMessage = "Theme name is required")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Theme name must be between 1 and 100 characters")]
    public string ThemeName { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }

    [StringLength(50, ErrorMessage = "Category cannot exceed 50 characters")]
    public string? Category { get; set; }

    public string[]? Tags { get; set; }
    public bool? IsCommunity { get; set; }

    [StringLength(100, ErrorMessage = "Scrims family name cannot exceed 100 characters")]
    public string? ScrimsFamilyName { get; set; }
}