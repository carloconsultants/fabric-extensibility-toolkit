using System.ComponentModel.DataAnnotations;

namespace PowerBITips.Api.Models.DTOs.Requests;

/// <summary>
/// Request model for creating a workload item via API
/// </summary>
public class CreateWorkloadItemRequest
{
    /// <summary>
    /// Display name for the new item
    /// </summary>
    [MaxLength(256)]
    public string? DisplayName { get; set; }

    /// <summary>
    /// Optional description for the item
    /// </summary>
    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Optional creation payload containing initial item data
    /// </summary>
    public Dictionary<string, object>? CreationPayload { get; set; }
}