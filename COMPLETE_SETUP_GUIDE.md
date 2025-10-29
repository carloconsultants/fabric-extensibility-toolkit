# Complete Fabric Workload Setup Guide

This guide shows you how to set up a new Fabric workload using a single configuration file. Everything is automated - just fill out one JSON file and run one command!

## 🚀 One-Command Setup

### Step 1: Copy the Template
```bash
# Copy the template to your new workload directory
cp -r fabric-extensibility-toolkit/Workload your-new-workload/
cd your-new-workload/
```

### Step 2: Configure Your Workload
Edit the `workload-config.json` file with your specific values:

```json
{
  "workload": {
    "name": "MyDataApp",
    "displayName": "My Data Application",
    "description": "A powerful data management application for Fabric",
    "version": "1.0.0",
    "organization": "MyOrg",
    "itemNames": "DataApp,DataViewer",
    "author": "John Doe",
    "email": "john.doe@company.com"
  },
  "azure": {
    "subscriptionId": "12345678-1234-1234-1234-123456789012",
    "tenantId": "87654321-4321-4321-4321-210987654321",
    "resourceGroup": "MyDataApp-01",
    "location": "East US",
    "storageAccount": {
      "name": "mydataappstorage",
      "connectionString": "DefaultEndpointsProtocol=https;AccountName=mydataappstorage;AccountKey=..."
    },
    "keyVault": {
      "endpoint": "https://mydataapp-keyvault.vault.azure.net/"
    }
  },
  "environments": {
    "dev": {
      "name": "MyDataApp-swa-dev",
      "branch": "develop",
      "aad": {
        "frontendAppId": "11111111-1111-1111-1111-111111111111",
        "backendAppId": "22222222-2222-2222-2222-222222222222",
        "clientId": "33333333-3333-3333-3333-333333333333",
        "clientSecret": "your-dev-client-secret",
        "audience": "api://22222222-2222-2222-2222-222222222222",
        "redirectUri": "https://mydataapp-swa-dev.azurestaticapps.net"
      },
      "url": "https://mydataapp-swa-dev.azurestaticapps.net"
    },
    "prod": {
      "name": "MyDataApp-swa-prod",
      "branch": "main",
      "aad": {
        "frontendAppId": "44444444-4444-4444-4444-444444444444",
        "backendAppId": "55555555-5555-5555-5555-555555555555",
        "clientId": "66666666-6666-6666-6666-666666666666",
        "clientSecret": "your-prod-client-secret",
        "audience": "api://55555555-5555-5555-5555-555555555555",
        "redirectUri": "https://mydataapp-swa-prod.azurestaticapps.net"
      },
      "url": "https://mydataapp-swa-prod.azurestaticapps.net"
    }
  },
  "github": {
    "repository": "myorg/mydataapp",
    "organization": "myorg"
  }
}
```

### Step 3: Run the Setup
```bash
# Run the complete setup (creates everything)
./setup-workload.ps1

# Or run with options
./setup-workload.ps1 -Force $true -SkipAzureResources $false
```

**That's it!** 🎉 Your workload is now fully configured and ready to go.

## 📋 What Gets Configured Automatically

### 1. **File Updates**
- ✅ `package.json` - Workload name, version, description
- ✅ All C# files - Namespace updates (`TemplateWorkload` → `MyDataApp`)
- ✅ All TypeScript files - Name replacements
- ✅ `swa-cli.config.json` - Azure Static Web App configuration
- ✅ `azure-pipelines.yml` - Azure DevOps pipeline
- ✅ `.github/workflows/deploy.yml` - GitHub Actions workflow

### 2. **Environment Files**
- ✅ `.env.dev` - Development environment variables
- ✅ `.env.test` - Test environment variables  
- ✅ `.env.prod` - Production environment variables

### 3. **Azure Resources** (if not skipped)
- ✅ Resource Group creation
- ✅ Static Web Apps for each environment
- ✅ Proper naming and configuration

### 4. **Documentation**
- ✅ `README.md` - Customized for your workload
- ✅ Environment URLs and configuration

## 🔧 Configuration Reference

### Workload Section
```json
"workload": {
  "name": "MyDataApp",                    // Used for namespacing, file names
  "displayName": "My Data Application",   // Human-readable name
  "description": "Description here",      // App description
  "version": "1.0.0",                    // Version number
  "organization": "MyOrg",               // Organization prefix
  "itemNames": "DataApp,DataViewer",     // Comma-separated item types
  "author": "Your Name",                 // Author name
  "email": "your@email.com"              // Contact email
}
```

### Azure Section
```json
"azure": {
  "subscriptionId": "your-sub-id",       // Azure subscription ID
  "tenantId": "your-tenant-id",          // Azure AD tenant ID
  "resourceGroup": "MyDataApp-01",       // Resource group name
  "location": "East US",                 // Azure region
  "storageAccount": {
    "name": "mydataappstorage",          // Storage account name
    "connectionString": "your-connection" // Storage connection string
  },
  "keyVault": {
    "endpoint": "https://myapp-kv.vault.azure.net/" // Key Vault URL
  }
}
```

### Environment Section
Each environment (dev, test, prod) needs:
```json
"dev": {
  "name": "MyDataApp-swa-dev",           // Static Web App name
  "branch": "develop",                   // Git branch for this environment
  "aad": {
    "frontendAppId": "app-id",           // Frontend Azure AD app ID
    "backendAppId": "api-id",            // Backend Azure AD app ID
    "clientId": "client-id",             // Service principal client ID
    "clientSecret": "client-secret",     // Service principal secret
    "audience": "api://api-id",          // API audience
    "redirectUri": "https://app-url"     // Redirect URI
  },
  "url": "https://app-url"               // Final app URL
}
```

## 🚀 Quick Start Commands

### Basic Setup
```bash
# 1. Copy template
cp -r fabric-extensibility-toolkit/Workload my-workload/
cd my-workload/

# 2. Edit configuration
# Edit workload-config.json with your values

# 3. Run setup
./setup-workload.ps1

# 4. Install dependencies
npm install

# 5. Start development
npm run start:devServer
```

### Advanced Setup Options
```bash
# Skip Azure resource creation (useful for testing)
./setup-workload.ps1 -SkipAzureResources $true

# Skip CI/CD setup
./setup-workload.ps1 -SkipCICD $true

# Use custom config file
./setup-workload.ps1 -ConfigPath "my-custom-config.json"

# Force setup without confirmation
./setup-workload.ps1 -Force $true
```

## 🔄 Updating Configuration

To update your workload configuration:

1. **Edit the config file**:
   ```bash
   # Edit workload-config.json with new values
   code workload-config.json
   ```

2. **Re-run setup**:
   ```bash
   # This will update all files with new values
   ./setup-workload.ps1
   ```

## 🎯 What You Get

After running the setup, you'll have:

### **Frontend** (React + TypeScript)
- ✅ TanStack Query for data fetching
- ✅ cs-ui-library integration
- ✅ Fabric client hooks and queries
- ✅ Vite for fast development
- ✅ Fluent UI components

### **Backend** (C# Azure Functions)
- ✅ Complete API structure
- ✅ Azure Storage integration
- ✅ Key Vault integration
- ✅ Fabric API client
- ✅ ServiceResponse pattern
- ✅ Dependency injection

### **CI/CD** (Automated Deployment)
- ✅ GitHub Actions workflow
- ✅ Azure DevOps pipeline
- ✅ Multi-environment support
- ✅ PR preview deployments
- ✅ Automated builds and tests

### **Infrastructure** (Azure Resources)
- ✅ Static Web Apps for each environment
- ✅ Resource group and configuration
- ✅ Environment-specific settings
- ✅ Proper naming and organization

## 🔍 Troubleshooting

### Common Issues

**1. Configuration File Errors**
```bash
# Validate your JSON
cat workload-config.json | jq .
```

**2. Azure CLI Not Found**
```bash
# Install Azure CLI
# Windows: winget install Microsoft.AzureCLI
# Mac: brew install azure-cli
# Linux: curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash
```

**3. Permission Issues**
```bash
# Make scripts executable
chmod +x setup-workload.ps1
chmod +x scripts/*.ps1
```

**4. Missing Dependencies**
```bash
# Install Node.js dependencies
npm install

# Install .NET dependencies
cd api && dotnet restore
```

### Getting Help

1. **Check the logs** - The setup script provides detailed output
2. **Validate configuration** - Ensure all required fields are filled
3. **Check Azure permissions** - Ensure you have rights to create resources
4. **Review generated files** - Check that files were updated correctly

## 📚 Next Steps

After setup completion:

1. **Install dependencies**: `npm install`
2. **Configure Azure AD**: Set up your app registrations
3. **Set up GitHub secrets**: Add deployment tokens
4. **Customize your workload**: Add your specific features
5. **Deploy**: Push to GitHub to trigger automatic deployment

## 🎉 Success!

Your Fabric workload is now ready for development! The setup has created:

- ✅ A fully configured React + C# workload
- ✅ Multi-environment CI/CD pipelines
- ✅ Azure infrastructure
- ✅ Development environment
- ✅ Production-ready configuration

Start building your amazing Fabric workload! 🚀
