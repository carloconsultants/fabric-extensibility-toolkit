param (
    # The environment to deploy to
    [Parameter(Mandatory = $true)]
    [ValidateSet("dev", "test", "prod")]
    [string]$Environment,
    
    # The deployment token for the environment
    [Parameter(Mandatory = $true)]
    [string]$DeploymentToken,
    
    # Force deployment without confirmation
    [boolean]$Force = $false,
    
    # Skip building (use existing build)
    [boolean]$SkipBuild = $false
)

<#
.SYNOPSIS
    Deploys the Fabric workload to Azure Static Web Apps.

.DESCRIPTION
    This script builds and deploys the workload to the specified environment
    using Azure Static Web Apps CLI.

.PARAMETER Environment
    The environment to deploy to (dev, test, prod).

.PARAMETER DeploymentToken
    The deployment token for the target environment.

.PARAMETER Force
    Skip confirmation prompts and deploy immediately.

.PARAMETER SkipBuild
    Skip the build process and use existing build artifacts.

.EXAMPLE
    .\deploy.ps1 -Environment "dev" -DeploymentToken "your-dev-token"

.EXAMPLE
    .\deploy.ps1 -Environment "prod" -DeploymentToken "your-prod-token" -Force $true
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

# Function to check if SWA CLI is installed
function Test-SWACLI {
    try {
        $null = swa --version
        Write-Info "Azure Static Web Apps CLI is installed."
        return $true
    }
    catch {
        Write-Error-Custom "Azure Static Web Apps CLI is not installed."
        Write-Info "Install it with: npm install -g @azure/static-web-apps-cli"
        return $false
    }
}

# Function to build the application
function Build-Application {
    Write-Info "Building application..."
    
    try {
        # Build frontend
        Write-Info "Building frontend..."
        Set-Location "app"
        npm run build:prod
        Set-Location ".."
        
        # Build API
        Write-Info "Building API..."
        Set-Location "api"
        dotnet publish -c Release -o ../build/api
        Set-Location ".."
        
        Write-Info "Build completed successfully."
        return $true
    }
    catch {
        Write-Error-Custom "Build failed: $($_.Exception.Message)"
        return $false
    }
}

# Function to deploy to Azure Static Web Apps
function Deploy-ToSWA {
    param (
        [string]$Environment,
        [string]$DeploymentToken
    )
    
    Write-Info "Deploying to $Environment environment..."
    
    try {
        # Create build directory if it doesn't exist
        if (-not (Test-Path "build")) {
            New-Item -ItemType Directory -Path "build" -Force
        }
        
        # Copy frontend build to build directory
        if (Test-Path "app/dist") {
            Copy-Item -Path "app/dist/*" -Destination "build/app" -Recurse -Force
        }
        
        # Deploy using SWA CLI
        $deployCommand = "swa deploy --app-location build/app --api-location build/api --deployment-token $DeploymentToken --env $Environment"
        
        Write-Info "Running: $deployCommand"
        Invoke-Expression $deployCommand
        
        if ($LASTEXITCODE -eq 0) {
            Write-Info "Deployment completed successfully!"
            return $true
        }
        else {
            Write-Error-Custom "Deployment failed with exit code: $LASTEXITCODE"
            return $false
        }
    }
    catch {
        Write-Error-Custom "Deployment failed: $($_.Exception.Message)"
        return $false
    }
}

# Main execution
Write-Info "=== Fabric Workload Deployment ===" "Cyan"
Write-Info ""

# Display configuration
Write-Info "Deployment Configuration:" "Yellow"
Write-Info "  Environment: $Environment"
Write-Info "  Skip Build: $SkipBuild"
Write-Info "  Force: $Force"
Write-Info ""

# Confirm deployment unless Force is specified
if (-not $Force) {
    $confirmation = Read-Host "Do you want to proceed with the deployment? (y/n)"
    if ($confirmation -ne 'y' -and $confirmation -ne 'Y') {
        Write-Info "Deployment cancelled."
        exit 0
    }
}

$deploymentStartTime = Get-Date

try {
    # Validate prerequisites
    Write-Info "=== Validating Prerequisites ===" "Yellow"
    if (-not (Test-SWACLI)) {
        exit 1
    }
    Write-Info ""
    
    # Build application if not skipping
    if (-not $SkipBuild) {
        Write-Info "=== Building Application ===" "Yellow"
        if (-not (Build-Application)) {
            exit 1
        }
        Write-Info ""
    }
    
    # Deploy to Azure Static Web Apps
    Write-Info "=== Deploying to Azure Static Web Apps ===" "Yellow"
    if (-not (Deploy-ToSWA -Environment $Environment -DeploymentToken $DeploymentToken)) {
        exit 1
    }
    Write-Info ""
    
    # Final summary
    $deploymentDuration = (Get-Date) - $deploymentStartTime
    Write-Info "=== Deployment Complete ===" "Green"
    Write-Info "✅ Successfully deployed to $Environment environment!" "Green"
    Write-Info "⏱️  Total deployment time: $($deploymentDuration.Minutes)m $($deploymentDuration.Seconds)s" "Green"
    Write-Info ""
    Write-Info "Next steps:" "Yellow"
    Write-Info "1. Test your workload functionality in the browser"
    Write-Info "2. Check the Azure Portal for deployment status"
    Write-Info "3. Monitor application logs for any issues"
    
}
catch {
    Write-Error-Custom "Deployment failed with error: $($_.Exception.Message)"
    exit 1
}
