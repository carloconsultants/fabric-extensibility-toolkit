param (
    #Indicates if the files should be validated before building the package
    [boolean]$ValidateFiles = $false,
    [string]$Environment = "dev"
)

Write-Host "Building Nuget Package ..."

################################################
# Load environment variables from .env file
################################################
$envFile = Join-Path $PSScriptRoot "..\..\Workload\.env.$Environment"
if (-not (Test-Path $envFile)) {
    Write-Error "Environment file not found at $envFile. Please run SetupWorkload.ps1 first or specify a valid environment (dev, test, prod)."
    exit 1
}

# Parse .env file into hashtable
$envVars = @{}
Get-Content $envFile | ForEach-Object {
    if ($_ -match '^([^#=]+)=(.*)$') {
        $key = $matches[1].Trim()
        $value = $matches[2].Trim()
        $envVars[$key] = $value
    }
}

Write-Host "Loaded environment variables from $envFile ($Environment environment)"

################################################
# Copy template files to temp directory and replace variables
################################################
$templatePath = Join-Path $PSScriptRoot "..\..\Workload\Manifest"
$tempPath = Join-Path $PSScriptRoot "..\..\build\Manifest\temp"
$outputDir = Join-Path $PSScriptRoot "..\..\build\Manifest\"

# Ensure temp directory exists and is clean
if (Test-Path $tempPath) {
    Write-Host "Cleaning existing temp directory..."
    Remove-Item $tempPath -Recurse -Force    
}
New-Item -ItemType Directory -Path $tempPath -Force | Out-Null
$tempPath = Resolve-Path $tempPath


Write-Host "Copying template files from $templatePath to $tempPath"

# Copy all template files to temp directory
Copy-Item -Path "$templatePath\*" -Destination $tempPath -Recurse -Force

# Check if assets folder exists and report
$assetsPath = Join-Path $tempPath "assets"
if (Test-Path $assetsPath) {
    $assetFiles = Get-ChildItem -Path $assetsPath -Recurse -File
    Write-Host "Copied $($assetFiles.Count) asset files (images, etc.) without modification"
}

# Move all JSON and XML files from items subdirectories to root temp directory
$itemsPath = Join-Path $tempPath "items"
if (Test-Path $itemsPath) {
    $itemFiles = Get-ChildItem -Path $itemsPath -Recurse -Include "*.json", "*.xml" | Where-Object { $_.FullName -notlike "*\ItemDefinition\*" }
    
    if ($itemFiles.Count -gt 0) {
        Write-Host "Moving $($itemFiles.Count) item configuration files to root directory..."
        
        foreach ($itemFile in $itemFiles) {
            $destinationPath = Join-Path $tempPath $itemFile.Name
            
            # Handle duplicate names by adding item folder name as prefix
            if (Test-Path $destinationPath) {
                $itemFolderName = Split-Path (Split-Path $itemFile.FullName -Parent) -Leaf
                $destinationPath = Join-Path $tempPath "$itemFolderName$($itemFile.Name)"
                Write-Host "  Renaming $($itemFile.Name) to $itemFolderName$($itemFile.Name) to avoid conflicts"
            }
            
            Move-Item -Path $itemFile.FullName -Destination $destinationPath
            Write-Host "  Moved $($itemFile.Name) to root directory"
        }
    }
}

# Process all XML, JSON, and nuspec files to replace placeholders
# Exclude assets folder and binary files from variable replacement
$filesToProcess = Get-ChildItem -Path $tempPath -Recurse -Include "*.xml", "*.json", "*.nuspec" | Where-Object { $_.FullName -notlike "*\assets\*" }

Write-Host "Processing $($filesToProcess.Count) files for variable replacement..."
Write-Host "Assets folder and binary files will be copied without modification."

foreach ($file in $filesToProcess) {
    $content = Get-Content $file.FullName -Raw -Encoding UTF8
    $originalContent = $content
    
    # Replace environment variables with actual values
    foreach ($key in $envVars.Keys) {
        $placeholder = "{{$key}}"
        if ($content -match [regex]::Escape($placeholder)) {
            $content = $content -replace [regex]::Escape($placeholder), $envVars[$key]
            Write-Host "  Replaced $placeholder with $($envVars[$key]) in $($file.Name)"
        }
    }
    
    # Additional common placeholders that might use different naming
    $content = $content -replace '\{\{WORKLOAD_ID\}\}', $envVars['WORKLOAD_NAME']
    
    # Replace MANIFEST_FOLDER with the current temp manifest folder path
    if ($content -match [regex]::Escape('{{MANIFEST_FOLDER}}')) {
        $content = $content -replace '\{\{MANIFEST_FOLDER\}\}', $tempPath
        Write-Host "  Replaced {{MANIFEST_FOLDER}} with $tempPath in $($file.Name)"
    }
    
    # Only write if content changed
    if ($content -ne $originalContent) {
        Set-Content -Path $file.FullName -Value $content -Encoding UTF8
    }
}

################################################
# Validate processed files if requested
################################################
if($ValidateFiles -eq $true) {
    Write-Output "Validating processed configuration files..."
    $ScriptsDir = Join-Path $PSScriptRoot "Manifest\ValidationScripts"

    & "$ScriptsDir\RemoveErrorFile.ps1" -outputDirectory $ScriptsDir
    & "$ScriptsDir\ManifestValidator.ps1" -inputDirectory $tempPath -inputXml "WorkloadManifest.xml" -inputXsd "WorkloadDefinition.xsd" -outputDirectory $ScriptsDir
    & "$ScriptsDir\ItemManifestValidator.ps1" -inputDirectory $tempPath -inputXsd "ItemDefinition.xsd" -workloadManifest "WorkloadManifest.xml" -outputDirectory $ScriptsDir

    $validationErrorFile = Join-Path $ScriptsDir "ValidationErrors.txt"
    if (Test-Path $validationErrorFile) {
        Write-Host "Validation errors found. See $validationErrorFile"
        Get-Content $validationErrorFile | Write-Host
        exit 1
    }
    Write-Host "Validation completed successfully" -ForegroundColor Green
}

################################################
# Build the current nuget package
################################################
# The nuget-bin package uses nuget.exe which requires mono on Linux
# But provides cross-platform support through the Node.js wrapper
$nugetBinPath = Join-Path $PSScriptRoot "..\..\Workload\node_modules\.bin\nuget"
$nugetExePath = Join-Path $PSScriptRoot "..\..\Workload\node_modules\nuget-bin\nuget.exe"
$nuspecPath = Join-Path $tempPath "\ManifestPackage.nuspec"



Write-Host "Using configuration in $outputDir"

if (-not (Test-Path $nugetExePath)) {
    Write-Host "Nuget executable not found at $nugetExePath will run npm install to get it."
    $workloadDir = Join-Path $PSScriptRoot "..\..\Workload"
    try {
        Push-Location $workloadDir
        npm install
    } finally {
        Pop-Location
    }
}

# On Windows, use nuget.exe directly; on Linux/Mac use mono with nuget.exe
if($IsWindows){
    & $nugetExePath pack $nuspecPath -OutputDirectory $outputDir -Verbosity detailed
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ NuGet pack failed with exit code $LASTEXITCODE" -ForegroundColor Red
        exit 1
    }
} else {
    # On Linux/Mac, use mono to run nuget.exe
    # Note: Mono 6.8 has a known crash bug after successful packaging
    # We verify success by checking if the .nupkg file was created
    Write-Host "Using mono to run NuGet.exe..." -ForegroundColor Yellow
    
    $monoOutput = & mono $nugetExePath pack $nuspecPath -OutputDirectory $outputDir -Verbosity quiet 2>&1
    
    # Check if package was created (Mono crashes after success, so exit code is unreliable)
    $packageFiles = Get-ChildItem -Path $outputDir -Filter "*.nupkg" -ErrorAction SilentlyContinue
    if ($packageFiles) {
        Write-Host "✅ NuGet package created successfully" -ForegroundColor Green
    } else {
        Write-Host "❌ NuGet package creation failed" -ForegroundColor Red
        Write-Host $monoOutput
        exit 1
    }
}

Write-Host "✅ Created the new ManifestPackage in $outputDir." -ForegroundColor Blue

