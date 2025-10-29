using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Logging;

namespace TemplateWorkload.Core
{
    public interface IKeyVaultAccess
    {
        Task<KeyVaultSecret> CreateSecretAsync(string secretName, string secretValue);
        Task<bool> UpdateSecretAsync(string secretName, string secretValue);
        Task<string> GetSecretAsync(string secretName);
        Task DeleteSecretAsync(string secretName);
    }

    public class KeyVaultAccess : IKeyVaultAccess
    {
        private readonly SecretClient? _client;
        private readonly ILogger<KeyVaultAccess> _logger;

        public KeyVaultAccess(ILogger<KeyVaultAccess> logger)
        {
            _logger = logger;
            var keyVaultEndpoint = Environment.GetEnvironmentVariable("KEY_VAULT_ENDPOINT");
            if (string.IsNullOrEmpty(keyVaultEndpoint))
            {
                _logger.LogWarning("Key Vault endpoint is not configured. KeyVault operations will not be available.");
                // For local development, allow initialization without KeyVault
                // KeyVault operations will fail gracefully when called
                return;
            }

            // Check if we have explicit Azure credentials configured
            var clientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID");
            var clientSecret = Environment.GetEnvironmentVariable("AZURE_CLIENT_SECRET");
            var tenantId = Environment.GetEnvironmentVariable("AZURE_TENANT_ID");

            if (!string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(clientSecret) && !string.IsNullOrEmpty(tenantId))
            {
                _logger.LogInformation("Using explicit Azure credentials for Key Vault access.");
                var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
                _client = new SecretClient(new Uri(keyVaultEndpoint), credential);
            }
            else
            {
                // Fallback to managed identity or default credential
                var userAssignedIdentity = Environment.GetEnvironmentVariable("MANAGED_ID_CLIENT_ID");
                if (!string.IsNullOrEmpty(userAssignedIdentity))
                {
                    var options = new DefaultAzureCredentialOptions
                    {
                        ManagedIdentityClientId = userAssignedIdentity
                    };
                    _client = new SecretClient(new Uri(keyVaultEndpoint), new DefaultAzureCredential(options));
                }
                else
                {
                    _logger.LogInformation("Using DefaultAzureCredential for Key Vault access.");
                    var credential = new DefaultAzureCredential();
                    _client = new SecretClient(new Uri(keyVaultEndpoint), credential);
                }
            }
        }

        public async Task<KeyVaultSecret> CreateSecretAsync(string secretName, string secretValue)
        {
            if (_client == null)
            {
                _logger.LogWarning("Key Vault client is not configured. Cannot create secret.");
                throw new InvalidOperationException("Key Vault is not configured for local development.");
            }

            try
            {
                KeyVaultSecret result = await _client.SetSecretAsync(secretName, secretValue);
                _logger.LogInformation($"Successfully created secret: {secretName}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"ERROR Creating Secret:: {ex.Message}");
                throw new Exception("Secret Creation Error", ex);
            }
        }

        public async Task<bool> UpdateSecretAsync(string secretName, string secretValue)
        {
            if (_client == null)
            {
                _logger.LogWarning("Key Vault client is not configured. Cannot update secret.");
                return false;
            }

            try
            {
                KeyVaultSecret result = await _client.SetSecretAsync(secretName, secretValue);
                _logger.LogInformation($"Successfully updated secret: {secretName}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"ERROR Updating Secret:: {ex.Message}");
                return false;
            }
        }

        public async Task<string> GetSecretAsync(string secretName)
        {
            if (_client == null)
            {
                _logger.LogWarning("Key Vault client is not configured. Cannot get secret.");
                throw new InvalidOperationException("Key Vault is not configured for local development.");
            }

            try
            {
                KeyVaultSecret result = await _client.GetSecretAsync(secretName);
                _logger.LogInformation($"Secret retrieved successfully: {secretName}");
                return result.Value;
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"ERROR:: {ex.Message}");
                throw new Exception("Secret Lookup Error", ex);
            }
        }

        public async Task DeleteSecretAsync(string secretName)
        {
            if (_client == null)
            {
                _logger.LogWarning("Key Vault client is not configured. Cannot delete secret.");
                throw new InvalidOperationException("Key Vault is not configured for local development.");
            }

            try
            {
                await _client.StartDeleteSecretAsync(secretName);
                _logger.LogInformation($"Successfully initiated deletion of secret: {secretName}");
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"ERROR Deleting Secret:: {ex.Message}");
                throw new Exception("Secret Deletion Error", ex);
            }
        }
    }
}
