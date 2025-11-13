#!/bin/bash

# PowerBI Tips Workload API Assessment Script (Bash/Curl version)
# This script helps assess the Workload API functionality using curl

echo "=== PowerBI Tips Workload API Assessment ==="
echo ""

BASE_URL="http://localhost:7071"
AUTH_HEADER='x-ms-client-principal: {"userId":"test-user","userRoles":["anonymous","authenticated"],"claims":[],"identityProvider":"staticwebapps","userDetails":"test@example.com"}'

echo "Testing Workload API endpoints..."
echo ""

# Test 1: Health Check
echo "1. Health Check Test"
response=$(curl -s -w "%{http_code}" -H "$AUTH_HEADER" -H "Content-Type: application/json" \
    "$BASE_URL/api/workload?workspaceId=health-check" -o /tmp/response.json)

if [ "$response" = "200" ]; then
    echo "   ✅ Status: $response"
    echo "   📄 Response: $(cat /tmp/response.json | jq -c .)"
elif [ "$response" = "503" ]; then
    echo "   ⚠️  Status: $response (Feature flag likely disabled)"
    echo "   💡 Response: $(cat /tmp/response.json | jq -c .)"
else
    echo "   ❌ Status: $response"
    echo "   📄 Response: $(cat /tmp/response.json)"
fi
echo ""

# Test 2: Get Workload Info
echo "2. Get Workload Info Test"
response=$(curl -s -w "%{http_code}" -H "$AUTH_HEADER" -H "Content-Type: application/json" \
    "$BASE_URL/api/workload?workspaceId=test-workspace-123" -o /tmp/response.json)

echo "   📊 Status: $response"
if [ "$response" = "200" ]; then
    echo "   📋 Workspace ID: $(cat /tmp/response.json | jq -r .workspaceId)"
    echo "   📈 Items Count: $(cat /tmp/response.json | jq -r '.items | length')"
    echo "   🔧 Status: $(cat /tmp/response.json | jq -r .metadata.status)"
else
    echo "   📄 Response: $(cat /tmp/response.json | jq -c . 2>/dev/null || cat /tmp/response.json)"
fi
echo ""

# Test 3: Get Item Payload
echo "3. Get Item Payload Test"
response=$(curl -s -w "%{http_code}" -H "$AUTH_HEADER" -H "Content-Type: application/json" \
    "$BASE_URL/api/workload/workspaces/test-workspace-123/items/Report/test-report-456/payload" -o /tmp/response.json)

echo "   📊 Status: $response"
if [ "$response" = "200" ]; then
    echo "   📄 Item Type: $(cat /tmp/response.json | jq -r .itemType)"
    echo "   🆔 Item ID: $(cat /tmp/response.json | jq -r .itemId)"
    echo "   📏 Size: $(cat /tmp/response.json | jq -r .size) bytes"
else
    echo "   📄 Response: $(cat /tmp/response.json | jq -c . 2>/dev/null || cat /tmp/response.json)"
fi
echo ""

# Test 4: Update Item
echo "4. Update Item Test"
update_data='{"displayName":"Updated Test Report","description":"Test update via curl","properties":{"testProperty":"testValue","timestamp":"'$(date -u +"%Y-%m-%dT%H:%M:%SZ")'"}}'

response=$(curl -s -w "%{http_code}" -X PATCH -H "$AUTH_HEADER" -H "Content-Type: application/json" \
    -d "$update_data" \
    "$BASE_URL/api/workload/workspaces/test-workspace-123/items/Report/test-report-456" -o /tmp/response.json)

echo "   📊 Status: $response"
if [ "$response" = "200" ]; then
    echo "   🎯 Success: $(cat /tmp/response.json | jq -r .success)"
    echo "   💬 Message: $(cat /tmp/response.json | jq -r .message)"
else
    echo "   📄 Response: $(cat /tmp/response.json | jq -c . 2>/dev/null || cat /tmp/response.json)"
fi
echo ""

# Clean up
rm -f /tmp/response.json

echo "=== Assessment Complete ==="
echo ""
echo "💡 Tips:"
echo "   • If you see 503 errors, check UseNewWorkloadApi in local.settings.json"
echo "   • If you see 401 errors, the authentication header is being validated"
echo "   • 200 responses indicate the API is working correctly"
echo "   • Use Bruno REST client for more detailed testing"
echo ""
echo "🔧 To enable the Workload API:"
echo "   Set 'UseNewWorkloadApi': true in api-dotnet/local.settings.json"
echo ""