# GitHub Secrets Setup for Fabric Workload Development

## 🔐 Setting Up Secrets for Your Team

This repository uses GitHub Codespaces secrets to automatically configure your development environment with sensitive configuration values.

### Required Secrets

Add these secrets to your GitHub repository:
- **Repository Settings** → **Secrets and variables** → **Codespaces**

| Secret Name | Description | Example |
|------------|-------------|---------|
| `AZURE_CLIENT_ID` | AAD Application ID for your workload | `12345678-1234-1234-1234-123456789012` |
| `AZURE_CLIENT_SECRET` | AAD Application Secret | `your-secret-value` |
| `AZURE_TENANT_ID` | Your Azure tenant ID | `87654321-4321-4321-4321-210987654321` |
| `FABRIC_CAPACITY_ID` | Your Fabric capacity ID | `capacity-id-here` |
| `WORKLOAD_NAME` | Your workload name | `YourOrg.YourWorkload` |

### How It Works

1. **Codespaces Launch**: When you open this repository in Codespaces, the dev container automatically:
   - Reads your GitHub secrets as environment variables
   - Creates `local.settings.json` and `.env.dev` files
   - Installs dependencies

2. **Local Development**: If you're developing locally (not in Codespaces):
   - The setup script creates template files with placeholders
   - You'll need to manually update them with your values

### Files Created Automatically

- `api/local.settings.json` - .NET Azure Functions configuration
- `.env.dev` - Vite frontend environment variables (development)
- `.env.test` - Vite frontend environment variables (test)
- `.env.prod` - Vite frontend environment variables (production)
- `swa-cli.config.json` - Azure Static Web Apps CLI configuration

### Security Best Practices

✅ **Do This:**
- Store sensitive values in GitHub secrets
- Use different secrets for dev/test/prod environments
- Rotate secrets regularly

❌ **Don't Do This:**
- Commit `local.settings.json` or `.env` files to git
- Share secrets in chat or email
- Use production secrets in development

### Manual Setup (Local Development)

If you're not using Codespaces, create these files manually:

#### `api/local.settings.json`
```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "AZURE_CLIENT_ID": "your-aad-app-id",
    "AZURE_CLIENT_SECRET": "your-aad-app-secret",
    "AZURE_TENANT_ID": "your-tenant-id",
    "FABRIC_CAPACITY_ID": "your-capacity-id",
    "WORKLOAD_NAME": "YourOrg.YourWorkload"
  }
}
```

#### `.env.dev`
```bash
VITE_AZURE_CLIENT_ID=your-aad-app-id
VITE_AZURE_TENANT_ID=your-tenant-id
VITE_WORKLOAD_NAME=YourOrg.YourWorkload
VITE_FABRIC_CAPACITY_ID=your-capacity-id
VITE_ENVIRONMENT=development
VITE_API_BASE_URL=http://localhost:7071/api
VITE_FABRIC_GATEWAY_URL=http://127.0.0.1:60006
```

### Troubleshooting

**Problem**: "GitHub secrets not found" message
- **Solution**: Make sure you've added the secrets to your repository and are using Codespaces

**Problem**: Authentication errors
- **Solution**: Verify your AAD application ID and secret are correct

**Problem**: Files not created automatically
- **Solution**: Check that the postCreateCommand ran successfully in the terminal output