# Theme API Test Collection

This Bruno test collection provides comprehensive testing for the Theme API endpoints in the PowerBI.tips application.

## Test Structure

The tests are organized in the following sequence:

1. **Get User Themes** - Retrieve all themes for a user
2. **Get Theme by ID** - Retrieve a specific theme
3. **Get Theme by ID (Not Found)** - Test 404 scenarios
4. **Save Theme** - Create a new theme
5. **Update Theme** - Update an existing theme
6. **Delete Theme** - Delete a theme
7. **Delete Theme (Not Found)** - Test deletion of non-existent theme
8. **Get User Themes (Unauthorized)** - Test authorization
9. **Save Theme (Invalid Data)** - Test validation
10. **Save Project Theme** - Test different theme types
11. **Save Theme (No Auth)** - Test authentication

## Environment Variables

The tests use the following variables from the Local environment:

- `base_url`: http://localhost:7071/api
- `auth_header`: Base64 encoded authentication header
- `test_user_id`: User ID for testing
- `admin_user_id`: Admin user ID for authorization tests
- `test_theme_id`: Existing theme ID for testing
- `non_existent_theme_id`: Non-existent theme ID for 404 tests
- `created_theme_id`: Dynamically set during Save Theme test

## Theme Data Structure

The tests validate the following theme structure:

```json
{
  "id": "string",
  "name": "string", 
  "type": 0, // 0=Theme, 1=Project, 2=Palette, 3=Scrims, 4=Layout
  "isDefault": false,
  "dataColors": ["#1f77b4", "#ff7f0e", ...],
  "visualStyles": {
    "card": { "backgroundColor": "#ffffff", ... },
    "table": { "gridColor": "#eeeeee", ... },
    "chart": { "axisColor": "#666666", ... }
  },
  "createdDate": "2023-10-23T...",
  "modifiedDate": "2023-10-23T...",
  "userId": "string"
}
```

## Test Coverage

### Happy Path Tests
- ✅ Get user themes with proper authentication
- ✅ Get specific theme by ID
- ✅ Create new theme with valid data
- ✅ Update existing theme
- ✅ Delete theme successfully

### Error Scenarios
- ✅ Theme not found (404)
- ✅ Unauthorized access (401) 
- ✅ Invalid theme data (400)
- ✅ Missing authentication (401)

### Theme Types
- ✅ Standard Theme (type 0)
- ✅ Project Theme (type 1)
- ✅ Complex visual styles validation

## Running the Tests

1. Start the Azure Functions host:
   ```bash
   cd api-dotnet
   func host start
   ```

2. Ensure Azurite is running for local storage:
   ```bash
   azurite --location ./azurite
   ```

3. Run the tests in Bruno by selecting the Theme API collection

## Authentication

Tests use a Base64 encoded authentication header that simulates the Static Web Apps authentication:

```json
{
  "userId": "test-user-123",
  "userRoles": "User", 
  "identityProvider": "aad",
  "userDetails": "test-user-123"
}
```

## Notes

- Tests are designed to run sequentially due to dependencies
- The Save Theme test stores the created theme ID for use in subsequent tests
- Authorization tests verify that users can only access their own themes
- Invalid data tests ensure proper validation is enforced