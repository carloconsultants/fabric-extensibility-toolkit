using Azure.Storage.Blobs;

namespace TemplateWorkload.Services
{
    public interface IBlobStorageClient
    {
        Task<BlobContainerClient> GetContainerClientAsync(string containerName);
        Task<bool> ContainerExistsAsync(string containerName);
        Task CreateContainerIfNotExistsAsync(string containerName);
    }
}
