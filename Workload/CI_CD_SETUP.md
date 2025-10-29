# CI/CD Setup Guide for Fabric Workload Template

This guide explains how to set up Continuous Integration/Continuous Deployment (CI/CD) for your Fabric workload using Azure Static Web Apps.

## What is CI/CD?

**CI/CD** stands for **Continuous Integration/Continuous Deployment**:

- **Continuous Integration (CI)**: Automatically builds, tests, and validates code when changes are pushed to the repository
- **Continuous Deployment (CD)**: Automatically deploys the validated code to different environments (dev, test, prod)

## Why Static Web Apps (SWA)?

**Azure Static Web Apps** are perfect for Fabric workloads because:

1. **Frontend Hosting**: Hosts your React frontend with global CDN distribution
2. **API Integration**: Can host your Azure Functions API alongside the frontend
3. **Authentication**: Built-in Azure AD integration for Fabric workloads
4. **Custom Domains**: Easy to configure custom domains for production
5. **Environment Management**: Separate apps for dev/test/prod environments
6. **Cost Effective**: Pay only for what you use, scales automatically
7. **GitHub Integration**: Automatic deployments from GitHub pushes

## Architecture Overview

```
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│   GitHub Repo   │───▶│  Azure DevOps    │───▶│ Azure Static    │
│                 │    │  / GitHub Actions│    │ Web Apps        │
│  - Main Branch  │    │                  │    │                 │
│  - Dev Branch   │    │  - Build Frontend│    │ - Dev Environment│
│  - PR Branches  │    │  - Build API     │    │ - Test Environment│
└─────────────────┘    │  - Run Tests     │    │ - Prod Environment│
                       └──────────────────┘    └─────────────────┘
```

## Quick Setup

### 1. Automated Setup (Recommended)

Use the provided PowerShell script to set up everything automatically:

```powershell
# Navigate to your workload directory
cd your-workload

# Run the setup script
.\scripts\setup-cicd.ps1 -WorkloadName "MyDataApp" -ResourceGroupName "MyDataApp-01" -SubscriptionId "your-subscription-id"
```

### 2. Manual Setup

If you prefer to set up manually, follow these steps:

#### Step 1: Create Azure Resources

1. **Create Resource Group**:
   ```bash
   az group create --name "MyDataApp-01" --location "East US"
   ```

2. **Create Static Web Apps**:
   ```bash
   # Development
   az staticwebapp create --name "MyDataApp-swa-dev" --resource-group "MyDataApp-01" --location "East US" --source "https://github.com/your-org/your-repo" --branch "develop" --app-location "app" --api-location "api" --output-location "dist"

   # Production
   az staticwebapp create --name "MyDataApp-swa-prod" --resource-group "MyDataApp-01" --location "East US" --source "https://github.com/your-org/your-repo" --branch "main" --app-location "app" --api-location "api" --output-location "dist"
   ```

#### Step 2: Configure GitHub Secrets

Add these secrets to your GitHub repository:

- `AZURE_STATIC_WEB_APPS_DEV_TOKEN`: Development deployment token
- `AZURE_STATIC_WEB_APPS_PROD_TOKEN`: Production deployment token

To get the tokens:
```bash
az staticwebapp secrets list --name "MyDataApp-swa-dev" --resource-group "MyDataApp-01" --query "properties.apiKey" -o tsv
```

#### Step 3: Configure Azure DevOps (if using)

1. Create a service connection to Azure
2. Update the `azure-pipelines.yml` with your service connection name
3. Set up the pipeline in Azure DevOps

## Configuration Files

### 1. swa-cli.config.json

Configuration for local development and deployment:

```json
{
  "$schema": "https://aka.ms/azure/static-web-apps-cli/schema",
  "configurations": {
    "MyDataApp": {
      "appLocation": "app",
      "apiLocation": "api",
      "outputLocation": "dist",
      "apiLanguage": "dotnetisolated",
      "apiVersion": "8.0",
      "appBuildCommand": "npm run build:prod",
      "apiBuildCommand": "dotnet publish -c Release",
      "run": "npm run start:devServer",
      "appDevserverUrl": "http://localhost:60006",
      "apiDevserverUrl": "http://localhost:7071",
      "appName": "MyDataApp-swa-dev",
      "resourceGroup": "MyDataApp-01"
    }
  }
}
```

### 2. azure-pipelines.yml

Azure DevOps pipeline configuration with multiple environments:

- **Development**: Deploys from `develop` branch
- **Production**: Deploys from `main` branch
- **PR Preview**: Deploys from pull requests

### 3. .github/workflows/deploy.yml

GitHub Actions workflow with the same environment structure.

## Environment Configuration

### Development Environment (.env.dev)
```bash
NODE_ENV=development
WORKLOAD_NAME=MyDataApp
BACKEND_URL=https://my-data-app-swa-dev.azurestaticapps.net
# ... other environment variables
```

### Production Environment (.env.prod)
```bash
NODE_ENV=production
WORKLOAD_NAME=MyDataApp
BACKEND_URL=https://my-data-app-swa-prod.azurestaticapps.net
# ... other environment variables
```

## Deployment Process

### 1. Development Deployment

- **Trigger**: Push to `develop` branch
- **Environment**: Development
- **URL**: `https://my-data-app-swa-dev.azurestaticapps.net`

### 2. Production Deployment

- **Trigger**: Push to `main` branch
- **Environment**: Production
- **URL**: `https://my-data-app-swa-prod.azurestaticapps.net`

### 3. PR Preview Deployment

- **Trigger**: Create/update pull request
- **Environment**: Preview
- **URL**: Generated preview URL

## Customization

### 1. Update Workload Name

Update these files when changing workload name:
- `swa-cli.config.json`
- `azure-pipelines.yml`
- `.github/workflows/deploy.yml`
- Environment files (`.env.*`)

### 2. Add New Environments

To add a staging environment:

1. Create a new Static Web App:
   ```bash
   az staticwebapp create --name "MyDataApp-swa-staging" --resource-group "MyDataApp-01" --location "East US" --source "https://github.com/your-org/your-repo" --branch "staging" --app-location "app" --api-location "api" --output-location "dist"
   ```

2. Add staging stage to `azure-pipelines.yml`:
   ```yaml
   - stage: DeployStaging
     displayName: 'Deploy to Staging'
     dependsOn: Build
     condition: and(succeeded(), eq(variables['Build.SourceBranch'], 'refs/heads/staging'))
     # ... rest of staging configuration
   ```

3. Add staging environment to GitHub Actions workflow

### 3. Custom Build Commands

Update build commands in configuration files:

```json
{
  "appBuildCommand": "npm run build:custom",
  "apiBuildCommand": "dotnet publish -c Release -o ./publish"
}
```

## Monitoring and Troubleshooting

### 1. Check Deployment Status

**Azure Portal**:
1. Go to your Static Web App
2. Check "Deployment history" for deployment status
3. View logs for any errors

**GitHub Actions**:
1. Go to your repository
2. Click "Actions" tab
3. View workflow runs and logs

**Azure DevOps**:
1. Go to your project
2. Click "Pipelines"
3. View pipeline runs and logs

### 2. Common Issues

**Build Failures**:
- Check Node.js and .NET versions
- Verify all dependencies are installed
- Check for TypeScript compilation errors

**Deployment Failures**:
- Verify deployment tokens are correct
- Check Azure resource permissions
- Ensure all required environment variables are set

**Runtime Errors**:
- Check application logs in Azure Portal
- Verify environment variables are configured
- Test API endpoints independently

### 3. Local Testing

Test your deployment locally:

```bash
# Install SWA CLI
npm install -g @azure/static-web-apps-cli

# Start local development
swa start

# Deploy to development
swa deploy --deployment-token your-dev-token
```

## Security Considerations

### 1. Environment Variables

- Never commit secrets to source control
- Use Azure Key Vault for production secrets
- Rotate deployment tokens regularly

### 2. Access Control

- Use Azure AD for authentication
- Configure proper CORS settings
- Implement proper API authorization

### 3. Network Security

- Use HTTPS for all communications
- Configure proper firewall rules
- Monitor for suspicious activity

## Cost Optimization

### 1. Resource Sizing

- Use appropriate SKU for your needs
- Monitor usage and scale accordingly
- Consider reserved capacity for production

### 2. Environment Management

- Use development environments for testing
- Shut down unused environments
- Implement auto-shutdown for non-production

## Best Practices

### 1. Branch Strategy

- Use `main` for production
- Use `develop` for development
- Use feature branches for new features
- Use pull requests for code review

### 2. Deployment Strategy

- Test in development before production
- Use blue-green deployments for critical updates
- Implement rollback procedures
- Monitor deployment health

### 3. Code Quality

- Run tests before deployment
- Use linting and formatting
- Implement code review processes
- Monitor code coverage

## Support

- **Azure Static Web Apps Documentation**: https://docs.microsoft.com/en-us/azure/static-web-apps/
- **GitHub Actions Documentation**: https://docs.github.com/en/actions
- **Azure DevOps Documentation**: https://docs.microsoft.com/en-us/azure/devops/
- **Fabric Workload Development**: https://learn.microsoft.com/en-us/fabric/extensibility-toolkit/

## Troubleshooting Commands

```bash
# Check Azure CLI login
az account show

# List Static Web Apps
az staticwebapp list --resource-group "MyDataApp-01"

# Get deployment token
az staticwebapp secrets list --name "MyDataApp-swa-dev" --resource-group "MyDataApp-01"

# Check deployment status
az staticwebapp show --name "MyDataApp-swa-dev" --resource-group "MyDataApp-01"

# View logs
az staticwebapp logs --name "MyDataApp-swa-dev" --resource-group "MyDataApp-01"
```
