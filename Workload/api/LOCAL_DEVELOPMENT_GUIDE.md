# Local Development Quick Start

## No Azure Deployment Needed Initially!

You can develop and test everything locally without deploying to Azure first. Here's what we've accomplished:

## ✅ What's Already Working Locally

### 1. **Complete .NET API Implementation**
- All User domain endpoints implemented
- Feature flag system in place
- Full business logic migrated from TypeScript
- Local storage emulation ready

### 2. **Local Development Stack**
```bash
# What's installed and working:
✅ .NET 8.0 SDK (version 9.0.304)
✅ Azure Functions Core Tools (version 4.2.2)  
✅ Azurite (local storage emulator)
✅ All NuGet packages restored
✅ Project builds successfully
```

### 3. **API Endpoints Ready for Testing**
```
✅ GET    http://localhost:7071/api/users
✅ GET    http://localhost:7071/api/users/{userId}
✅ POST   http://localhost:7071/api/users
✅ PUT    http://localhost:7071/api/users/{userId}/role
✅ PUT    http://localhost:7071/api/users/{userId}/trial
✅ POST   http://localhost:7071/api/users/login-event
```

## 🚀 How to Start Local Development

### Option 1: Manual Start (Recommended)
```bash
# Terminal 1: Start storage emulator
azurite --silent --location /tmp/azurite

# Terminal 2: Start API
cd api-dotnet
dotnet build
func start
```

### Option 2: VS Code Integration
1. Open VS Code in the `api-dotnet` folder
2. Install "Azure Functions" extension
3. Press F5 to debug/run locally
4. Functions will start automatically with debugging

### Option 3: Docker (Advanced)
```bash
# If you prefer containerized development
docker run -p 10000:10000 -p 10001:10001 -p 10002:10002 \
  mcr.microsoft.com/azure-storage/azurite
```

## 📊 Comparing with Intelexos

Based on your question about Intelexos deployment, here's what you can check:

### Look for These Files in Intelexos Project:
```
✅ .github/workflows/               # GitHub Actions for CI/CD
✅ azuredeploy.json                 # ARM template for Azure resources  
✅ azure-pipelines.yml             # Azure DevOps pipeline
✅ func-app-settings.json          # Production configuration
✅ README.md deployment section    # Manual deployment steps
```

### Common Deployment Patterns:
1. **GitHub Actions** - Automated deployment on push
2. **Azure DevOps** - Enterprise CI/CD pipeline
3. **Manual Deployment** - Using Azure CLI or func tools
4. **ARM Templates** - Infrastructure as Code

## 🔍 Where to Look for Intelexos Deployment Info

### 1. **GitHub Repository**
```bash
# Check these locations in Intelexos repo:
├── .github/workflows/deploy.yml        # GitHub Actions
├── docs/deployment.md               # Documentation
├── scripts/deploy.sh               # Deployment scripts  
├── azure/                          # Azure configuration
└── README.md                       # Getting started guide
```

### 2. **Azure Portal (if you have access)**
- Function App resource
- Application Settings
- Deployment Center configuration
- Monitoring and logs

### 3. **Documentation Files**
Look for files named:
- `DEPLOYMENT.md`
- `GETTING_STARTED.md`  
- `INFRASTRUCTURE.md`
- `CI_CD.md`

## 🎯 Next Steps (In Order)

### Phase 1: Local Development (This Week)
```bash
1. ✅ API implementation complete
2. 🔄 Test locally with curl/Postman
3. 🔄 Create sample data for testing
4. 🔄 Test all endpoints thoroughly
5. 🔄 Document any issues found
```

### Phase 2: Integration (Next Week)  
```bash
1. Connect to real Azure Storage (optional)
2. Test with actual PowerBI.tips data
3. Update frontend to call new API
4. Implement feature flag switching
```

### Phase 3: Deployment (When Ready)
```bash
1. Set up Azure Function App
2. Configure production settings
3. Deploy API to Azure
4. Update DNS/routing
5. Monitor and validate
```

## 🔧 Troubleshooting Local Issues

### If Functions Won't Start:
```bash
# Check current directory
pwd  # Should be in api-dotnet folder

# Verify files exist
ls -la host.json local.settings.json

# Manual build and start
dotnet clean
dotnet build
func start --verbose
```

### If Storage Issues:
```bash
# Start fresh Azurite instance
pkill azurite
azurite --silent --location /tmp/azurite &

# Test storage connectivity
curl http://127.0.0.1:10002/devstoreaccount1/tables
```

### If Port Conflicts:
```bash
# Use different port
func start --port 7072

# Check what's using port 7071
lsof -i :7071
```

## 📝 Testing Without Real Data

You can test all functionality locally with mock data:

### Create Test User:
```bash
curl -X POST "http://localhost:7071/api/users" \
  -H "Content-Type: application/json" \
  -d '{
    "environment": "local",
    "identityProvider": "github",
    "idpUserName": "testuser",
    "idpUserId": "123456",
    "tenantId": "test-tenant",
    "userName": "Test User", 
    "firstName": "Test",
    "lastName": "User"
  }'
```

### Test Get Users:
```bash
curl "http://localhost:7071/api/users"
```

## 💡 Key Benefits of Local Development

1. **No Azure Costs** - Develop for free locally
2. **Fast Iteration** - Instant feedback on changes  
3. **Easy Debugging** - Full IDE debugging support
4. **Isolated Testing** - No impact on production
5. **Offline Development** - Work without internet

## 🎉 Bottom Line

**You DO NOT need Azure deployment initially!** Everything can be developed, tested, and validated locally first. This gives you confidence before deploying to Azure.

The deployment to Azure is only needed when you want to:
- Share the API with others
- Test with production data  
- Replace the existing TypeScript API
- Scale beyond local development

Focus on getting the local development working perfectly first, then deployment becomes much easier and lower risk.