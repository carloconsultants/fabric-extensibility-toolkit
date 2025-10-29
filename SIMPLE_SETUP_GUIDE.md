# Simple Fabric Workload Setup Guide

**The easiest way to set up a new Fabric workload - just 3 steps!**

## 🚀 Ultra-Simple Setup

### Step 1: Copy Template
```bash
cp -r fabric-extensibility-toolkit/Workload my-workload/
cd my-workload/
```

### Step 2: Edit One Simple Config File
Edit `workload-config.json` with just the essentials:

```json
{
  "workload": {
    "name": "MyDataApp",
    "displayName": "My Data Application", 
    "description": "A powerful data management application",
    "version": "1.0.0",
    "organization": "MyOrg",
    "itemNames": "DataApp,DataViewer"
  },
  "azure": {
    "subscriptionId": "12345678-1234-1234-1234-123456789012",
    "tenantId": "87654321-4321-4321-4321-210987654321",
    "resourceGroup": "MyDataApp-01",
    "location": "East US",
    "storageAccountName": "mydataappstorage",
    "keyVaultEndpoint": "https://mydataapp-keyvault.vault.azure.net/"
  },
  "environments": {
    "dev": {
      "name": "MyDataApp-swa-dev",
      "branch": "develop",
      "clientId": "33333333-3333-3333-3333-333333333333",
      "clientSecret": "your-dev-secret"
    },
    "prod": {
      "name": "MyDataApp-swa-prod", 
      "branch": "main",
      "clientId": "99999999-9999-9999-9999-999999999999",
      "clientSecret": "your-prod-secret"
    }
  }
}
```

### Step 3: Run Setup
```bash
# Windows
./setup-workload-simple.ps1

# Linux/Mac  
./setup-workload.sh
```

**That's it!** 🎉 Everything else is auto-generated!

## ✨ What Gets Auto-Generated

The setup script automatically creates:

- ✅ **Environment Files** (`.env.dev`, `.env.prod`) with all variables
- ✅ **URLs** for each environment (`https://myapp-swa-dev.azurestaticapps.net`)
- ✅ **AAD Configuration** (frontend/backend app IDs, audiences, redirect URIs)
- ✅ **GitHub Repository** detection from git remote
- ✅ **CI/CD Pipelines** (GitHub Actions, Azure DevOps)
- ✅ **All File Updates** (package.json, C# files, TypeScript files)
- ✅ **Documentation** (README.md with your specific info)

## 🔧 Configuration Reference

### Required Fields Only

| Field | Description | Example |
|-------|-------------|---------|
| `workload.name` | Your workload name (used for namespacing) | `"MyDataApp"` |
| `workload.displayName` | Human-readable name | `"My Data Application"` |
| `workload.description` | App description | `"A powerful data app"` |
| `workload.version` | Version number | `"1.0.0"` |
| `workload.organization` | Organization prefix | `"MyOrg"` |
| `workload.itemNames` | Comma-separated item types | `"DataApp,DataViewer"` |
| `azure.subscriptionId` | Azure subscription ID | `"12345678-..."` |
| `azure.tenantId` | Azure AD tenant ID | `"87654321-..."` |
| `azure.resourceGroup` | Resource group name | `"MyDataApp-01"` |
| `azure.location` | Azure region | `"East US"` |
| `azure.storageAccountName` | Storage account name | `"mydataappstorage"` |
| `azure.keyVaultEndpoint` | Key Vault URL | `"https://myapp-kv.vault.azure.net/"` |
| `environments.*.name` | Static Web App name | `"MyDataApp-swa-dev"` |
| `environments.*.branch` | Git branch | `"develop"` |
| `environments.*.clientId` | Azure AD client ID | `"33333333-..."` |
| `environments.*.clientSecret` | Azure AD client secret | `"your-secret"` |

### Auto-Generated Fields

These are automatically created by the setup script:

- ✅ **Environment URLs** - Generated from Static Web App names
- ✅ **AAD Frontend/Backend App IDs** - Uses client ID for both
- ✅ **AAD Audience** - Generated as `api://{clientId}`
- ✅ **AAD Redirect URI** - Generated from environment URL
- ✅ **GitHub Repository** - Detected from git remote
- ✅ **Table Names** - Uses standard naming convention
- ✅ **Blob Container Names** - Uses standard naming convention
- ✅ **CI/CD Configuration** - Uses standard Node.js/.NET versions

## 🎯 Benefits of Simplified Config

### ❌ **Removed Unnecessary Items**

1. **Author & Email** - Not needed for functionality
2. **Storage Connection String** - Security risk, use managed identity
3. **Frontend/Backend App IDs** - Redundant with client ID
4. **Audience & Redirect URI** - Auto-generated from other values
5. **GitHub Repository** - Auto-detected from git
6. **Table/Container Names** - Use standard conventions
7. **CI/CD Versions** - Use standard versions

### ✅ **What You Get**

- **90% less configuration** - Only essential values needed
- **Auto-generated URLs** - No manual URL construction
- **Security best practices** - No secrets in config files
- **Git integration** - Auto-detects repository info
- **Standard conventions** - Consistent naming and structure
- **Zero redundancy** - No duplicate or calculated values

## 🚀 Quick Commands

### Basic Setup
```bash
# 1. Copy template
cp -r fabric-extensibility-toolkit/Workload my-workload/
cd my-workload/

# 2. Edit config (just the essentials!)
code workload-config.json

# 3. Run setup
./setup-workload-simple.ps1

# 4. Install and start
npm install
npm run start:devServer
```

### Update Configuration
```bash
# Edit config file
code workload-config.json

# Re-run setup to update everything
./setup-workload-simple.ps1
```

### Advanced Options
```bash
# Skip Azure resource creation
./setup-workload-simple.ps1 -SkipAzureResources $true

# Force setup without confirmation
./setup-workload-simple.ps1 -Force $true

# Use custom config file
./setup-workload-simple.ps1 -ConfigPath "my-config.json"
```

## 🔍 What Happens During Setup

1. **Loads your config** - Reads `workload-config.json`
2. **Auto-generates missing values** - URLs, AAD config, GitHub info
3. **Updates all files** - package.json, C# files, TypeScript files
4. **Creates environment files** - .env.dev, .env.prod with all variables
5. **Updates CI/CD** - GitHub Actions, Azure DevOps pipelines
6. **Creates documentation** - README.md with your specific info

## 🎉 Result

After running the setup, you get:

- ✅ **Fully configured workload** - Ready for development
- ✅ **Multi-environment setup** - Dev, test, prod environments
- ✅ **CI/CD pipelines** - Automatic deployments
- ✅ **Azure integration** - Storage, Key Vault, AAD
- ✅ **Development environment** - Local development ready
- ✅ **Production ready** - All configurations in place

## 🆚 Comparison: Before vs After

### Before (Complex)
```json
{
  "workload": {
    "name": "MyApp",
    "displayName": "My App",
    "description": "Description",
    "version": "1.0.0",
    "organization": "MyOrg",
    "itemNames": "Item1,Item2",
    "author": "John Doe",           // ❌ Not needed
    "email": "john@company.com"     // ❌ Not needed
  },
  "azure": {
    "subscriptionId": "123...",
    "tenantId": "456...",
    "resourceGroup": "MyApp-01",
    "location": "East US",
    "storageAccount": {
      "name": "mystorage",
      "connectionString": "secret"  // ❌ Security risk
    },
    "keyVault": {
      "endpoint": "https://kv.vault.azure.net/"
    }
  },
  "environments": {
    "dev": {
      "name": "MyApp-swa-dev",
      "branch": "develop",
      "aad": {
        "frontendAppId": "111...",  // ❌ Redundant
        "backendAppId": "222...",   // ❌ Redundant
        "clientId": "333...",
        "clientSecret": "secret",
        "audience": "api://222...", // ❌ Auto-generated
        "redirectUri": "https://..." // ❌ Auto-generated
      },
      "url": "https://..."          // ❌ Auto-generated
    }
  },
  "github": {                       // ❌ Auto-detected
    "repository": "org/repo",
    "organization": "org"
  },
  "tables": { ... },               // ❌ Standard conventions
  "blobContainers": { ... },       // ❌ Standard conventions
  "ci": { ... }                    // ❌ Standard versions
}
```

### After (Simple)
```json
{
  "workload": {
    "name": "MyApp",
    "displayName": "My App",
    "description": "Description",
    "version": "1.0.0",
    "organization": "MyOrg",
    "itemNames": "Item1,Item2"
  },
  "azure": {
    "subscriptionId": "123...",
    "tenantId": "456...",
    "resourceGroup": "MyApp-01",
    "location": "East US",
    "storageAccountName": "mystorage",
    "keyVaultEndpoint": "https://kv.vault.azure.net/"
  },
  "environments": {
    "dev": {
      "name": "MyApp-swa-dev",
      "branch": "develop",
      "clientId": "333...",
      "clientSecret": "secret"
    }
  }
}
```

**Result**: 90% less configuration, same functionality! 🎉

## 🎯 Perfect For

- ✅ **Quick prototyping** - Get started in minutes
- ✅ **Team onboarding** - Minimal configuration to learn
- ✅ **Standard workloads** - Follows best practices
- ✅ **CI/CD automation** - Perfect for automated setups
- ✅ **Security compliance** - No secrets in config files

Start building your amazing Fabric workload with minimal configuration! 🚀
