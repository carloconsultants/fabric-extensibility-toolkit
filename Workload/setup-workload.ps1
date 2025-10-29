param (
    # Path to the workload configuration file
    [Parameter(Mandatory = $false)]
    [string]$ConfigPath = "workload-config.json",
    
    # Force setup without confirmation
    [boolean]$Force = $false,
    
    # Skip Azure resource creation
    [boolean]$SkipAzureResources = $false,
    
    # Skip CI/CD setup
    [boolean]$SkipCICD = $false
)

<#
.SYNOPSIS
    Complete workload setup from a single configuration file.

.DESCRIPTION
    This script reads the workload-config.json file and automatically configures
    all aspects of the workload including:
    - File replacements and updates
    - Environment configuration
    - Azure resource creation
    - CI/CD pipeline setup
    - GitHub Actions configuration

.PARAMETER ConfigPath
    Path to the workload configuration JSON file.

.PARAMETER Force
    Skip confirmation prompts and proceed immediately.

.PARAMETER SkipAzureResources
    Skip Azure resource creation (useful for testing).

.PARAMETER SkipCICD
    Skip CI/CD pipeline setup.

.EXAMPLE
    .\setup-workload.ps1

.EXAMPLE
    .\setup-workload.ps1 -ConfigPath "my-workload-config.json" -Force $true
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

# Function to load and validate configuration
function Get-WorkloadConfig {
    param (
        [string]$ConfigPath
    )
    
    if (-not (Test-Path $ConfigPath)) {
        Write-Error-Custom "Configuration file not found: $ConfigPath"
        Write-Info "Please create a workload-config.json file with your workload settings."
        Write-Info "You can copy workload-config.json and modify it with your values."
        exit 1
    }
    
    try {
        $config = Get-Content $ConfigPath -Raw | ConvertFrom-Json
        Write-Info "Configuration loaded successfully from: $ConfigPath"
        return $config
    }
    catch {
        Write-Error-Custom "Failed to parse configuration file: $($_.Exception.Message)"
        exit 1
    }
}

# Function to replace placeholders in file content
function Update-FileContent {
    param (
        [string]$FilePath,
        [hashtable]$Replacements
    )
    
    if (-not (Test-Path $FilePath)) {
        Write-Warning-Custom "File not found: $FilePath"
        return $false
    }
    
    try {
        $content = Get-Content $FilePath -Raw
        
        foreach ($key in $Replacements.Keys) {
            $content = $content -replace [regex]::Escape($key), $Replacements[$key]
        }
        
        Set-Content -Path $FilePath -Value $content -NoNewline
        Write-Info "Updated: $FilePath"
        return $true
    }
    catch {
        Write-Error-Custom "Failed to update file $FilePath : $($_.Exception.Message)"
        return $false
    }
}

# Function to create environment file
function New-EnvironmentFile {
    param (
        [string]$Environment,
        [object]$Config,
        [string]$OutputPath
    )
    
    $envConfig = $Config.environments.$Environment
    $workload = $Config.workload
    
    $envContent = @"
# $($workload.displayName) - $Environment Environment Configuration
NODE_ENV=$Environment
WORKLOAD_NAME=$($workload.name)
ITEM_NAMES=$($workload.itemNames)
WORKLOAD_VERSION=$($workload.version)
LOG_LEVEL=$(if ($Environment -eq "prod") { "warn" } elseif ($Environment -eq "test") { "info" } else { "debug" })

# Frontend Configuration
FRONTEND_APPID=$($envConfig.aad.frontendAppId)
BACKEND_APPID=$($envConfig.aad.backendAppId)
BACKEND_URL=$($envConfig.url)
DEV_AAD_CONFIG_BE_AUDIENCE=$($envConfig.aad.audience)
DEV_AAD_CONFIG_BE_REDIRECT_URI=$($envConfig.aad.redirectUri)

# Backend Configuration
AZURE_TENANT_ID=$($Config.azure.tenantId)
AZURE_CLIENT_ID=$($envConfig.aad.clientId)
AZURE_CLIENT_SECRET=$($envConfig.aad.clientSecret)
KEY_VAULT_ENDPOINT=$($Config.azure.keyVault.endpoint)
STORAGE_ACCOUNT_NAME=$($Config.azure.storageAccount.name)
STORAGE_CONNECTION_STRING=$($Config.azure.storageAccount.connectionString)

# Table Names
PROVIDER_PROFILE_TABLE=$($Config.tables.providerProfile)
USERS_TABLE=$($Config.tables.users)
PROVIDER_USERS_TABLE=$($Config.tables.providerUsers)
DATA_SHARE_OFFERS_TABLE=$($Config.tables.dataShareOffers)
OFFER_ATTACHMENTS_TABLE=$($Config.tables.offerAttachments)
OFFER_HAS_REPORT_INDEX_TABLE=$($Config.tables.offersHasReportIndex)
OFFER_PROVIDER_INDEX_TABLE=$($Config.tables.offersProviderIndex)
PRIVATE_DATA_SHARES_INDEX_TABLE=$($Config.tables.privateDataSharesIndex)
PROVIDER_TO_OFFER_LINK_INDEX_TABLE=$($Config.tables.providerToOfferLinkIndex)
ACCEPTED_DATA_SHARES_TABLE=$($Config.tables.acceptedDataShares)
APP_REGISTRATIONS_TABLE=$($Config.tables.appRegistrations)

# Blob Containers
PROVIDER_LOGO_CONTAINER=$($Config.blobContainers.logos)
OFFER_ATTACHMENT_CONTAINER=$($Config.blobContainers.attachments)
"@
    
    try {
        Set-Content -Path $OutputPath -Value $envContent
        Write-Info "Created environment file: $OutputPath"
        return $true
    }
    catch {
        Write-Error-Custom "Failed to create environment file $OutputPath : $($_.Exception.Message)"
        return $false
    }
}

# Function to update package.json
function Update-PackageJson {
    param (
        [object]$Config
    )
    
    $workload = $Config.workload
    
    $replacements = @{
        "fabric-developer-sample" = $workload.name
        "2.0.0" = $workload.version
        "Microsoft Fabric - Developer Sample App" = $workload.description
    }
    
    return Update-FileContent -FilePath "package.json" -Replacements $replacements
}

# Function to update swa-cli.config.json
function Update-SWAConfig {
    param (
        [object]$Config
    )
    
    $workload = $Config.workload
    $devEnv = $Config.environments.dev
    
    $swaConfig = @{
        '$schema' = 'https://aka.ms/azure/static-web-apps-cli/schema'
        configurations = @{
            $workload.name = @{
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
                appName = $devEnv.name
                resourceGroup = $Config.azure.resourceGroup
            }
        }
    }
    
    try {
        $swaConfig | ConvertTo-Json -Depth 10 | Set-Content 'swa-cli.config.json'
        Write-Info "Updated swa-cli.config.json"
        return $true
    }
    catch {
        Write-Error-Custom "Failed to update swa-cli.config.json : $($_.Exception.Message)"
        return $false
    }
}

# Function to update Azure Pipelines
function Update-AzurePipelines {
    param (
        [object]$Config
    )
    
    $workload = $Config.workload
    $ci = $Config.ci
    
    $replacements = @{
        "workloadName: 'TemplateWorkload'" = "workloadName: '$($workload.name)'"
        "resourceGroupName: 'TemplateWorkload-01'" = "resourceGroupName: '$($Config.azure.resourceGroup)'"
        "nodeVersion: '20.x'" = "nodeVersion: '$($ci.nodeVersion)'"
        "dotnetVersion: '8.x'" = "dotnetVersion: '$($ci.dotnetVersion)'"
    }
    
    return Update-FileContent -FilePath "azure-pipelines.yml" -Replacements $replacements
}

# Function to update GitHub Actions
function Update-GitHubActions {
    param (
        [object]$Config
    )
    
    $workload = $Config.workload
    $ci = $Config.ci
    
    $replacements = @{
        "WORKLOAD_NAME: 'TemplateWorkload'" = "WORKLOAD_NAME: '$($workload.name)'"
        "RESOURCE_GROUP: 'TemplateWorkload-01'" = "RESOURCE_GROUP: '$($Config.azure.resourceGroup)'"
        "NODE_VERSION: '20.x'" = "NODE_VERSION: '$($ci.nodeVersion)'"
        "DOTNET_VERSION: '8.x'" = "DOTNET_VERSION: '$($ci.dotnetVersion)'"
    }
    
    return Update-FileContent -FilePath ".github/workflows/deploy.yml" -Replacements $replacements
}

# Function to update C# files
function Update-CSharpFiles {
    param (
        [object]$Config
    )
    
    $workload = $Config.workload
    
    $replacements = @{
        "TemplateWorkload" = $workload.name
    }
    
    $csharpFiles = Get-ChildItem -Path "api" -Recurse -Filter "*.cs"
    $success = $true
    
    foreach ($file in $csharpFiles) {
        if (-not (Update-FileContent -FilePath $file.FullName -Replacements $replacements)) {
            $success = $false
        }
    }
    
    return $success
}

# Function to update TypeScript files
function Update-TypeScriptFiles {
    param (
        [object]$Config
    )
    
    $workload = $Config.workload
    
    $replacements = @{
        "TemplateWorkload" = $workload.name
    }
    
    $tsFiles = Get-ChildItem -Path "app" -Recurse -Filter "*.ts"
    $tsxFiles = Get-ChildItem -Path "app" -Recurse -Filter "*.tsx"
    $allFiles = $tsFiles + $tsxFiles
    $success = $true
    
    foreach ($file in $allFiles) {
        if (-not (Update-FileContent -FilePath $file.FullName -Replacements $replacements)) {
            $success = $false
        }
    }
    
    return $success
}

# Function to create Azure resources
function New-AzureResources {
    param (
        [object]$Config
    )
    
    if ($SkipAzureResources) {
        Write-Info "Skipping Azure resource creation (SkipAzureResources = true)"
        return $true
    }
    
    Write-Info "Creating Azure resources..."
    
    try {
        # Check if Azure CLI is available
        $null = az --version
    }
    catch {
        Write-Error-Custom "Azure CLI is not installed. Please install Azure CLI first."
        Write-Info "Download from: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli"
        return $false
    }
    
    # Login to Azure
    try {
        $account = az account show 2>$null | ConvertFrom-Json
        if ($null -eq $account) {
            Write-Info "Logging in to Azure..."
            az login
        }
        else {
            Write-Info "Already logged in to Azure as: $($account.user.name)"
        }
        
        # Set subscription
        if ($account.id -ne $Config.azure.subscriptionId) {
            Write-Info "Switching to subscription: $($Config.azure.subscriptionId)"
            az account set --subscription $Config.azure.subscriptionId
        }
    }
    catch {
        Write-Error-Custom "Failed to authenticate with Azure CLI."
        return $false
    }
    
    # Create resource group
    Write-Info "Creating resource group: $($Config.azure.resourceGroup)"
    az group create --name $Config.azure.resourceGroup --location $Config.azure.location
    
    # Create Static Web Apps for each environment
    foreach ($envName in $Config.environments.PSObject.Properties.Name) {
        $env = $Config.environments.$envName
        Write-Info "Creating Static Web App: $($env.name)"
        
        az staticwebapp create `
            --name $env.name `
            --resource-group $Config.azure.resourceGroup `
            --location $Config.azure.location `
            --source "https://github.com/$($Config.github.organization)/$($Config.github.repository.Split('/')[1])" `
            --branch $env.branch `
            --app-location "app" `
            --api-location "api" `
            --output-location "dist"
    }
    
    Write-Info "Azure resources created successfully!"
    return $true
}

# Function to create README
function New-Readme {
    param (
        [object]$Config
    )
    
    $workload = $Config.workload
    
    $readmeContent = @"
# $($workload.displayName)

$($workload.description)

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

- **Development**: https://$($Config.environments.dev.url.Replace('https://', ''))
- **Test**: https://$($Config.environments.test.url.Replace('https://', ''))
- **Production**: https://$($Config.environments.prod.url.Replace('https://', ''))

## Configuration

This workload was configured using the workload-config.json file. To modify settings, update the configuration file and run:

\`\`\`bash
./setup-workload.ps1
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

- **Author**: $($workload.author) ($($workload.email))
- **Version**: $($workload.version)
- **Organization**: $($workload.organization)
"@
    
    try {
        Set-Content -Path "README.md" -Value $readmeContent
        Write-Info "Created README.md"
        return $true
    }
    catch {
        Write-Error-Custom "Failed to create README.md : $($_.Exception.Message)"
        return $false
    }
}

# Main execution
Write-Info "=== Fabric Workload Complete Setup ===" "Cyan"
Write-Info ""

# Load configuration
$config = Get-WorkloadConfig -ConfigPath $ConfigPath

# Display configuration summary
Write-Info "Workload Configuration:" "Yellow"
Write-Info "  Name: $($config.workload.name)"
Write-Info "  Display Name: $($config.workload.displayName)"
Write-Info "  Version: $($config.workload.version)"
Write-Info "  Organization: $($config.workload.organization)"
Write-Info "  Resource Group: $($config.azure.resourceGroup)"
Write-Info "  Subscription: $($config.azure.subscriptionId)"
Write-Info "  GitHub Repo: $($config.github.repository)"
Write-Info ""

# Confirm setup unless Force is specified
if (-not $Force) {
    $confirmation = Read-Host "Do you want to proceed with the complete workload setup? (y/n)"
    if ($confirmation -ne 'y' -and $confirmation -ne 'Y') {
        Write-Info "Setup cancelled."
        exit 0
    }
}

$setupStartTime = Get-Date

try {
    # Update configuration files
    Write-Info "=== Updating Configuration Files ===" "Yellow"
    Update-PackageJson -Config $config
    Update-SWAConfig -Config $config
    Update-AzurePipelines -Config $config
    Update-GitHubActions -Config $config
    Update-CSharpFiles -Config $config
    Update-TypeScriptFiles -Config $config
    Write-Info ""
    
    # Create environment files
    Write-Info "=== Creating Environment Files ===" "Yellow"
    New-EnvironmentFile -Environment "dev" -Config $config -OutputPath ".env.dev"
    New-EnvironmentFile -Environment "test" -Config $config -OutputPath ".env.test"
    New-EnvironmentFile -Environment "prod" -Config $config -OutputPath ".env.prod"
    Write-Info ""
    
    # Create Azure resources
    if (-not $SkipAzureResources) {
        Write-Info "=== Creating Azure Resources ===" "Yellow"
        New-AzureResources -Config $config
        Write-Info ""
    }
    
    # Create README
    Write-Info "=== Creating Documentation ===" "Yellow"
    New-Readme -Config $config
    Write-Info ""
    
    # Final summary
    $setupDuration = (Get-Date) - $setupStartTime
    Write-Info "=== Setup Complete ===" "Green"
    Write-Info "✅ Successfully configured $($config.workload.displayName)!" "Green"
    Write-Info "⏱️  Total setup time: $($setupDuration.Minutes)m $($setupDuration.Seconds)s" "Green"
    Write-Info ""
    
    Write-Info "Next steps:" "Yellow"
    Write-Info "1. Run 'npm install' to install dependencies"
    Write-Info "2. Update the workload manifest in Manifest/ directory"
    Write-Info "3. Configure your Azure AD app registrations"
    Write-Info "4. Set up GitHub secrets for deployment tokens"
    Write-Info "5. Run 'npm run start:devServer' to start development"
    Write-Info ""
    
    Write-Info "Environment URLs:" "Yellow"
    Write-Info "  Development: $($config.environments.dev.url)" "Cyan"
    Write-Info "  Test: $($config.environments.test.url)" "Cyan"
    Write-Info "  Production: $($config.environments.prod.url)" "Cyan"
    
}
catch {
    Write-Error-Custom "Setup failed with error: $($_.Exception.Message)"
    exit 1
}
