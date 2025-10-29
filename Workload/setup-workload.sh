#!/bin/bash

# Complete workload setup script for Linux/Mac
# This script reads workload-config.json and automatically configures everything

set -e

# Default values
CONFIG_PATH="workload-config.json"
FORCE=false
SKIP_AZURE_RESOURCES=false
SKIP_CICD=false

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Function to print colored output
print_info() {
    echo -e "${GREEN}$1${NC}"
}

print_warning() {
    echo -e "${YELLOW}$1${NC}"
}

print_error() {
    echo -e "${RED}$1${NC}"
}

print_header() {
    echo -e "${CYAN}$1${NC}"
}

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        -c|--config)
            CONFIG_PATH="$2"
            shift 2
            ;;
        -f|--force)
            FORCE=true
            shift
            ;;
        --skip-azure)
            SKIP_AZURE_RESOURCES=true
            shift
            ;;
        --skip-cicd)
            SKIP_CICD=true
            shift
            ;;
        -h|--help)
            echo "Usage: $0 [OPTIONS]"
            echo "Options:"
            echo "  -c, --config PATH     Path to workload configuration file (default: workload-config.json)"
            echo "  -f, --force           Skip confirmation prompts"
            echo "  --skip-azure          Skip Azure resource creation"
            echo "  --skip-cicd           Skip CI/CD setup"
            echo "  -h, --help            Show this help message"
            exit 0
            ;;
        *)
            print_error "Unknown option: $1"
            exit 1
            ;;
    esac
done

# Check if jq is installed
if ! command -v jq &> /dev/null; then
    print_error "jq is required but not installed. Please install jq first."
    echo "Install with:"
    echo "  Ubuntu/Debian: sudo apt-get install jq"
    echo "  macOS: brew install jq"
    echo "  CentOS/RHEL: sudo yum install jq"
    exit 1
fi

# Check if configuration file exists
if [ ! -f "$CONFIG_PATH" ]; then
    print_error "Configuration file not found: $CONFIG_PATH"
    print_info "Please create a workload-config.json file with your workload settings."
    print_info "You can copy workload-config.example.json and modify it with your values."
    exit 1
fi

# Load configuration
print_info "Loading configuration from: $CONFIG_PATH"
WORKLOAD_NAME=$(jq -r '.workload.name' "$CONFIG_PATH")
DISPLAY_NAME=$(jq -r '.workload.displayName' "$CONFIG_PATH")
VERSION=$(jq -r '.workload.version' "$CONFIG_PATH")
RESOURCE_GROUP=$(jq -r '.azure.resourceGroup' "$CONFIG_PATH")
SUBSCRIPTION_ID=$(jq -r '.azure.subscriptionId' "$CONFIG_PATH")
GITHUB_REPO=$(jq -r '.github.repository' "$CONFIG_PATH")

# Display configuration summary
print_header "=== Fabric Workload Complete Setup ==="
echo ""
print_info "Workload Configuration:"
print_info "  Name: $WORKLOAD_NAME"
print_info "  Display Name: $DISPLAY_NAME"
print_info "  Version: $VERSION"
print_info "  Resource Group: $RESOURCE_GROUP"
print_info "  Subscription: $SUBSCRIPTION_ID"
print_info "  GitHub Repo: $GITHUB_REPO"
echo ""

# Confirm setup unless Force is specified
if [ "$FORCE" = false ]; then
    read -p "Do you want to proceed with the complete workload setup? (y/n): " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        print_info "Setup cancelled."
        exit 0
    fi
fi

# Function to replace placeholders in files
update_file() {
    local file_path="$1"
    local old_value="$2"
    local new_value="$3"
    
    if [ -f "$file_path" ]; then
        sed -i.bak "s|$(printf '%s\n' "$old_value" | sed 's/[[\.*^$()+?{|]/\\&/g')|$(printf '%s\n' "$new_value" | sed 's/[[\.*^$()+?{|]/\\&/g')|g" "$file_path"
        rm -f "$file_path.bak"
        print_info "Updated: $file_path"
    else
        print_warning "File not found: $file_path"
    fi
}

# Function to create environment file
create_env_file() {
    local env_name="$1"
    local output_path="$2"
    
    local log_level
    case $env_name in
        "prod") log_level="warn" ;;
        "test") log_level="info" ;;
        *) log_level="debug" ;;
    esac
    
    local frontend_app_id=$(jq -r ".environments.$env_name.aad.frontendAppId" "$CONFIG_PATH")
    local backend_app_id=$(jq -r ".environments.$env_name.aad.backendAppId" "$CONFIG_PATH")
    local client_id=$(jq -r ".environments.$env_name.aad.clientId" "$CONFIG_PATH")
    local client_secret=$(jq -r ".environments.$env_name.aad.clientSecret" "$CONFIG_PATH")
    local audience=$(jq -r ".environments.$env_name.aad.audience" "$CONFIG_PATH")
    local redirect_uri=$(jq -r ".environments.$env_name.aad.redirectUri" "$CONFIG_PATH")
    local url=$(jq -r ".environments.$env_name.url" "$CONFIG_PATH")
    local tenant_id=$(jq -r '.azure.tenantId' "$CONFIG_PATH")
    local key_vault_endpoint=$(jq -r '.azure.keyVault.endpoint' "$CONFIG_PATH")
    local storage_account_name=$(jq -r '.azure.storageAccount.name' "$CONFIG_PATH")
    local storage_connection_string=$(jq -r '.azure.storageAccount.connectionString' "$CONFIG_PATH")
    
    cat > "$output_path" << EOF
# $DISPLAY_NAME - $env_name Environment Configuration
NODE_ENV=$env_name
WORKLOAD_NAME=$WORKLOAD_NAME
ITEM_NAMES=$(jq -r '.workload.itemNames' "$CONFIG_PATH")
WORKLOAD_VERSION=$VERSION
LOG_LEVEL=$log_level

# Frontend Configuration
FRONTEND_APPID=$frontend_app_id
BACKEND_APPID=$backend_app_id
BACKEND_URL=$url
DEV_AAD_CONFIG_BE_AUDIENCE=$audience
DEV_AAD_CONFIG_BE_REDIRECT_URI=$redirect_uri

# Backend Configuration
AZURE_TENANT_ID=$tenant_id
AZURE_CLIENT_ID=$client_id
AZURE_CLIENT_SECRET=$client_secret
KEY_VAULT_ENDPOINT=$key_vault_endpoint
STORAGE_ACCOUNT_NAME=$storage_account_name
STORAGE_CONNECTION_STRING=$storage_connection_string

# Table Names
PROVIDER_PROFILE_TABLE=$(jq -r '.tables.providerProfile' "$CONFIG_PATH")
USERS_TABLE=$(jq -r '.tables.users' "$CONFIG_PATH")
PROVIDER_USERS_TABLE=$(jq -r '.tables.providerUsers' "$CONFIG_PATH")
DATA_SHARE_OFFERS_TABLE=$(jq -r '.tables.dataShareOffers' "$CONFIG_PATH")
OFFER_ATTACHMENTS_TABLE=$(jq -r '.tables.offerAttachments' "$CONFIG_PATH")
OFFER_HAS_REPORT_INDEX_TABLE=$(jq -r '.tables.offersHasReportIndex' "$CONFIG_PATH")
OFFER_PROVIDER_INDEX_TABLE=$(jq -r '.tables.offersProviderIndex' "$CONFIG_PATH")
PRIVATE_DATA_SHARES_INDEX_TABLE=$(jq -r '.tables.privateDataSharesIndex' "$CONFIG_PATH")
PROVIDER_TO_OFFER_LINK_INDEX_TABLE=$(jq -r '.tables.providerToOfferLinkIndex' "$CONFIG_PATH")
ACCEPTED_DATA_SHARES_TABLE=$(jq -r '.tables.acceptedDataShares' "$CONFIG_PATH")
APP_REGISTRATIONS_TABLE=$(jq -r '.tables.appRegistrations' "$CONFIG_PATH")

# Blob Containers
PROVIDER_LOGO_CONTAINER=$(jq -r '.blobContainers.logos' "$CONFIG_PATH")
OFFER_ATTACHMENT_CONTAINER=$(jq -r '.blobContainers.attachments' "$CONFIG_PATH")
EOF
    
    print_info "Created environment file: $output_path"
}

# Start setup
setup_start_time=$(date +%s)

print_header "=== Updating Configuration Files ==="

# Update package.json
update_file "package.json" "fabric-developer-sample" "$WORKLOAD_NAME"
update_file "package.json" "2.0.0" "$VERSION"
update_file "package.json" "Microsoft Fabric - Developer Sample App" "$DISPLAY_NAME"

# Update swa-cli.config.json
jq --arg workload_name "$WORKLOAD_NAME" --arg resource_group "$RESOURCE_GROUP" --arg dev_name "$(jq -r '.environments.dev.name' "$CONFIG_PATH")" '
  .configurations = {
    ($workload_name): {
      appLocation: "app",
      apiLocation: "api",
      outputLocation: "dist",
      apiLanguage: "dotnetisolated",
      apiVersion: "8.0",
      appBuildCommand: "npm run build:prod",
      apiBuildCommand: "dotnet publish -c Release",
      run: "npm run start:devServer",
      appDevserverUrl: "http://localhost:60006",
      apiDevserverUrl: "http://localhost:7071",
      appName: $dev_name,
      resourceGroup: $resource_group
    }
  }
' swa-cli.config.json > swa-cli.config.json.tmp && mv swa-cli.config.json.tmp swa-cli.config.json
print_info "Updated swa-cli.config.json"

# Update Azure Pipelines
update_file "azure-pipelines.yml" "workloadName: 'TemplateWorkload'" "workloadName: '$WORKLOAD_NAME'"
update_file "azure-pipelines.yml" "resourceGroupName: 'TemplateWorkload-01'" "resourceGroupName: '$RESOURCE_GROUP'"

# Update GitHub Actions
update_file ".github/workflows/deploy.yml" "WORKLOAD_NAME: 'TemplateWorkload'" "WORKLOAD_NAME: '$WORKLOAD_NAME'"
update_file ".github/workflows/deploy.yml" "RESOURCE_GROUP: 'TemplateWorkload-01'" "RESOURCE_GROUP: '$RESOURCE_GROUP'"

# Update C# files
find api -name "*.cs" -type f -exec sed -i.bak "s/TemplateWorkload/$WORKLOAD_NAME/g" {} \;
find api -name "*.cs.bak" -type f -delete
print_info "Updated C# files"

# Update TypeScript files
find app -name "*.ts" -o -name "*.tsx" | xargs sed -i.bak "s/TemplateWorkload/$WORKLOAD_NAME/g"
find app -name "*.ts.bak" -o -name "*.tsx.bak" -type f -delete
print_info "Updated TypeScript files"

echo ""

print_header "=== Creating Environment Files ==="
create_env_file "dev" ".env.dev"
create_env_file "test" ".env.test"
create_env_file "prod" ".env.prod"
echo ""

# Create README
print_header "=== Creating Documentation ==="
cat > README.md << EOF
# $DISPLAY_NAME

$(jq -r '.workload.description' "$CONFIG_PATH")

## Quick Start

### 1. Install Dependencies
\`\`\`bash
npm install
\`\`\`

### 2. Configure Environment
\`\`\`bash
cp .env.dev .env.local
# Edit .env.local with your configuration
\`\`\`

### 3. Start Development
\`\`\`bash
npm run start:devServer
\`\`\`

## Environments

- **Development**: $(jq -r '.environments.dev.url' "$CONFIG_PATH")
- **Test**: $(jq -r '.environments.test.url' "$CONFIG_PATH")
- **Production**: $(jq -r '.environments.prod.url' "$CONFIG_PATH")

## Configuration

This workload was configured using the workload-config.json file. To modify settings, update the configuration file and run:

\`\`\`bash
./setup-workload.sh
\`\`\`

## Development

See the copilot instructions in:
- \`api/COPILOT_INSTRUCTIONS.md\` for backend development
- \`app/COPILOT_INSTRUCTIONS.md\` for frontend development

## Deployment

The workload is configured with automated CI/CD:
- **GitHub Actions**: Automatic deployment on push to main/develop
- **Azure DevOps**: Alternative CI/CD pipeline
- **Manual Deployment**: Use \`./scripts/deploy.ps1\`

## Support

- **Author**: $(jq -r '.workload.author' "$CONFIG_PATH") ($(jq -r '.workload.email' "$CONFIG_PATH"))
- **Version**: $VERSION
- **Organization**: $(jq -r '.workload.organization' "$CONFIG_PATH")
EOF

print_info "Created README.md"
echo ""

# Final summary
setup_end_time=$(date +%s)
setup_duration=$((setup_end_time - setup_start_time))

print_header "=== Setup Complete ==="
print_info "✅ Successfully configured $DISPLAY_NAME!"
print_info "⏱️  Total setup time: ${setup_duration}s"
echo ""

print_info "Next steps:"
print_info "1. Run 'npm install' to install dependencies"
print_info "2. Update the workload manifest in Manifest/ directory"
print_info "3. Configure your Azure AD app registrations"
print_info "4. Set up GitHub secrets for deployment tokens"
print_info "5. Run 'npm run start:devServer' to start development"
echo ""

print_info "Environment URLs:"
print_info "  Development: $(jq -r '.environments.dev.url' "$CONFIG_PATH")"
print_info "  Test: $(jq -r '.environments.test.url' "$CONFIG_PATH")"
print_info "  Production: $(jq -r '.environments.prod.url' "$CONFIG_PATH")"

print_info ""
print_info "🎉 Your Fabric workload is ready for development!"
