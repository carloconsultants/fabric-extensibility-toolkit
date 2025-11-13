using Microsoft.Azure.Functions.Worker.Http;
using PowerBITips.Api.Models.Authentication;

namespace PowerBITips.Api.Services.Interfaces
{
    public interface IAuthenticationService
    {
        ClientPrincipal? GetClientPrincipal(HttpRequestData request);
        Task<string?> GetAccessTokenAsync(ClientPrincipal principal, string[] scopes);
        Task<string?> GetFabricAccessTokenAsync(ClientPrincipal principal);
    }
}