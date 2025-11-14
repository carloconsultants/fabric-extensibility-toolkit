using PowerBITips.Api.Models.Authentication;
using PowerBITips.Api.Models.Workload;
using PowerBITips.Api.Services.Common;

namespace PowerBITips.Api.Services.Interfaces
{
    public interface IWorkloadService
    {
        Task<ServiceResponse<WorkloadInfoResponse>> GetWorkloadInfoAsync(string workspaceId);
        Task<ServiceResponse<ItemPayloadResponse>> GetItemPayloadAsync(string workspaceId, string itemType, string itemId);
        Task<ServiceResponse<CreateWorkloadItemResponse>> CreateWorkloadItemAsync(string workspaceId, string itemType, CreateWorkloadItemRequest request);
        Task<ServiceResponse<UpdateWorkloadItemResponse>> UpdateWorkloadItemAsync(string workspaceId, string itemType, string itemId, UpdateWorkloadItemRequest request);
        Task<ServiceResponse<WorkloadProxyResponse>> ProxyToFabricApiAsync(string path, string method, ClientPrincipal? principal = null, Dictionary<string, string>? headers = null, string? body = null, Dictionary<string, string>? queryParams = null);
    }
}