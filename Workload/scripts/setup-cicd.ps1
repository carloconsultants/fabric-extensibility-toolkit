param (
    # The name of your workload
    [Parameter(Mandatory = $true)]
    [string]$WorkloadName,
    
    # The resource group name
    [Parameter(Mandatory = $true)]
    [string]$ResourceGroupName,
    
    # The Azure subscription ID
    [Parameter(Mandatory = $true)]
    [string]$SubscriptionId,
    
    # The Azure region
    [string]$Location = "East US",
    
    # The organization name for the Static Web App
    [string]$OrganizationName = "Org",
    
    # The environment (dev, test, prod)
    [ValidateSet("dev", "test", "prod", "all")]
    [string]$Environment = "all",
    
    # Force setup without confirmation
    [boolean]$Force = $false
)

<#
.SYNOPSIS
    Sets up CI/CD infrastructure for a Fabric workload using Azure Static Web Apps.

.DESCRIPTION
    This script creates the necessary Azure resources and configures CI/CD pipelines
    for deploying a Fabric workload to Azure Static Web Apps.

.PARAMETER WorkloadName
    The name of your workload (e.g., "MyDataApp").

.PARAMETER ResourceGroupName
    The name of the Azure resource group to create or use.

.PARAMETER SubscriptionId
    The Azure subscription ID to use.

.PARAMETER Location
    The Azure region to deploy resources to.

.PARAMETER OrganizationName
    The organization name for the Static Web App (e.g., "Org" for Org.MyDataApp).

.PARAMETER Environment
    The environment to set up (dev, test, prod, or all).

.PARAMETER Force
    Skip confirmation prompts and proceed immediately.

.EXAMPLE
    .\setup-cicd.ps1 -WorkloadName "MyDataApp" -ResourceGroupName "MyDataApp-01" -SubscriptionId "12345678-1234-1234-1234-123456789012"

.EXAMPLE
    .\setup-cicd.ps1 -WorkloadName "MyDataApp" -ResourceGroupName "MyDataApp-01" -SubscriptionId "12345678-1234-1234-1234-123456789012" -Environment "dev" -Force $true
#>

# Function to print formatted information
function Write-Info {
    param (
        [string]$Message,
        [string]$Color = "Green"
    )
    Write-Host $Message -ForegroundColor $Color
}

# Function to print formatted warnings
function Write-Warning-Custom {
    param (
        [string]$Message
    )
    Write-Host $Message -ForegroundColor Yellow
}

# Function to print formatted errors
function Write-Error-Custom {
    param (
        [string]$Message
    )
    Write-Host $Message -ForegroundColor Red
}

# Function to check if Azure CLI is installed and user is logged in
function Test-AzureCLI {
    try {
        $null = az --version
        Write-Info "Azure CLI is installed."
    }
    catch {
        Write-Error-Custom "Azure CLI is not installed. Please install Azure CLI first."
        Write-Info "Download from: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli"
        exit 1
    }

    try {
        $account = az account show 2>$null | ConvertFrom-Json
        if ($null -eq $account) {
            Write-Warning-Custom "Not logged in to Azure CLI. Attempting to login..."
            az login
        }
        else {
            Write-Info "Logged in to Azure as: $($account.user.name)"
            if ($account.id -ne $SubscriptionId) {
                Write-Info "Switching to subscription: $SubscriptionId"
                az account set --subscription $SubscriptionId
            }
            else {
                Write-Info "Using subscription: $($account.name) ($($account.id))"
            }
        }
    }
    catch {
        Write-Error-Custom "Failed to authenticate with Azure CLI."
        exit 1
    }
}

# Function to create resource group
function New-ResourceGroup {
    param (
        [string]$ResourceGroupName,
        [string]$Location
    )
    
    Write-Info "Creating resource group: $ResourceGroupName"
    
    try {
        $rg = az group show --name $ResourceGroupName 2>$null | ConvertFrom-Json
        if ($null -eq $rg) {
            az group create --name $ResourceGroupName --location $Location
            Write-Info "Resource group created successfully."
        }
        else {
            Write-Info "Resource group already exists."
        }
        return $true
    }
    catch {
        Write-Error-Custom "Failed to create resource group: $($_.Exception.Message)"
        return $false
    }
}

# Function to create Static Web App
function New-StaticWebApp {
    param (
        [string]$AppName,
        [string]$ResourceGroupName,
        [string]$Location
    )
    
    Write-Info "Creating Static Web App: $AppName"
    
    try {
        $swa = az staticwebapp show --name $AppName --resource-group $ResourceGroupName 2>$null | ConvertFrom-Json
        if ($null -eq $swa) {
            $swa = az staticwebapp create --name $AppName --resource-group $ResourceGroupName --location $Location --source "https://github.com/$env:GITHUB_REPOSITORY" --branch "main" --app-location "app" --api-location "api" --output-location "dist" | ConvertFrom-Json
            Write-Info "Static Web App created successfully."
        }
        else {
            Write-Info "Static Web App already exists."
        }
        
        # Get deployment token
        $deploymentToken = az staticwebapp secrets list --name $AppName --resource-group $ResourceGroupName --query "properties.apiKey" -o tsv
        Write-Info "Deployment token: $deploymentToken"
        
        return @{
            AppName = $AppName
            DeploymentToken = $deploymentToken
            DefaultHostName = $swa.defaultHostname
        }
    }
    catch {
        Write-Error-Custom "Failed to create Static Web App: $($_.Exception.Message)"
        return $null
    }
}

# Function to update configuration files
function Update-ConfigurationFiles {
    param (
        [string]$WorkloadName,
        [string]$ResourceGroupName,
        [hashtable]$DevConfig,
        [hashtable]$TestConfig,
        [hashtable]$ProdConfig
    )
    
    Write-Info "Updating configuration files..."
    
    # Update swa-cli.config.json
    $swaConfig = @{
        '$schema' = 'https://aka.ms/azure/static-web-apps-cli/schema'
        configurations = @{
            $WorkloadName = @{
                appLocation = 'app'
                apiLocation = 'api'
                outputLocation = 'dist'
                apiLanguage = 'dotnetisolated'
                apiVersion = '8.0'
                appBuildCommand = 'npm run build:prod'
                apiBuildCommand = 'dotnet publish -c Release'
                run = 'npm run start:devServer'
                appDevserverUrl = 'http://localhost:60006'
                apiDevserverUrl = 'http://localhost:7071'
                appName = $DevConfig.AppName
                resourceGroup = $ResourceGroupName
            }
        }
    }
    
    $swaConfig | ConvertTo-Json -Depth 10 | Set-Content 'swa-cli.config.json'
    Write-Info "Updated swa-cli.config.json"
    
    # Update azure-pipelines.yml
    $pipelineContent = Get-Content 'azure-pipelines.yml' -Raw
    $pipelineContent = $pipelineContent -replace 'workloadName: ''TemplateWorkload''', "workloadName: '$WorkloadName'"
    $pipelineContent = $pipelineContent -replace 'resourceGroupName: ''TemplateWorkload-01''', "resourceGroupName: '$ResourceGroupName'"
    $pipelineContent | Set-Content 'azure-pipelines.yml'
    Write-Info "Updated azure-pipelines.yml"
    
    # Update GitHub Actions workflow
    $workflowContent = Get-Content '.github/workflows/deploy.yml' -Raw
    $workflowContent = $workflowContent -replace 'WORKLOAD_NAME: ''TemplateWorkload''', "WORKLOAD_NAME: '$WorkloadName'"
    $workflowContent = $workflowContent -replace 'RESOURCE_GROUP: ''TemplateWorkload-01''', "RESOURCE_GROUP: '$ResourceGroupName'"
    $workflowContent | Set-Content '.github/workflows/deploy.yml'
    Write-Info "Updated .github/workflows/deploy.yml"
}

# Function to create environment-specific configurations
function New-EnvironmentConfigs {
    param (
        [string]$WorkloadName,
        [hashtable]$DevConfig,
        [hashtable]$TestConfig,
        [hashtable]$ProdConfig
    )
    
    Write-Info "Creating environment-specific configurations..."
    
    # Development environment
    $devEnv = @"
# Development Environment Configuration
NODE_ENV=development
WORKLOAD_NAME=$WorkloadName
ITEM_NAMES=${WorkloadName}Item
WORKLOAD_VERSION=1.0.0
LOG_LEVEL=debug

# Frontend Configuration
FRONTEND_APPID=your-dev-frontend-app-id
BACKEND_APPID=your-dev-backend-app-id
BACKEND_URL=https://$($DevConfig.DefaultHostName)
DEV_AAD_CONFIG_BE_AUDIENCE=api://your-dev-backend-app-id
DEV_AAD_CONFIG_BE_REDIRECT_URI=https://$($DevConfig.DefaultHostName)

# Backend Configuration
AZURE_TENANT_ID=your-tenant-id
AZURE_CLIENT_ID=your-client-id
AZURE_CLIENT_SECRET=your-client-secret
KEY_VAULT_ENDPOINT=https://your-keyvault.vault.azure.net/
STORAGE_ACCOUNT_NAME=your-storage-account
STORAGE_CONNECTION_STRING=your-storage-connection-string

# Table Names
PROVIDER_PROFILE_TABLE=ProviderProfiles
USERS_TABLE=Users
PROVIDER_USERS_TABLE=ProviderUsers
DATA_SHARE_OFFERS_TABLE=DataShareOffers
OFFER_ATTACHMENTS_TABLE=OfferAttachments
OFFER_HAS_REPORT_INDEX_TABLE=OfferHasReportIndex
OFFER_PROVIDER_INDEX_TABLE=OfferProviderIndex
PRIVATE_DATA_SHARES_INDEX_TABLE=PrivateDataSharesIndex
PROVIDER_TO_OFFER_LINK_INDEX_TABLE=ProviderToOfferLinkIndex
ACCEPTED_DATA_SHARES_TABLE=AcceptedDataShares
APP_REGISTRATIONS_TABLE=AppRegistrations

# Blob Containers
PROVIDER_LOGO_CONTAINER=provider-logos
OFFER_ATTACHMENT_CONTAINER=offer-attachments
"@
    
    $devEnv | Set-Content '.env.dev'
    Write-Info "Created .env.dev"
    
    # Test environment
    $testEnv = $devEnv -replace 'NODE_ENV=development', 'NODE_ENV=test' -replace 'LOG_LEVEL=debug', 'LOG_LEVEL=info' -replace "BACKEND_URL=https://$($DevConfig.DefaultHostName)", "BACKEND_URL=https://$($TestConfig.DefaultHostName)" -replace "DEV_AAD_CONFIG_BE_REDIRECT_URI=https://$($DevConfig.DefaultHostName)", "DEV_AAD_CONFIG_BE_REDIRECT_URI=https://$($TestConfig.DefaultHostName)"
    $testEnv | Set-Content '.env.test'
    Write-Info "Created .env.test"
    
    # Production environment
    $prodEnv = $devEnv -replace 'NODE_ENV=development', 'NODE_ENV=production' -replace 'LOG_LEVEL=debug', 'LOG_LEVEL=warn' -replace "BACKEND_URL=https://$($DevConfig.DefaultHostName)", "BACKEND_URL=https://$($ProdConfig.DefaultHostName)" -replace "DEV_AAD_CONFIG_BE_REDIRECT_URI=https://$($DevConfig.DefaultHostName)", "DEV_AAD_CONFIG_BE_REDIRECT_URI=https://$($ProdConfig.DefaultHostName)"
    $prodEnv | Set-Content '.env.prod'
    Write-Info "Created .env.prod"
}

# Main execution
Write-Info "=== Fabric Workload CI/CD Setup ===" "Cyan"
Write-Info ""

# Display configuration
Write-Info "Setup Configuration:" "Yellow"
Write-Info "  Workload Name: $WorkloadName"
Write-Info "  Resource Group: $ResourceGroupName"
Write-Info "  Subscription ID: $SubscriptionId"
Write-Info "  Location: $Location"
Write-Info "  Organization: $OrganizationName"
Write-Info "  Environment: $Environment"
Write-Info ""

# Confirm setup unless Force is specified
if (-not $Force) {
    $confirmation = Read-Host "Do you want to proceed with the CI/CD setup? (y/n)"
    if ($confirmation -ne 'y' -and $confirmation -ne 'Y') {
        Write-Info "Setup cancelled."
        exit 0
    }
}

$setupStartTime = Get-Date

try {
    # Validate prerequisites
    Write-Info "=== Validating Prerequisites ===" "Yellow"
    Test-AzureCLI
    Write-Info ""
    
    # Create resource group
    Write-Info "=== Creating Resource Group ===" "Yellow"
    if (-not (New-ResourceGroup -ResourceGroupName $ResourceGroupName -Location $Location)) {
        exit 1
    }
    Write-Info ""
    
    # Create Static Web Apps for each environment
    $devConfig = $null
    $testConfig = $null
    $prodConfig = $null
    
    if ($Environment -eq "dev" -or $Environment -eq "all") {
        Write-Info "=== Creating Development Static Web App ===" "Yellow"
        $devAppName = "$WorkloadName-swa-dev"
        $devConfig = New-StaticWebApp -AppName $devAppName -ResourceGroupName $ResourceGroupName -Location $Location
        if ($null -eq $devConfig) {
            exit 1
        }
        Write-Info ""
    }
    
    if ($Environment -eq "test" -or $Environment -eq "all") {
        Write-Info "=== Creating Test Static Web App ===" "Yellow"
        $testAppName = "$WorkloadName-swa-test"
        $testConfig = New-StaticWebApp -AppName $testAppName -ResourceGroupName $ResourceGroupName -Location $Location
        if ($null -eq $testConfig) {
            exit 1
        }
        Write-Info ""
    }
    
    if ($Environment -eq "prod" -or $Environment -eq "all") {
        Write-Info "=== Creating Production Static Web App ===" "Yellow"
        $prodAppName = "$WorkloadName-swa-prod"
        $prodConfig = New-StaticWebApp -AppName $prodAppName -ResourceGroupName $ResourceGroupName -Location $Location
        if ($null -eq $prodConfig) {
            exit 1
        }
        Write-Info ""
    }
    
    # Update configuration files
    Write-Info "=== Updating Configuration Files ===" "Yellow"
    Update-ConfigurationFiles -WorkloadName $WorkloadName -ResourceGroupName $ResourceGroupName -DevConfig $devConfig -TestConfig $testConfig -ProdConfig $prodConfig
    Write-Info ""
    
    # Create environment configurations
    Write-Info "=== Creating Environment Configurations ===" "Yellow"
    New-EnvironmentConfigs -WorkloadName $WorkloadName -DevConfig $devConfig -TestConfig $testConfig -ProdConfig $prodConfig
    Write-Info ""
    
    # Final summary
    $setupDuration = (Get-Date) - $setupStartTime
    Write-Info "=== CI/CD Setup Complete ===" "Green"
    Write-Info "✅ Successfully set up CI/CD infrastructure!" "Green"
    Write-Info "⏱️  Total setup time: $($setupDuration.Minutes)m $($setupDuration.Seconds)s" "Green"
    Write-Info ""
    
    Write-Info "Next steps:" "Yellow"
    Write-Info "1. Update the environment variables in .env.* files with your actual values"
    Write-Info "2. Configure GitHub secrets for deployment tokens:"
    if ($devConfig) {
        Write-Info "   - AZURE_STATIC_WEB_APPS_DEV_TOKEN: $($devConfig.DeploymentToken)" "Cyan"
    }
    if ($prodConfig) {
        Write-Info "   - AZURE_STATIC_WEB_APPS_PROD_TOKEN: $($prodConfig.DeploymentToken)" "Cyan"
    }
    Write-Info "3. Push your code to trigger the first deployment"
    Write-Info "4. Configure your Azure AD app registrations for authentication"
    Write-Info "5. Set up your Azure Storage and Key Vault resources"
    
    if ($devConfig) {
        Write-Info ""
        Write-Info "🌐 Development URL: https://$($devConfig.DefaultHostName)" "Green"
    }
    if ($testConfig) {
        Write-Info "🌐 Test URL: https://$($testConfig.DefaultHostName)" "Green"
    }
    if ($prodConfig) {
        Write-Info "🌐 Production URL: https://$($prodConfig.DefaultHostName)" "Green"
    }
    
}
catch {
    Write-Error-Custom "Setup failed with error: $($_.Exception.Message)"
    exit 1
}
