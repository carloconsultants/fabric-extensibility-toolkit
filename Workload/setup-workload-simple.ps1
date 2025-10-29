param (
    # Path to the workload configuration file
    [Parameter(Mandatory = $false)]
    [string]$ConfigPath = "workload-config.simple.json",
    
    # Force setup without confirmation
    [boolean]$Force = $false,
    
    # Skip Azure resource creation
    [boolean]$SkipAzureResources = $false
)

<#
.SYNOPSIS
    Simplified workload setup from a minimal configuration file.

.DESCRIPTION
    This script reads a simplified workload-config.json file and automatically
    generates all necessary values, reducing configuration complexity.

.PARAMETER ConfigPath
    Path to the simplified workload configuration JSON file.

.PARAMETER Force
    Skip confirmation prompts and proceed immediately.

.PARAMETER SkipAzureResources
    Skip Azure resource creation (useful for testing).

.EXAMPLE
    .\setup-workload-simple.ps1

.EXAMPLE
    .\setup-workload-simple.ps1 -ConfigPath "my-config.json" -Force $true
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
        Write-Info "Please create a workload-config.simple.json file with your workload settings."
        Write-Info "You can copy workload-config.simple.json and modify it with your values."
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

# Function to auto-generate missing values
function Expand-Configuration {
    param (
        [object]$Config
    )
    
    $workload = $Config.workload
    $azure = $Config.azure
    
    # Auto-generate Azure values
    $azure.resourceGroup = "$($workload.name)-01"
    $azure.storageAccountName = "$($workload.name.ToLower())storage"
    $azure.keyVaultEndpoint = "https://$($workload.name.ToLower())-keyvault.vault.azure.net/"
    
    # Auto-generate environment values
    foreach ($envName in $Config.environments.PSObject.Properties.Name) {
        $env = $Config.environments.$envName
        
        # Auto-generate Static Web App name
        $env.name = "$($workload.name)-swa-$envName"
        
        # Auto-generate branch name
        $env.branch = switch ($envName) {
            "dev" { "develop" }
            "prod" { "main" }
            default { $envName }
        }
        
        # Generate URL
        $env.url = "https://$($env.name).azurestaticapps.net"
        
        # Auto-generate AAD values
        $env.aad = @{
            frontendAppId = $env.clientId  # Use client ID as frontend app ID
            backendAppId = $env.clientId   # Use client ID as backend app ID  
            clientId = $env.clientId
            clientSecret = $env.clientSecret
            audience = "api://$($env.clientId)"
            redirectUri = $env.url
        }
        
        # Remove the original clientId and clientSecret from root level
        $env.PSObject.Properties.Remove('clientId')
        $env.PSObject.Properties.Remove('clientSecret')
    }
    
    # Auto-generate GitHub repository from git remote
    try {
        $gitRemote = git remote get-url origin 2>$null
        if ($gitRemote) {
            $repoPath = $gitRemote -replace '^https://github.com/', '' -replace '^git@github.com:', '' -replace '\.git$', ''
            $Config.github = @{
                repository = $repoPath
                organization = $repoPath.Split('/')[0]
            }
        }
        else {
            # Fallback values
            $Config.github = @{
                repository = "$($workload.organization.ToLower())/$($workload.name.ToLower())"
                organization = $workload.organization.ToLower()
            }
        }
    }
    catch {
        # Fallback values if git is not available
        $Config.github = @{
            repository = "$($workload.organization.ToLower())/$($workload.name.ToLower())"
            organization = $workload.organization.ToLower()
        }
    }
    
    # Add default CI configuration
    $Config.ci = @{
        nodeVersion = "20.x"
        dotnetVersion = "8.x"
        azureServiceConnection = "azure-service-connection"
    }
    
    # Add default table and container names
    $Config.tables = @{
        providerProfile = "ProviderProfiles"
        users = "Users"
        providerUsers = "ProviderUsers"
        dataShareOffers = "DataShareOffers"
        offerAttachments = "OfferAttachments"
        offersHasReportIndex = "OfferHasReportIndex"
        offersProviderIndex = "OfferProviderIndex"
        privateDataSharesIndex = "PrivateDataSharesIndex"
        providerToOfferLinkIndex = "ProviderToOfferLinkIndex"
        acceptedDataShares = "AcceptedDataShares"
        appRegistrations = "AppRegistrations"
    }
    
    $Config.blobContainers = @{
        logos = "provider-logos"
        attachments = "offer-attachments"
    }
    
    return $Config
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
    $azure = $Config.azure
    
    $logLevel = if ($Environment -eq "prod") { "warn" } elseif ($Environment -eq "test") { "info" } else { "debug" }
    
    $envContent = @"
# $($workload.displayName) - $Environment Environment Configuration
NODE_ENV=$Environment
WORKLOAD_NAME=$($workload.name)
ITEM_NAMES=$($workload.itemNames)
WORKLOAD_VERSION=$($workload.version)
LOG_LEVEL=$logLevel

# Frontend Configuration
FRONTEND_APPID=$($envConfig.aad.frontendAppId)
BACKEND_APPID=$($envConfig.aad.backendAppId)
BACKEND_URL=$($envConfig.url)
DEV_AAD_CONFIG_BE_AUDIENCE=$($envConfig.aad.audience)
DEV_AAD_CONFIG_BE_REDIRECT_URI=$($envConfig.aad.redirectUri)

# Backend Configuration
AZURE_TENANT_ID=$($azure.tenantId)
AZURE_CLIENT_ID=$($envConfig.aad.clientId)
AZURE_CLIENT_SECRET=$($envConfig.aad.clientSecret)
KEY_VAULT_ENDPOINT=$($azure.keyVaultEndpoint)
STORAGE_ACCOUNT_NAME=$($azure.storageAccountName)
STORAGE_CONNECTION_STRING=your-storage-connection-string

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

# Function to update all configuration files
function Update-AllFiles {
    param (
        [object]$Config
    )
    
    $workload = $Config.workload
    $azure = $Config.azure
    $ci = $Config.ci
    
    # Update package.json
    $packageReplacements = @{
        "fabric-developer-sample" = $workload.name
        "2.0.0" = $workload.version
        "Microsoft Fabric - Developer Sample App" = $workload.description
    }
    Update-FileContent -FilePath "package.json" -Replacements $packageReplacements
    
    # Update swa-cli.config.json
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
                resourceGroup = $azure.resourceGroup
            }
        }
    }
    $swaConfig | ConvertTo-Json -Depth 10 | Set-Content 'swa-cli.config.json'
    Write-Info "Updated swa-cli.config.json"
    
    # Update Azure Pipelines
    $pipelineReplacements = @{
        "workloadName: 'TemplateWorkload'" = "workloadName: '$($workload.name)'"
        "resourceGroupName: 'TemplateWorkload-01'" = "resourceGroupName: '$($azure.resourceGroup)'"
        "nodeVersion: '20.x'" = "nodeVersion: '$($ci.nodeVersion)'"
        "dotnetVersion: '8.x'" = "dotnetVersion: '$($ci.dotnetVersion)'"
    }
    Update-FileContent -FilePath "azure-pipelines.yml" -Replacements $pipelineReplacements
    
    # Update GitHub Actions
    $githubReplacements = @{
        "WORKLOAD_NAME: 'TemplateWorkload'" = "WORKLOAD_NAME: '$($workload.name)'"
        "RESOURCE_GROUP: 'TemplateWorkload-01'" = "RESOURCE_GROUP: '$($azure.resourceGroup)'"
        "NODE_VERSION: '20.x'" = "NODE_VERSION: '$($ci.nodeVersion)'"
        "DOTNET_VERSION: '8.x'" = "DOTNET_VERSION: '$($ci.dotnetVersion)'"
    }
    Update-FileContent -FilePath ".github/workflows/deploy.yml" -Replacements $githubReplacements
    
    # Update C# files
    $csharpReplacements = @{
        "TemplateWorkload" = $workload.name
    }
    $csharpFiles = Get-ChildItem -Path "api" -Recurse -Filter "*.cs"
    foreach ($file in $csharpFiles) {
        Update-FileContent -FilePath $file.FullName -Replacements $csharpReplacements
    }
    Write-Info "Updated C# files"
    
    # Update TypeScript files
    $tsFiles = Get-ChildItem -Path "app" -Recurse -Filter "*.ts"
    $tsxFiles = Get-ChildItem -Path "app" -Recurse -Filter "*.tsx"
    $allFiles = $tsFiles + $tsxFiles
    foreach ($file in $allFiles) {
        Update-FileContent -FilePath $file.FullName -Replacements $csharpReplacements
    }
    Write-Info "Updated TypeScript files"
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

- **Development**: $($Config.environments.dev.url)
- **Test**: $($Config.environments.test.url)
- **Production**: $($Config.environments.prod.url)

## Configuration

This workload was configured using the simplified workload-config.simple.json file. To modify settings, update the configuration file and run:

\`\`\`bash
./setup-workload-simple.ps1
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
Write-Info "=== Fabric Workload Simple Setup ===" "Cyan"
Write-Info ""

# Load and expand configuration
$config = Get-WorkloadConfig -ConfigPath $ConfigPath
$config = Expand-Configuration -Config $config

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
    # Update all configuration files
    Write-Info "=== Updating Configuration Files ===" "Yellow"
    Update-AllFiles -Config $config
    Write-Info ""
    
    # Create environment files
    Write-Info "=== Creating Environment Files ===" "Yellow"
    New-EnvironmentFile -Environment "dev" -Config $config -OutputPath ".env.dev"
    New-EnvironmentFile -Environment "test" -Config $config -OutputPath ".env.test"
    New-EnvironmentFile -Environment "prod" -Config $config -OutputPath ".env.prod"
    Write-Info ""
    
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
