#!/bin/bash

# Enhanced setup script for PBI.tips Workload Template
# Supports Vite + .NET Functions + Azure Static Web Apps
echo "🚀 Setting up PBI.tips Workload Template development environment..."

# Ensure .NET SDK is available in PATH
if [ -d "$HOME/.dotnet" ]; then
    export PATH="$HOME/.dotnet:$PATH"
    export DOTNET_ROOT="$HOME/.dotnet"
    echo "📦 Added .NET SDK to PATH"
elif ! command -v dotnet &> /dev/null || ! dotnet --version &> /dev/null; then
    echo "📦 Installing .NET 8 SDK..."
    curl -fsSL https://dot.net/v1/dotnet-install.sh | bash -s -- --channel 8.0
    export PATH="$HOME/.dotnet:$PATH"
    export DOTNET_ROOT="$HOME/.dotnet"
    echo "✅ .NET 8 SDK installed and added to PATH"
fi

# Install mono-complete for NuGet.exe support (required by nuget-bin package)
if ! command -v mono &> /dev/null; then
    echo "📦 Installing mono-complete for NuGet packaging support..."
    sudo apt-get update && sudo apt-get install -y mono-complete
    echo "✅ Mono installed successfully"
else
    echo "✅ Mono already installed"
fi

# Create local.settings.json for .NET Functions if we have GitHub secrets
WORKLOAD_DIR="/workspaces/fabric-extensibility-toolkit/Workload"
API_DIR="$WORKLOAD_DIR/api"
LOCAL_SETTINGS_FILE="$API_DIR/local.settings.json"

# Check if we're in Codespaces and have the required secrets
if [ -n "$AZURE_CLIENT_ID" ] && [ -n "$AZURE_CLIENT_SECRET" ]; then
    echo "✅ GitHub secrets detected, creating configuration files..."
    
    # Create API directory if it doesn't exist
    mkdir -p "$API_DIR"
    
    # Generate local.settings.json for .NET Functions
    cat > "$LOCAL_SETTINGS_FILE" << EOF
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "AZURE_CLIENT_ID": "$AZURE_CLIENT_ID",
    "AZURE_CLIENT_SECRET": "$AZURE_CLIENT_SECRET",
    "AZURE_TENANT_ID": "$AZURE_TENANT_ID",
    "FABRIC_CAPACITY_ID": "$FABRIC_CAPACITY_ID",
    "WORKLOAD_NAME": "$WORKLOAD_NAME"
  },
  "Host": {
    "LocalHttpPort": 7071,
    "CORS": "*",
    "CORSCredentials": false
  }
}
EOF
    
    echo "✅ local.settings.json created for .NET Functions!"
    
    # Create .env files for Vite frontend
    ENV_DEV_FILE="$WORKLOAD_DIR/.env.dev"
    cat > "$ENV_DEV_FILE" << EOF
# Vite Frontend Environment Variables
VITE_AZURE_CLIENT_ID=$AZURE_CLIENT_ID
VITE_AZURE_TENANT_ID=$AZURE_TENANT_ID
VITE_WORKLOAD_NAME=$WORKLOAD_NAME
VITE_FABRIC_CAPACITY_ID=$FABRIC_CAPACITY_ID
VITE_ENVIRONMENT=development
VITE_API_BASE_URL=http://localhost:7071/api
VITE_FABRIC_GATEWAY_URL=http://127.0.0.1:60006
EOF
    
    ENV_TEST_FILE="$WORKLOAD_DIR/.env.test"
    cat > "$ENV_TEST_FILE" << EOF
# Vite Frontend Environment Variables - Test
VITE_AZURE_CLIENT_ID=$AZURE_CLIENT_ID
VITE_AZURE_TENANT_ID=$AZURE_TENANT_ID
VITE_WORKLOAD_NAME=$WORKLOAD_NAME
VITE_FABRIC_CAPACITY_ID=$FABRIC_CAPACITY_ID
VITE_ENVIRONMENT=test
VITE_API_BASE_URL=https://your-test-swa.azurestaticapps.net/api
EOF

    ENV_PROD_FILE="$WORKLOAD_DIR/.env.prod"
    cat > "$ENV_PROD_FILE" << EOF
# Vite Frontend Environment Variables - Production
VITE_AZURE_CLIENT_ID=$AZURE_CLIENT_ID
VITE_AZURE_TENANT_ID=$AZURE_TENANT_ID
VITE_WORKLOAD_NAME=$WORKLOAD_NAME
VITE_FABRIC_CAPACITY_ID=$FABRIC_CAPACITY_ID
VITE_ENVIRONMENT=production
VITE_API_BASE_URL=https://your-prod-swa.azurestaticapps.net/api
EOF
    
    echo "✅ Environment files created for all stages!"
    
else
    echo "⚠️  GitHub secrets not found. Creating template files..."
    
    # Create template files if secrets aren't available
    mkdir -p "$API_DIR"
    
    if [ ! -f "$LOCAL_SETTINGS_FILE" ]; then
        cat > "$LOCAL_SETTINGS_FILE" << EOF
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "AZURE_CLIENT_ID": "your-aad-app-id-here",
    "AZURE_CLIENT_SECRET": "your-aad-app-secret-here",
    "AZURE_TENANT_ID": "your-tenant-id-here",
    "FABRIC_CAPACITY_ID": "your-capacity-id-here",
    "WORKLOAD_NAME": "YourOrg.YourWorkload"
  },
  "Host": {
    "LocalHttpPort": 7071,
    "CORS": "*",
    "CORSCredentials": false
  }
}
EOF
        echo "📝 Created template local.settings.json - please update with your values"
    fi
    
    # Create template .env.dev if it doesn't exist
    if [ ! -f "$WORKLOAD_DIR/.env.dev" ]; then
        cat > "$WORKLOAD_DIR/.env.dev" << EOF
# Vite Frontend Environment Variables
VITE_AZURE_CLIENT_ID=your-aad-app-id-here
VITE_AZURE_TENANT_ID=your-tenant-id-here
VITE_WORKLOAD_NAME=YourOrg.YourWorkload
VITE_FABRIC_CAPACITY_ID=your-capacity-id-here
VITE_ENVIRONMENT=development
VITE_API_BASE_URL=http://localhost:7071/api
VITE_FABRIC_GATEWAY_URL=http://127.0.0.1:60006
EOF
        echo "📝 Created template .env.dev - please update with your values"
    fi
fi

# Install npm dependencies
echo "📦 Installing npm dependencies..."
cd "$WORKLOAD_DIR"
npm install

# Install TanStack Router if not already in package.json dependencies
if ! grep -q "@tanstack/react-router" package.json; then
    echo "📦 Installing TanStack Router..."
    npm install @tanstack/react-router @tanstack/router-devtools @tanstack/router-vite-plugin
fi

# Install Azure Functions Core Tools if not already installed
if ! command -v func &> /dev/null; then
    echo "📦 Installing Azure Functions Core Tools..."
    npm install -g azure-functions-core-tools@4 --unsafe-perm true
fi

# Install Azure Static Web Apps CLI if not already installed  
if ! command -v swa &> /dev/null; then
    echo "📦 Installing Azure Static Web Apps CLI..."
    npm install -g @azure/static-web-apps-cli
fi

# Download DevGateway if not already present
DEVGATEWAY_DIR="/workspaces/fabric-extensibility-toolkit/tools/DevGateway"
if [ ! -d "$DEVGATEWAY_DIR" ] || [ ! -f "$DEVGATEWAY_DIR/Microsoft.Fabric.Workload.DevGateway.dll" ]; then
    echo "📥 Downloading DevGateway..."
    pwsh -Command "& /workspaces/fabric-extensibility-toolkit/scripts/Setup/DownloadDevGateway.ps1 -Force \$true"
fi

# Create swa-cli.config.json for Azure Static Web Apps development
SWA_CONFIG_FILE="$WORKLOAD_DIR/swa-cli.config.json"
if [ ! -f "$SWA_CONFIG_FILE" ]; then
    cat > "$SWA_CONFIG_FILE" << EOF
{
  "\$schema": "https://aka.ms/azure/static-web-apps-cli/schema",
  "configurations": {
    "pbitips-workload": {
      "appLocation": ".",
      "apiLocation": "api",
      "outputLocation": "dist",
      "appBuildCommand": "npm run build:prod",
      "apiBuildCommand": "dotnet publish -c Release -o bin/Release/net8.0",
      "run": "npm run start:vite",
      "appDevserverUrl": "http://127.0.0.1:60006"
    }
  }
}
EOF
    echo "✅ Created swa-cli.config.json for Azure Static Web Apps!"
fi

# Add .NET to user profile to persist across sessions
if [ -d "$HOME/.dotnet" ]; then
    if ! grep -q "/.dotnet" ~/.bashrc 2>/dev/null; then
        echo 'export PATH="$HOME/.dotnet:$PATH"' >> ~/.bashrc
        echo 'export DOTNET_ROOT="$HOME/.dotnet"' >> ~/.bashrc
        echo "📝 Added .NET SDK to ~/.bashrc for persistent PATH"
    fi
fi

echo ""
echo "🎉 Setup complete! Your PBI.tips Workload Template is ready for:"
echo "   ✅ Vite + React frontend development"
echo "   ✅ .NET isolated Azure Functions backend"
echo "   ✅ Azure Static Web Apps deployment"
echo "   ✅ Microsoft Fabric workload development"
echo ""
echo "🚀 Quick start commands:"
echo "   Frontend: npm run start"
echo "   Backend:  func start (from api/ directory)"
echo "   Full SWA: swa start --config pbitips-workload"
echo ""