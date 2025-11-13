# 🎯 PBI.tips Workload Template Usage Guide

## Overview
This template repository provides everything you need to build production-ready Microsoft Fabric workloads using modern development practices and the patterns used by PBI.tips.

## 🚀 Getting Started with Your New Workload

### 1. Use This Template
1. Click **"Use this template"** on GitHub
2. Create your new repository (e.g., `yourorg-workload-name`)
3. Clone your new repository

### 2. Customize for Your Workload
Replace all instances of "pbitips-workload" with your workload name:

```bash
# Find and replace in your code editor:
# From: pbitips-workload
# To: yourorg-workload-name

# Key files to update:
# - package.json (name, description)
# - swa-cli.config.json (configuration name)
# - .devcontainer/* (references in setup scripts)
# - API project files (namespaces, assembly names)
```

### 3. Configure Your Workload
Update these core configuration files:

#### `Workload/package.json`
```json
{
  "name": "yourorg-workload-name",
  "description": "Your Workload Description",
  "author": "Your Organization",
  // ... rest of config
}
```

#### `Workload/api/PbiTips.Workload.Template.csproj`
```xml
<PropertyGroup>
  <AssemblyName>YourOrg.Workload.Name.Api</AssemblyName>
  <RootNamespace>YourOrg.Workload.Name.Api</RootNamespace>
  <UserSecretsId>yourorg-workload-name</UserSecretsId>
</PropertyGroup>
```

#### `Workload/swa-cli.config.json`
```json
{
  "configurations": {
    "yourorg-workload-name": {
      // ... configuration
    }
  }
}
```

### 4. Set Up Authentication
Configure your Azure AD application:

1. **Create Azure AD App Registration**
2. **Set Redirect URIs** for your Static Web App
3. **Configure API Permissions** for Microsoft Fabric
4. **Update Environment Variables**

#### GitHub Secrets (for Codespaces)
Add these secrets to your repository:
```
AZURE_CLIENT_ID=your-aad-app-id
AZURE_CLIENT_SECRET=your-aad-app-secret
AZURE_TENANT_ID=your-tenant-id
FABRIC_CAPACITY_ID=your-capacity-id
WORKLOAD_NAME=YourOrg.YourWorkloadName
```

### 5. Customize Your Workload Logic

#### Frontend Customization
- Update `Workload/app/` with your React components
- Modify `Workload/app/items/` for your custom Fabric items
- Customize `Workload/app/assets/` with your branding

#### Backend Customization
- Add your business logic in `Workload/api/Services/`
- Create your data models in `Workload/api/Models/`
- Add custom endpoints in `Workload/api/Controllers/`

### 6. Update Fabric Manifest
Customize your Fabric workload manifest:

#### `Workload/Manifest/WorkloadManifest.xml`
```xml
<WorkloadManifest>
  <Name>YourWorkloadName</Name>
  <Publisher>YourOrganization</Publisher>
  <Version>1.0.0</Version>
  <!-- ... customize for your workload -->
</WorkloadManifest>
```

## 🛠️ Development Workflow

### Local Development Setup
```bash
# 1. Open in dev container (GitHub Codespaces or VS Code)
# 2. Environment will auto-configure with secrets
# 3. Start development

# Full-stack development (from Workload/ directory)
swa start --config yourorg-workload-name

# Or individual services
cd app && npm run start    # Frontend (port 60006) - from app/ directory
cd api && func start       # Backend (port 7071) - from api/ directory
```

### Adding New Features

#### 1. New Frontend Component
```typescript
// Workload/app/components/YourComponent.tsx
import { FC } from 'react';
import { Button } from '@fluentui/react-components';

export const YourComponent: FC = () => {
  return <Button>Your Custom Component</Button>;
};
```

#### 2. New API Endpoint
```csharp
// Workload/api/Controllers/YourController.cs
[Route("api/your-endpoint")]
public class YourController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetYourData()
    {
        // Your business logic
        return Ok();
    }
}
```

#### 3. New Fabric Item
```typescript
// Workload/app/items/YourItem/YourItemEditor.tsx
// Follow the established pattern for Fabric items
```

### Testing Your Workload
```bash
# Run tests
cd app && npm test                   # Frontend tests (from app/ directory)
cd api && dotnet test               # Backend tests (from api/ directory)

# Build for production
cd app && npm run build:prod        # Frontend build (from app/ directory)
cd api && dotnet publish -c Release # Backend build (from api/ directory)
```

## 🚀 Deployment

### Azure Static Web Apps Deployment

#### 1. GitHub Actions (Recommended)
The template includes GitHub Actions workflow for automatic deployment:
- Push to `main` branch triggers deployment
- Preview deployments for pull requests
- Environment-specific configurations

#### 2. Azure CLI Deployment
```bash
# Create Static Web App
az staticwebapp create \
  --name yourorg-workload-name \
  --resource-group your-rg \
  --source https://github.com/youraccount/yourorg-workload-name \
  --location "Central US" \
  --branch main \
  --app-location "Workload" \
  --api-location "Workload/api" \
  --output-location "dist"
```

### Environment Configuration
Set these application settings in Azure:
```
AZURE_CLIENT_ID=your-production-app-id
AZURE_CLIENT_SECRET=your-production-app-secret
AZURE_TENANT_ID=your-tenant-id
FABRIC_CAPACITY_ID=your-production-capacity-id
WORKLOAD_NAME=YourOrg.YourWorkloadName
```

## 📚 Template Architecture

### Frontend Stack
- **Vite** - Fast build tool and dev server
- **React 18** - Modern React with hooks
- **TypeScript** - Type safety and better DX
- **Fluent UI v9** - Microsoft's design system
- **React Router** - Client-side routing

### Backend Stack
- **.NET 8** - Latest .NET with improved performance
- **Azure Functions (Isolated)** - Serverless compute
- **Azure Tables** - NoSQL data storage
- **Azure Blobs** - File and configuration storage
- **Application Insights** - Monitoring and telemetry

### Development Tools
- **GitHub Codespaces** - Cloud development environment
- **Dev Containers** - Consistent local development
- **Azure SWA CLI** - Local full-stack development
- **GitHub Actions** - CI/CD automation

## 🎯 Best Practices Included

### Security
✅ **Azure Static Web Apps Authentication**  
✅ **Secure API endpoints with user context**  
✅ **Environment-based configuration**  
✅ **Secrets management with GitHub/Azure**  

### Performance
✅ **Vite for fast development and builds**  
✅ **Tree shaking and code splitting**  
✅ **CDN distribution via Static Web Apps**  
✅ **Efficient .NET isolated Functions**  

### Maintainability
✅ **TypeScript for type safety**  
✅ **Layered architecture with clear separation**  
✅ **Comprehensive error handling**  
✅ **Structured logging and monitoring**  

### Developer Experience
✅ **Hot reload for frontend and backend**  
✅ **GitHub Codespaces ready**  
✅ **Automated environment setup**  
✅ **Comprehensive documentation**  

## 🔍 Troubleshooting

### Common Issues

#### Authentication Not Working
- Verify Azure AD app registration
- Check redirect URIs match your Static Web App URL
- Ensure secrets are configured correctly

#### API Calls Failing
- Check CORS configuration in Functions
- Verify API routes match frontend calls
- Review Application Insights logs

#### Build Failures
- **Dev Container**: Use GitHub Codespaces or VS Code Remote Containers for automatic .NET 8 SDK setup
- **Local Development**: Ensure .NET 8 SDK is installed (`dotnet --version` should show 8.x)
- Check package.json dependencies
- Verify TypeScript compilation

#### .NET SDK Not Found
- **Recommended**: Use the dev container which automatically configures .NET 8 SDK
- **Manual Install**: Download .NET 8 SDK from https://dot.net/download
- **GitHub Codespaces**: .NET SDK is pre-configured in the dev container

### Getting Help
1. **Check the logs** in Application Insights
2. **Review the documentation** in this repository
3. **Search existing issues** on GitHub
4. **Create a new issue** with detailed information

## 🎉 You're Ready!

Your PBI.tips Workload Template is now ready for development. This template provides:

✅ **Production-ready foundation**  
✅ **Modern development stack**  
✅ **Comprehensive tooling**  
✅ **Security best practices**  
✅ **Deployment automation**  
✅ **Monitoring and observability**  

Start building your amazing Microsoft Fabric workload! 🚀