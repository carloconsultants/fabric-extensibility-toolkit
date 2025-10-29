# Fabric Workload Template

This is a comprehensive template for creating Microsoft Fabric workloads. It includes all the necessary components, patterns, and configurations to quickly spin up a new workload.

## Features

### Frontend
- **React 18** with TypeScript
- **TanStack Query** for server state management
- **cs-ui-library** integration for consistent UI components
- **Vite** for fast development and building
- **Fluent UI** components
- Pre-configured Fabric client hooks and queries

### Backend
- **C# Azure Functions** API
- **Azure Table Storage** integration
- **Azure Blob Storage** integration
- **Azure Key Vault** integration
- **Microsoft Fabric API** integration
- Comprehensive error handling with `ServiceResponse` pattern
- Dependency injection setup

### CI/CD & Deployment
- **Azure Static Web Apps** integration
- **GitHub Actions** workflow for automated deployments (preview and prod)
- **Azure DevOps** pipeline configuration
- **Two environments**: dev (preview) and prod
- **Automated setup scripts** for CI/CD infrastructure
- **PR preview** deployments
- **Local development** with SWA CLI

### Development Tools
- Environment configuration files (dev, prod)
- Pre-configured `.npmrc` for cs-ui-library access
- Copilot instructions for AI-assisted development
- Comprehensive logging and error handling
- Automated deployment scripts

## Quick Start

### 1. Clone and Setup
```bash
# Copy this template to your new workload directory
cp -r fabric-extensibility-toolkit/Workload your-new-workload/

# Navigate to your new workload
cd your-new-workload/

# Install dependencies
npm install
```

### 2. Set Up Azure Resources

#### A. Create App Registration
Follow the [App Registration Setup Guide](APP_REGISTRATION_SETUP.md) to create your Azure AD app registration with the required permissions.

#### B. Create Azure Static Web Apps
You'll need to create two Static Web Apps (one for preview/dev, one for production) to get the required URLs:

1. **Create Preview Static Web App:**
   - Go to [Azure Portal](https://portal.azure.com) → **Static Web Apps**
   - Click **"Create"**
   - Fill in:
     - **Name**: `{YourWorkloadName}-preview` (e.g., "MyDataApp-preview")
     - **Resource Group**: Create new or use existing
     - **Plan**: Free (for development)
   - Click **"Review + create"** → **"Create"**
   - After creation, go to **"Manage deployment token"** and copy the token (you'll need this for GitHub Actions)

2. **Create Production Static Web App:**
   - Repeat the same process with name `{YourWorkloadName}-prod`
   - Copy the deployment token for this one too

3. **Get the URLs:**
   - After creation, each Static Web App will have a URL like: `https://{name}.azurestaticapps.net`
   - Note these URLs - you'll need them for your environment configuration

### 3. Configure Environment (single source of truth)
Create and fill out the environment files used by both the frontend and API:

Required files in `Workload/` root:
```text
.env.dev
.env.prod
```

Minimal variables you must set (match entelexos/pbitips style):

`.env.dev` (preview)
```ini
# Workload identity
WORKLOAD_NAME=Org.FabricTools
ITEM_NAMES=HelloWorldItem
WORKLOAD_VERSION=1.0.0
LOG_LEVEL=info

# Azure AD (preview)
AZURE_TENANT_ID=<your-tenant-id>
FRONTEND_APPID=<your-frontend-app-id>

# URLs (from Static Web Apps)
FRONTEND_URL=https://your-workload-preview.azurestaticapps.net/
BACKEND_URL=https://your-workload-preview.azurestaticapps.net/api

# Storage (dev can use connection string)
STORAGE_ACCOUNT_NAME=<your-dev-storage-account>
STORAGE_CONNECTION_STRING=<your-dev-connection-string>

# Key Vault (optional for local)
KEY_VAULT_ENDPOINT=
MANAGED_ID_CLIENT_ID=
```

`.env.prod`
```ini
# Workload identity
WORKLOAD_NAME=Org.FabricTools
ITEM_NAMES=HelloWorldItem
WORKLOAD_VERSION=1.0.0
LOG_LEVEL=warn

# Azure AD (prod)
AZURE_TENANT_ID=<your-tenant-id>
FRONTEND_APPID=<your-frontend-app-id>

# URLs (from Static Web Apps)
FRONTEND_URL=https://your-workload-prod.azurestaticapps.net/
BACKEND_URL=https://your-workload-prod.azurestaticapps.net/api

# Storage (prod should use managed identity; no connection string)
STORAGE_ACCOUNT_NAME=<your-prod-storage-account>

# Key Vault (recommended)
KEY_VAULT_ENDPOINT=https://<your-kv>.vault.azure.net/
```

Notes:
- Local development uses `.env.dev` and overrides URLs with `http://localhost`.
- Add any extra variables your items need; keep prod free of secrets.

### 3. Update Configuration
- Update `package.json` name/version and any display strings
- Modify variables in `.env.dev` and `.env.prod` as above
- Update the workload manifest in `Manifest/` directory (product name, item ids)
- Customize the API namespace in C# files (replace `TemplateWorkload`)

### 4. Configure API local settings
Create or update `Workload/api/local.settings.json` for local Functions:
```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",

    "AZURE_TENANT_ID": "${AZURE_TENANT_ID}",
    "KEY_VAULT_ENDPOINT": "${KEY_VAULT_ENDPOINT}",
    "MANAGED_ID_CLIENT_ID": "${MANAGED_ID_CLIENT_ID}",

    "STORAGE_ACCOUNT_NAME": "${STORAGE_ACCOUNT_NAME}",
    "STORAGE_CONNECTION_STRING": "${STORAGE_CONNECTION_STRING}",

    "TABLE_1": "table1",
    "TABLE_2": "table2",
    "TABLE_3": "table3",

    "CONTAINER_1": "container1",
    "CONTAINER_2": "container2"
  }
}
```
Tip: The API reads these via `TemplateWorkload.Core.StorageConfig`.

### 5. Configure GitHub Actions (preview/prod)
If using GitHub Actions, update or add workflows under `.github/workflows/`:

#### A. Set up GitHub Secrets
Go to your repository → **Settings** → **Secrets and variables** → **Actions** and add:

- `AZURE_SWA_API_TOKEN_DEV` - Deployment token from your preview Static Web App
- `AZURE_SWA_API_TOKEN_MAIN` - Deployment token from your production Static Web App

#### B. Update Workflow Files
What to edit in your workflow files:
- Environment names: `preview` and `prod`
- Static Web App names and resource group
- Branch filters: preview from `preview` (or `develop`), prod from `main`
- Build env injection: pass `.env.dev` for preview, `.env.prod` for prod
- Update the `azure_static_web_apps_api_token` values to use the secrets above

Example deploy step snippet:
```yaml
    - name: Build frontend
      run: |
        cp .env.dev .env
        npm ci
        npm run build:prod

    - name: Deploy to Azure Static Web Apps
      uses: Azure/static-web-apps-deploy@v1
      with:
        azure_static_web_apps_api_token: ${{ secrets.SWA_PREVIEW_TOKEN }}
        app_location: Workload/app
        output_location: build/Frontend
```

### 6. Start Development
```bash
# Start the development server
npm run start:devServer

# In another terminal, start the API server
npm run start:apiServer
```

### 7. Deploy (if CI/CD is set up)
```bash
# Deploy to development
./scripts/deploy.ps1 -Environment "dev" -DeploymentToken "your-dev-token"

# Deploy to production
./scripts/deploy.ps1 -Environment "prod" -DeploymentToken "your-prod-token"
```

## Project Structure

```
Workload/
├── api/                          # C# Azure Functions API
│   ├── Controllers/              # API controllers
│   ├── Services/                 # Business logic services
│   ├── Models/                   # Data models
│   ├── Core/                     # Core services (FabricApi, KeyVault, etc.)
│   ├── Utilities/                # Shared utilities
│   └── Program.cs                # API startup configuration
├── app/                          # React frontend
│   ├── hooks/queries/            # TanStack Query hooks
│   ├── clients/                  # API client implementations
│   ├── components/               # Reusable UI components
│   ├── items/                    # Fabric item editors
│   └── App.tsx                   # Main app component
├── Manifest/                     # Fabric workload manifest
├── .env.dev                      # Development (preview) environment
├── .env.prod                     # Production environment
└── .npmrc                        # NPM registry configuration
```

## Key Components

### Frontend Hooks
- `useGetWorkspaces()`: Get all accessible workspaces
- `useGetLakehouses(workspaceId)`: Get lakehouses for a workspace
- `useGetLakehouseTables(workspaceId, lakehouseId)`: Get tables in a lakehouse
- `useFabricWorkspaceUsers()`: Get users from all workspaces
- `useFabricWorkspaceUsersByWorkspace(workspaceId)`: Get users for specific workspace

### Backend Services
- `IFabricApi`: Microsoft Fabric API operations
- `IKeyVaultAccess`: Azure Key Vault operations
- `IAzureTableClient`: Azure Table Storage operations
- `IBlobStorageClient`: Azure Blob Storage operations

### UI Components
- `Layout`: Main layout component from cs-ui-library
- Fluent UI components for consistent Microsoft design
- Pre-configured theme and styling

## Environment Variables

### Required Variables
- `WORKLOAD_NAME`: Name of your workload
- `ITEM_NAMES`: Comma-separated list of item types
- `WORKLOAD_VERSION`: Version of your workload
- `AZURE_TENANT_ID`: Azure tenant ID
- `FRONTEND_APPID`: Frontend application ID (from App Registration)
- `FRONTEND_URL`: Frontend application URL (from Static Web App)
- `BACKEND_URL`: Backend API URL (from Static Web App)
- `STORAGE_ACCOUNT_NAME`: Azure Storage account name
- `STORAGE_CONNECTION_STRING`: Azure Storage connection string

### Optional Variables
- `KEY_VAULT_ENDPOINT`: Azure Key Vault endpoint
- `MANAGED_ID_CLIENT_ID`: Managed identity client ID
- `BACKEND_APPID`: Backend application ID (if using separate backend auth)
- Various table and container names for storage

## Development Guidelines

### Frontend Development
1. Use TanStack Query for all data fetching
2. Wrap components with the Layout component
3. Follow the patterns in `app/COPILOT_INSTRUCTIONS.md`
4. Use TypeScript for all components
5. Implement proper error handling and loading states

### Backend Development
1. Use the ServiceResponse pattern for all API responses
2. Follow the patterns in `api/COPILOT_INSTRUCTIONS.md`
3. Use dependency injection for all services
4. Implement proper logging and error handling
5. Use the Configuration class for environment variables

## Deployment

### Frontend
```bash
# Build for production
npm run build:prod

# The built files will be in the build/Frontend directory
```

### Backend
Deploy the `api/` directory as an Azure Functions app with the required environment variables configured.

## Customization

### Adding New Features
1. Create new hooks in `app/hooks/queries/` for data fetching
2. Add new services in `api/Services/` for business logic
3. Create new controllers in `api/Controllers/` for API endpoints
4. Add new models in `api/Models/` for data structures

### UI Customization
1. Use cs-ui-library components for consistency
2. Extend the theme in `app/theme.tsx`
3. Add custom components in `app/components/`
4. Follow Fluent UI design guidelines

## Support

- Check the copilot instructions in `api/COPILOT_INSTRUCTIONS.md` and `app/COPILOT_INSTRUCTIONS.md`
- Refer to the Microsoft Fabric documentation
- Use the cs-ui-library documentation for UI components
- Check TanStack Query documentation for data fetching patterns

## License

This template follows the same license as the original Microsoft Fabric Extensibility Toolkit.
