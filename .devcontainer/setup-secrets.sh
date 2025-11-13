#!/bin/bash

# Setup script for GitHub Codespaces secrets
echo "🔐 Setting up local.settings.json from GitHub secrets..."

# Create local.settings.json if it doesn't exist
LOCAL_SETTINGS_FILE="/workspaces/fabric-extensibility-toolkit/Workload/local.settings.json"

# Check if we're in Codespaces and have the required secrets
if [ -n "$AZURE_CLIENT_ID" ] && [ -n "$AZURE_CLIENT_SECRET" ]; then
    echo "✅ GitHub secrets detected, creating local.settings.json..."
    
    # Create the directory if it doesn't exist
    mkdir -p "$(dirname "$LOCAL_SETTINGS_FILE")"
    
    # Generate local.settings.json from secrets
    cat > "$LOCAL_SETTINGS_FILE" << EOF
{
  "IsEncrypted": false,
  "Values": {
    "FUNCTIONS_WORKER_RUNTIME": "node",
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "AZURE_CLIENT_ID": "$AZURE_CLIENT_ID",
    "AZURE_CLIENT_SECRET": "$AZURE_CLIENT_SECRET",
    "AZURE_TENANT_ID": "$AZURE_TENANT_ID",
    "FABRIC_CAPACITY_ID": "$FABRIC_CAPACITY_ID",
    "WORKLOAD_NAME": "$WORKLOAD_NAME",
    "NODE_ENV": "development"
  },
  "Host": {
    "LocalHttpPort": 7071,
    "CORS": "*"
  }
}
EOF
    
    echo "✅ local.settings.json created successfully!"
    
    # Also create .env files for the frontend
    ENV_DEV_FILE="/workspaces/fabric-extensibility-toolkit/Workload/.env.dev"
    cat > "$ENV_DEV_FILE" << EOF
REACT_APP_AZURE_CLIENT_ID=$AZURE_CLIENT_ID
REACT_APP_AZURE_TENANT_ID=$AZURE_TENANT_ID
REACT_APP_WORKLOAD_NAME=$WORKLOAD_NAME
REACT_APP_FABRIC_CAPACITY_ID=$FABRIC_CAPACITY_ID
REACT_APP_ENVIRONMENT=development
EOF
    
    echo "✅ .env.dev created successfully!"
    
else
    echo "⚠️  GitHub secrets not found. You'll need to set up local.settings.json manually."
    echo "📖 See README.md for manual setup instructions."
    
    # Create a template file if secrets aren't available
    if [ ! -f "$LOCAL_SETTINGS_FILE" ]; then
        cat > "$LOCAL_SETTINGS_FILE" << EOF
{
  "IsEncrypted": false,
  "Values": {
    "FUNCTIONS_WORKER_RUNTIME": "node",
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "AZURE_CLIENT_ID": "your-aad-app-id-here",
    "AZURE_CLIENT_SECRET": "your-aad-app-secret-here",
    "AZURE_TENANT_ID": "your-tenant-id-here",
    "FABRIC_CAPACITY_ID": "your-capacity-id-here",
    "WORKLOAD_NAME": "YourOrg.YourWorkload",
    "NODE_ENV": "development"
  },
  "Host": {
    "LocalHttpPort": 7071,
    "CORS": "*"
  }
}
EOF
        echo "📝 Created template local.settings.json - please update with your values"
    fi
fi

# Install npm dependencies
echo "📦 Installing npm dependencies..."
cd /workspaces/fabric-extensibility-toolkit/Workload
npm install

echo "🚀 Setup complete! Ready for Fabric workload development."