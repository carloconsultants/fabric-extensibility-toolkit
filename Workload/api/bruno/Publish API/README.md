# Publish API Bruno Tests

## Overview
The Publish API allows users to publish themes, layouts, projects, and scrims to make them available to other users.

## Important: Delete Operation ID
When you publish an item, the API generates a unique `PublishedGuid` that is different from your original `ItemId`. 

**To delete a published item, you must use the `PublishedGuid` returned from the publish operation, NOT the original `ItemId`.**

## Test Workflow

### 1. Publish an Item (Create or Update)
The same endpoint handles both creating new published items and updating existing ones.

**Create New:**
```json
POST /api/publish-item
{
  "ItemName": "Beautiful Blue Theme",
  "ItemId": "test-theme-123",           // Your original ID
  "PublishType": 0,                     // 0=Theme, 1=Layout, 2=Project, 3=Scrims
  "UserThemeId": "test-theme-123",
  "PreviewImage": "https://example.com/preview.png"
}
```

**Update Existing:** Use the same request with the same `ItemId` - the API will find and update the existing published item instead of creating a new one.

**Create Response:**
```json
{
  "success": true,
  "message": "Item published successfully",
  "publishedGuid": "a1b2c3d4-e5f6-7890-1234-567890abcdef"  // Use THIS for delete
}
```

**Update Response:**
```json
{
  "success": true,
  "message": "Item updated successfully",
  "publishedGuid": "a1b2c3d4-e5f6-7890-1234-567890abcdef"  // Same GUID as original
}
```

### 2. Delete the Published Item
```json
DELETE /api/published-item
{
  "ItemType": 0,
  "ItemId": "a1b2c3d4-e5f6-7890-1234-567890abcdef"  // Use the publishedGuid here!
}
```

## Bruno Test Flow
1. **Run "01 - Publish Theme Item"** - This saves the returned `publishedGuid` to the `published_theme_guid` variable
2. **Run "09 - Delete Published Item"** - This uses the saved `published_theme_guid` to delete the item

## Item Types
- `0` = Theme
- `1` = Layout  
- `2` = Project
- `3` = Scrims

## Notes
- The publish operation creates a new database record with the `publishedGuid` as the primary key
- The original `ItemId` is stored in the `ItemLink` field but is NOT used for delete operations
- Each published item has ownership validation - only the creator can delete their published items