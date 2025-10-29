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

### Development Tools
- Environment configuration files (dev, test, prod)
- Pre-configured `.npmrc` for cs-ui-library access
- Copilot instructions for AI-assisted development
- Comprehensive logging and error handling

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

### 2. Configure Environment
Copy and modify the environment files:
```bash
cp .env.dev .env.local
# Edit .env.local with your specific configuration
```

### 3. Update Configuration
- Update `package.json` with your workload name
- Modify environment variables in `.env.*` files
- Update the workload manifest in `Manifest/` directory
- Customize the API namespace in C# files (replace `TemplateWorkload`)

### 4. Start Development
```bash
# Start the development server
npm run start:devServer

# In another terminal, start the API server
npm run start:apiServer
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
├── .env.dev                      # Development environment
├── .env.test                     # Test environment
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
- `KEY_VAULT_ENDPOINT`: Azure Key Vault endpoint
- `STORAGE_ACCOUNT_NAME`: Azure Storage account name
- `STORAGE_CONNECTION_STRING`: Azure Storage connection string

### Optional Variables
- `FRONTEND_APPID`: Frontend application ID
- `BACKEND_APPID`: Backend application ID
- `BACKEND_URL`: Backend API URL
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
