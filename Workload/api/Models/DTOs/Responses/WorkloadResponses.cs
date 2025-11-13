namespace PowerBITips.Api.Models.DTOs.Responses;

public class WorkloadInfoResponse
{
    public string WorkspaceId { get; set; } = string.Empty;
    public string WorkspaceName { get; set; } = string.Empty;
    public List<WorkloadItemSummary> Items { get; set; } = new();
    public WorkloadMetadata Metadata { get; set; } = new();
    public Dictionary<string, int> ItemTypeCounts { get; set; } = new();
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public string? ContinuationToken { get; set; }
}

public class WorkloadItemSummary
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedDateTime { get; set; }
    public DateTime LastModifiedDateTime { get; set; }
    public List<string> Tags { get; set; } = new();
    public string? Category { get; set; }
    public Dictionary<string, object>? BasicProperties { get; set; }
}

public class WorkloadMetadata
{
    public string Status { get; set; } = "Active";
    public int ItemCount { get; set; }
    public long TotalSizeBytes { get; set; }
    public DateTime LastActivity { get; set; }
    public Dictionary<string, object>? AdditionalInfo { get; set; }
}

public class WorkloadItemPayloadResponse
{
    public string ItemId { get; set; } = string.Empty;
    public string ItemType { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public Dictionary<string, object> ItemPayload { get; set; } = new();
    public string ContentType { get; set; } = "application/json";
    public long Size { get; set; }
    public string? Version { get; set; }
    public DateTime RetrievedAt { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object>? Metadata { get; set; }
}

public class UpdateWorkloadItemResponse
{
    public bool Success { get; set; }
    public string ItemId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, object> UpdatedProperties { get; set; } = new();
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string? Version { get; set; }
    public List<string> ModifiedFields { get; set; } = new();
}

public class FabricApiProxyResponse
{
    public int StatusCode { get; set; }
    public string? Body { get; set; }
    public Dictionary<string, string> Headers { get; set; } = new();
    public string ContentType { get; set; } = string.Empty;
    public TimeSpan ProcessingTime { get; set; }
    public DateTime ResponseTime { get; set; } = DateTime.UtcNow;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

public class WorkloadItemCreateResponse
{
    public bool Success { get; set; }
    public string ItemId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string ItemType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string WorkspaceId { get; set; } = string.Empty;
    public Dictionary<string, object>? CreatedProperties { get; set; }
    public string? Message { get; set; }
}

public class WorkloadBatchOperationResponse
{
    public string BatchId { get; set; } = string.Empty;
    public int TotalOperations { get; set; }
    public int SuccessfulOperations { get; set; }
    public int FailedOperations { get; set; }
    public List<WorkloadOperationResult> Results { get; set; } = new();
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    public TimeSpan ProcessingTime { get; set; }
    public bool OverallSuccess { get; set; }
}

public class WorkloadOperationResult
{
    public string? OperationId { get; set; }
    public string ItemId { get; set; } = string.Empty;
    public string OperationType { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object>? ResultData { get; set; }
}

public class WorkloadUsageStatsResponse
{
    public string WorkspaceId { get; set; } = string.Empty;
    public Dictionary<string, int> ApiCallsByEndpoint { get; set; } = new();
    public Dictionary<string, int> ItemAccessByType { get; set; } = new();
    public int TotalApiCalls { get; set; }
    public TimeSpan AverageResponseTime { get; set; }
    public DateTime StatsGeneratedAt { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object>? AdditionalMetrics { get; set; }
}