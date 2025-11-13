using System.ComponentModel.DataAnnotations;

namespace PowerBITips.Api.Models.DTOs.Requests;

public class WorkloadInfoRequest
{
    [Required]
    public string WorkspaceId { get; set; } = string.Empty;
    
    public string? IncludeFields { get; set; } // Comma-separated list of fields to include
    
    public string? ItemType { get; set; } // Filter by specific item type
    
    public int? Limit { get; set; }
    
    public string? ContinuationToken { get; set; }
}

public class WorkloadItemPayloadRequest
{
    [Required]
    public string WorkspaceId { get; set; } = string.Empty;
    
    [Required]
    public string ItemType { get; set; } = string.Empty;
    
    [Required]
    public string ItemId { get; set; } = string.Empty;
    
    public string? Version { get; set; }
    
    public string? Format { get; set; } = "json"; // "json", "xml", "binary"
    
    public bool? IncludeMetadata { get; set; } = true;
}

public class UpdateWorkloadItemRequest
{
    public string? DisplayName { get; set; }
    
    public string? Description { get; set; }
    
    public Dictionary<string, object>? Properties { get; set; }
    
    public Dictionary<string, object>? Configuration { get; set; }
    
    public List<string>? Tags { get; set; }
    
    public string? Category { get; set; }
}

public class FabricApiProxyRequest
{
    [Required]
    public string Method { get; set; } = string.Empty; // GET, POST, PUT, PATCH, DELETE
    
    [Required]
    public string Path { get; set; } = string.Empty; // The path to proxy to Fabric API
    
    public Dictionary<string, string>? Headers { get; set; }
    
    public Dictionary<string, string>? QueryParameters { get; set; }
    
    public string? Body { get; set; }
    
    public string? ContentType { get; set; } = "application/json";
    
    public TimeSpan? Timeout { get; set; } = TimeSpan.FromSeconds(30);
}

public class WorkloadItemCreateRequest
{
    [Required]
    public string WorkspaceId { get; set; } = string.Empty;
    
    [Required]
    public string ItemType { get; set; } = string.Empty;
    
    [Required]
    public string DisplayName { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    public Dictionary<string, object>? Definition { get; set; }
    
    public Dictionary<string, object>? Configuration { get; set; }
    
    public List<string>? Tags { get; set; }
}

public class WorkloadBatchOperationRequest
{
    [Required]
    public string WorkspaceId { get; set; } = string.Empty;
    
    [Required]
    public List<WorkloadBatchOperation> Operations { get; set; } = new();
    
    public bool? ContinueOnError { get; set; } = false;
    
    public Dictionary<string, object>? GlobalSettings { get; set; }
}

public class WorkloadBatchOperation
{
    [Required]
    public string OperationType { get; set; } = string.Empty; // "create", "update", "delete"
    
    [Required]
    public string ItemId { get; set; } = string.Empty;
    
    public string? ItemType { get; set; }
    
    public Dictionary<string, object>? Data { get; set; }
    
    public string? OperationId { get; set; }
}