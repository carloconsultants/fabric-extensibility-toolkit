using System.Text.Json.Serialization;

namespace PowerBITips.Api.Models.Authentication
{
    public class ClientPrincipal
    {
        [JsonPropertyName("identityProvider")]
        public string IdentityProvider { get; set; } = string.Empty;

        [JsonPropertyName("userId")]
        public string UserId { get; set; } = string.Empty;

        [JsonPropertyName("userDetails")]
        public string UserDetails { get; set; } = string.Empty;

        [JsonPropertyName("userRoles")]
        public string[] UserRoles { get; set; } = Array.Empty<string>();

        [JsonPropertyName("claims")]
        public Claim[] Claims { get; set; } = Array.Empty<Claim>();
    }

    public class Claim
    {
        [JsonPropertyName("typ")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("val")]
        public string Value { get; set; } = string.Empty;
    }
}