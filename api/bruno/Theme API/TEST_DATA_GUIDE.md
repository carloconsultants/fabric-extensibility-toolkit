# Theme API Tests - Data Configuration Guide

This collection contains comprehensive tests for the Theme API endpoints with proper test data management.

## 🔄 Test Data Strategy

### **Environment Variables (Local.bru)**
- `base_url`: API endpoint (http://localhost:7071/api)
- `auth_header`: Regular user authentication (test-user-123)
- `admin_auth_header`: Admin user authentication (admin-user-456)
- `created_theme_id`: Dynamically set by Save Theme test
- `test_theme_id`: Static theme ID for specific tests
- `non_existent_theme_id`: ID that doesn't exist (for 404 tests)

### **Test Execution Order**

**IMPORTANT**: Run tests in this sequence for proper data flow:

1. **Save Theme** → Creates `created_theme_id` variable
2. **Get Theme by ID** → Uses `created_theme_id` 
3. **Get User Themes** → Shows all themes for user
4. **Update Theme** → Modifies `created_theme_id`
5. **Delete Theme** → Removes `created_theme_id`

### **Authentication Patterns**

```javascript
// Regular User (test-user-123)
x-ms-client-principal: {{auth_header}}

// Admin User (admin-user-456) 
x-ms-client-principal: {{admin_auth_header}}

// No Auth (for testing unauthorized access)
// (remove x-ms-client-principal header)
```

### **⚠️ Authorization in Local Development**

**Important**: When `APP_ENVIRONMENT=local` (default in local.settings.json), authorization checks are bypassed for easier development. This means:

- ✅ `Get User Themes (Unauthorized).bru` will return **200 OK** (not 401)  
- ✅ `Get User Themes (No Auth Header).bru` will return **401 Unauthorized** (as expected)

In production (`APP_ENVIRONMENT=production`), all authorization rules are enforced normally.

### **Theme Request Format**

```json
{
  "id": "theme-{{$timestamp}}",
  "themeName": "Theme Name",
  "payload": {
    "dataColors": ["#1f77b4", "#ff7f0e"],
    "visualStyles": {
      "card": {"backgroundColor": "#ffffff"}
    }
  },
  "projectPayload": {...},  // Optional
  "projectImages": [...],   // Optional
  "scrimsFamilyName": "...", // Optional
  "isCommunity": false      // Optional
}
```

## 🧪 Test Categories

### **Success Path Tests**
- `Save Theme.bru` - Creates valid theme
- `Get Theme by ID.bru` - Retrieves created theme
- `Get User Themes.bru` - Lists user's themes
- `Save Project Theme.bru` - Creates project-specific theme

### **Error/Edge Case Tests**
- `Save Theme (Invalid Data).bru` - Tests validation
- `Save Theme (No Auth).bru` - Tests unauthorized access
- `Get Theme by ID (Not Found).bru` - Tests 404 response
- `Get User Themes (Unauthorized).bru` - Tests cross-user access
- `Delete Theme (Not Found).bru` - Tests deleting non-existent theme

## 🚀 Running Tests

### **Prerequisites**
1. ✅ .NET Azure Functions host running on port 7071
2. ✅ Azurite storage emulator running
3. ✅ Local environment selected in Bruno

### **Execution Steps**
1. **Individual Tests**: Run any test independently
2. **Full Collection**: Run entire collection (tests are designed to be independent)
3. **Sequence Testing**: For data flow testing, run Save → Get → Delete in order

### **Expected Results**
- **200 OK**: Save Theme, Get existing theme, Get User Themes
- **404 Not Found**: Get non-existent theme, Delete non-existent theme  
- **400 Bad Request**: Save with invalid data
- **401 Unauthorized**: Requests without proper auth headers

## 🔧 Troubleshooting

### **Common Issues**
- **404 errors**: Ensure .NET API (not TypeScript) is running
- **Invalid theme data**: Check request format matches API expectations
- **Connection refused**: Verify Functions host is running on port 7071
- **Auth errors**: Verify x-ms-client-principal headers are present

### **Test Data Reset**
To reset test data between runs:
1. Stop/restart Azurite storage emulator
2. Or manually delete themes via Delete Theme tests

## 📝 Variable Reference

| Variable | Purpose | Set By | Used By |
|----------|---------|--------|---------|
| `created_theme_id` | Dynamic theme ID | Save Theme test | Get, Update, Delete tests |
| `test_theme_id` | Static theme ID | Environment | Specific tests |
| `auth_header` | User auth | Environment | All authenticated tests |
| `admin_auth_header` | Admin auth | Environment | Admin-specific tests |
| `timestamp` | Unique IDs | Bruno built-in | Theme ID generation |

## 🎯 Test Data Examples

### **Valid Save Theme Request**
```json
{
  "id": "test-theme-{{$timestamp}}",
  "themeName": "My Theme",
  "payload": {
    "dataColors": ["#1f77b4", "#ff7f0e", "#2ca02c"],
    "visualStyles": {
      "card": {"backgroundColor": "#ffffff"},
      "table": {"gridColor": "#eeeeee"}
    }
  }
}
```

### **Invalid Save Theme Request** (for validation testing)
```json
{
  "id": "",
  "themeName": "",
  "payload": {
    "dataColors": "not_array",
    "visualStyles": "not_object"
  }
}
```