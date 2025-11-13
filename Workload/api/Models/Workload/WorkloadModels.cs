using System.ComponentModel.DataAnnotations;

namespace PowerBITips.Api.Models.Workload
{
    // Workload item types
    public enum WorkloadItemType
    {
        Report,
        Dataset,
        Dashboard,
        Dataflow,
        Notebook
    }

    // Request/Response models for workload operations
    public class WorkloadItem
    {
        public string? Id { get; set; }
        public string? WorkspaceId { get; set; }
        public WorkloadItemType Type { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public Dictionary<string, object>? Properties { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }

    public class WorkloadItemPayload
    {
        public string? ItemId { get; set; }
        public WorkloadItemType ItemType { get; set; }
        public Dictionary<string, object>? Payload { get; set; }
        public string? ContentType { get; set; }
        public byte[]? Data { get; set; }
    }

    public class WorkloadProxyRequest
    {
        public string? Method { get; set; }
        public string? Path { get; set; }
        public Dictionary<string, string>? Headers { get; set; }
        public Dictionary<string, string>? QueryParameters { get; set; }
        public object? Body { get; set; }
    }

    public class WorkloadProxyResponse
    {
        public int StatusCode { get; set; }
        public Dictionary<string, string>? Headers { get; set; }
        public string? Body { get; set; }
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
    }

    // Request DTOs
    public class GetWorkloadInfoRequest
    {
        [Required]
        public string? WorkspaceId { get; set; }
    }

    public class GetItemPayloadRequest
    {
        [Required]
        public string? WorkspaceId { get; set; }
        
        [Required]
        public string? ItemType { get; set; }
        
        [Required]
        public string? ItemId { get; set; }
    }

    public class UpdateWorkloadItemRequest
    {
        [Required]
        public string? WorkspaceId { get; set; }
        
        [Required]
        public string? ItemType { get; set; }
        
        [Required]
        public string? ItemId { get; set; }
        
        public Dictionary<string, object>? Properties { get; set; }
        public object? Payload { get; set; }
    }

    // Response DTOs
    public class WorkloadInfoResponse
    {
        public string? WorkspaceId { get; set; }
        public List<WorkloadItem>? Items { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class ItemPayloadResponse
    {
        public string? ItemId { get; set; }
        public WorkloadItemType ItemType { get; set; }
        public Dictionary<string, object>? ItemPayload { get; set; }
        public string? ContentType { get; set; }
        public int? Size { get; set; }
    }

    public class UpdateWorkloadItemResponse
    {
        public bool Success { get; set; }
        public string? ItemId { get; set; }
        public string? Message { get; set; }
        public Dictionary<string, object>? UpdatedProperties { get; set; }
    }

    // Fabric API integration models
    public class FabricApiRequest
    {
        public string? Url { get; set; }
        public string? Method { get; set; }
        public Dictionary<string, string>? Headers { get; set; }
        public string? Body { get; set; }
    }

    public class FabricApiResponse
    {
        public int StatusCode { get; set; }
        public string? Body { get; set; }
        public Dictionary<string, string>? Headers { get; set; }
        public bool IsSuccess => StatusCode >= 200 && StatusCode < 300;
    }
}