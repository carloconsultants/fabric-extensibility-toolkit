#!/bin/bash

# Setup script for creating a new workload from this template
# Usage: ./setup-new-workload.sh <workload-name> <item-names>

if [ $# -lt 2 ]; then
    echo "Usage: ./setup-new-workload.sh <workload-name> <item-names>"
    echo "Example: ./setup-new-workload.sh MyDataApp DataApp,DataViewer"
    exit 1
fi

WORKLOAD_NAME=$1
ITEM_NAMES=$2
WORKLOAD_VERSION="1.0.0"

echo "Setting up new workload: $WORKLOAD_NAME"
echo "Item names: $ITEM_NAMES"
echo "Version: $WORKLOAD_VERSION"

# Update package.json
echo "Updating package.json..."
sed -i "s/fabric-developer-sample/$WORKLOAD_NAME/g" package.json
sed -i "s/2.0.0/$WORKLOAD_VERSION/g" package.json

# Update environment files
echo "Updating environment files..."
for env_file in .env.dev .env.test .env.prod; do
    if [ -f "$env_file" ]; then
        sed -i "s/TemplateWorkload/$WORKLOAD_NAME/g" "$env_file"
        sed -i "s/TemplateItem/$ITEM_NAMES/g" "$env_file"
        sed -i "s/1.0.0/$WORKLOAD_VERSION/g" "$env_file"
    fi
done

# Update C# namespace
echo "Updating C# namespace..."
find api/ -name "*.cs" -exec sed -i "s/TemplateWorkload/$WORKLOAD_NAME/g" {} \;

# Update TypeScript files
echo "Updating TypeScript files..."
find app/ -name "*.ts" -o -name "*.tsx" | xargs sed -i "s/TemplateWorkload/$WORKLOAD_NAME/g"

# Create a simple README for the new workload
echo "Creating README for $WORKLOAD_NAME..."
cat > README.md << EOF
# $WORKLOAD_NAME

A Microsoft Fabric workload created from the template.

## Quick Start

1. Install dependencies:
   \`\`\`bash
   npm install
   \`\`\`

2. Configure environment:
   \`\`\`bash
   cp .env.dev .env.local
   # Edit .env.local with your configuration
   \`\`\`

3. Start development:
   \`\`\`bash
   npm run start:devServer
   \`\`\`

## Features

- React frontend with TanStack Query
- C# Azure Functions API
- Azure Storage integration
- Microsoft Fabric API integration
- cs-ui-library components

## Development

See the copilot instructions in:
- \`api/COPILOT_INSTRUCTIONS.md\` for backend development
- \`app/COPILOT_INSTRUCTIONS.md\` for frontend development

## Deployment

1. Build frontend: \`npm run build:prod\`
2. Deploy API to Azure Functions
3. Configure environment variables in Azure
EOF

echo "Setup complete!"
echo ""
echo "Next steps:"
echo "1. Update the workload manifest in Manifest/ directory"
echo "2. Configure your Azure resources and environment variables"
echo "3. Customize the UI and API for your specific needs"
echo "4. Run 'npm install' to install dependencies"
echo "5. Run 'npm run start:devServer' to start development"
