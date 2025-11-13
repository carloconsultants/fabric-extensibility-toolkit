namespace PowerBITips.Api.Models.Workload;

/// <summary>
/// Service model for creating workload items
/// </summary>
public class CreateWorkloadItemRequest
{
    public required string WorkspaceId { get; set; }
    public required string ItemType { get; set; }
    public required string DisplayName { get; set; }
    public string? Description { get; set; }
    public Dictionary<string, object>? CreationPayload { get; set; }
}

/// <summary>
/// Response model for created workload items
/// </summary>
public class CreateWorkloadItemResponse
{
    public required string ItemId { get; set; }
    public required string ItemType { get; set; }
    public required string DisplayName { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? CreatedBy { get; set; }
    public bool Success { get; set; } = true;
    public string? Message { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}