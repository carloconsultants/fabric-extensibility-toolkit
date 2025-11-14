# PowerBI Tips Workload API Assessment Script
# This script helps assess the Workload API functionality

Write-Host "=== PowerBI Tips Workload API Assessment ===" -ForegroundColor Green
Write-Host ""

$baseUrl = "http://localhost:7071"
$testHeaders = @{
    'x-ms-client-principal' = '{"userId":"test-user","userRoles":["anonymous","authenticated"],"claims":[],"identityProvider":"staticwebapps","userDetails":"test@example.com"}'
    'Content-Type' = 'application/json'
}

Write-Host "Testing Workload API endpoints..." -ForegroundColor Yellow
Write-Host ""

# Test 1: Health Check
Write-Host "1. Health Check Test" -ForegroundColor Cyan
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/workload?workspaceId=health-check" -Headers $testHeaders -Method GET
    Write-Host "   ✅ Status: $($response.StatusCode)" -ForegroundColor Green
    $data = $response.Content | ConvertFrom-Json
    Write-Host "   📄 Response Type: $($data.GetType().Name)" -ForegroundColor Blue
}
catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    Write-Host "   ⚠️  Status: $statusCode" -ForegroundColor Yellow
    if ($statusCode -eq 503) {
        Write-Host "   💡 Feature flag is likely disabled" -ForegroundColor Blue
    }
}

Write-Host ""

# Test 2: Get Workload Info
Write-Host "2. Get Workload Info Test" -ForegroundColor Cyan
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/workload?workspaceId=test-workspace-123" -Headers $testHeaders -Method GET
    Write-Host "   ✅ Status: $($response.StatusCode)" -ForegroundColor Green
    $data = $response.Content | ConvertFrom-Json
    Write-Host "   📊 Workspace ID: $($data.workspaceId)" -ForegroundColor Blue
    Write-Host "   📈 Items Count: $($data.items.Count)" -ForegroundColor Blue
    Write-Host "   🔧 Status: $($data.metadata.status)" -ForegroundColor Blue
}
catch {
    Write-Host "   ❌ Failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""

# Test 3: Get Item Payload
Write-Host "3. Get Item Payload Test" -ForegroundColor Cyan
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/workload/workspaces/test-workspace-123/items/Report/test-report-456/payload" -Headers $testHeaders -Method GET
    Write-Host "   ✅ Status: $($response.StatusCode)" -ForegroundColor Green
    $data = $response.Content | ConvertFrom-Json
    Write-Host "   📄 Item Type: $($data.itemType)" -ForegroundColor Blue
    Write-Host "   🆔 Item ID: $($data.itemId)" -ForegroundColor Blue
    Write-Host "   📏 Size: $($data.size) bytes" -ForegroundColor Blue
}
catch {
    Write-Host "   ❌ Failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""

# Test 4: Update Item
Write-Host "4. Update Item Test" -ForegroundColor Cyan
$updateBody = @{
    displayName = "Updated Test Report"
    description = "Test update via PowerShell"
    properties = @{
        testProperty = "testValue"
        timestamp = Get-Date -Format "yyyy-MM-ddTHH:mm:ssZ"
    }
} | ConvertTo-Json

try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/workload/workspaces/test-workspace-123/items/Report/test-report-456" -Headers $testHeaders -Method PATCH -Body $updateBody
    Write-Host "   ✅ Status: $($response.StatusCode)" -ForegroundColor Green
    $data = $response.Content | ConvertFrom-Json
    Write-Host "   🎯 Success: $($data.success)" -ForegroundColor Blue
    Write-Host "   💬 Message: $($data.message)" -ForegroundColor Blue
}
catch {
    Write-Host "   ❌ Failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""

# Test 5: Function Registration Check
Write-Host "5. Function Registration Check" -ForegroundColor Cyan
try {
    $adminResponse = Invoke-WebRequest -Uri "$baseUrl/admin/functions" -Method GET
    $functions = $adminResponse.Content | ConvertFrom-Json
    $workloadFunctions = $functions | Where-Object { $_.name -like "*Workload*" }
    
    Write-Host "   📋 Total Functions: $($functions.Count)" -ForegroundColor Blue
    Write-Host "   🔧 Workload Functions: $($workloadFunctions.Count)" -ForegroundColor Blue
    
    foreach ($func in $workloadFunctions) {
        Write-Host "   • $($func.name)" -ForegroundColor Green
    }
}
catch {
    Write-Host "   ℹ️  Admin endpoint not accessible (expected in production)" -ForegroundColor Blue
}

Write-Host ""
Write-Host "=== Assessment Complete ===" -ForegroundColor Green
Write-Host ""
Write-Host "💡 Tips:" -ForegroundColor Yellow
Write-Host "   • If you see 503 errors, check UseNewWorkloadApi in local.settings.json"
Write-Host "   • If you see 401 errors, the authentication header is being validated"
Write-Host "   • 200 responses indicate the API is working correctly"
Write-Host "   • Use Bruno REST client for more detailed testing"
Write-Host ""