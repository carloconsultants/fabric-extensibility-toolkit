# PowerBI.tips API Redesign Proposal [COMPLETED]

> **NOTE**: This document is now historical. The migration from TypeScript to .NET has been completed successfully.

## Overview

This document outlined the redesign of the PowerBI.tips API to align with the Intelexos API Architecture Specification. The redesign migrated from the TypeScript/Node.js function-per-folder approach to a modern .NET 8.0 layered architecture.

## Current State Analysis

### Technology Stack

- **Current**: TypeScript/Node.js Azure Functions v3
- **Proposed**: C#/.NET 8.0 Azure Functions v4 (Isolated Worker Model)

### Current Structure Issues

1. **Function-per-folder approach** - Each endpoint is a separate folder with individual files
2. **No clear separation of concerns** - Business logic mixed with HTTP handling
3. **Inconsistent error handling** - Different patterns across functions
4. **Limited dependency injection** - Manual service instantiation
5. **Scattered domain logic** - Related functionality spread across multiple folders

### Business Domains Identified

1. **User Management** - Authentication, roles, subscriptions, trial management
2. **Theme Management** - PowerBI themes, palettes, layouts, visual styles
3. **Publishing System** - Shared content publishing and discovery
4. **PayPal Integration** - Subscription management and billing
5. **PowerBI Embedding** - Report embedding, access tokens, workspace management
6. **Azure Subscription** - Azure resource management and OBO flows
7. **Workload Management** - Custom workload operations and item management
8. **Analytics & Tracking** - Google Analytics integration and usage tracking

## Proposed Architecture

### Directory Structure

```
api/
├── Controllers/                    # HTTP endpoint definitions
│   ├── UserController.cs          # User management endpoints
│   ├── ThemeController.cs         # Theme CRUD operations
│   ├── PublishController.cs       # Publishing operations
│   ├── EmbedController.cs         # PowerBI embedding
│   ├── PayPalController.cs        # PayPal subscription management
│   ├── WorkloadController.cs      # Workload operations
│   ├── AnalyticsController.cs     # Analytics and tracking
│   └── AzureController.cs         # Azure subscription management
│
├── Services/                      # Business logic layer
│   ├── Interfaces/
│   │   ├── IUserService.cs
│   │   ├── IThemeService.cs
│   │   ├── IPublishService.cs
│   │   ├── IEmbedService.cs
│   │   ├── IPayPalService.cs
│   │   ├── IWorkloadService.cs
│   │   ├── IAnalyticsService.cs
│   │   └── IAzureSubscriptionService.cs
│   ├── UserService.cs
│   ├── ThemeService.cs
│   ├── PublishService.cs
│   ├── EmbedService.cs
│   ├── PayPalService.cs
│   ├── WorkloadService.cs
│   ├── AnalyticsService.cs
│   ├── AzureSubscriptionService.cs
│   └── Common/
│       ├── ServiceBase.cs         # Base service functionality
│       └── ServiceResponse.cs     # Standardized response wrapper
│
├── Core/                         # External service integrations
│   ├── Interfaces/
│   │   ├── IPowerBIApi.cs
│   │   ├── IPayPalApi.cs
│   │   ├── IAzureTableStorage.cs
│   │   ├── IAzureBlobStorage.cs
│   │   ├── IAzureSubscriptionApi.cs
│   │   └── IGoogleAnalyticsApi.cs
│   ├── PowerBIApi.cs             # PowerBI REST API integration
│   ├── PayPalApi.cs              # PayPal API integration
│   ├── AzureTableStorage.cs      # Azure Table Storage operations
│   ├── AzureBlobStorage.cs       # Azure Blob Storage operations
│   ├── AzureSubscriptionApi.cs   # Azure subscription management
│   └── GoogleAnalyticsApi.cs     # Google Analytics integration
│
├── Models/                       # Data transfer objects and entities
│   ├── Entities/                 # Domain entities
│   │   ├── User.cs
│   │   ├── Theme.cs
│   │   ├── PublishedItem.cs
│   │   ├── Subscription.cs
│   │   ├── EmbedToken.cs
│   │   ├── Workload.cs
│   │   └── AnalyticsEvent.cs
│   ├── DTOs/                     # Request/Response objects
│   │   ├── Requests/
│   │   │   ├── User/
│   │   │   │   ├── CreateUserRequest.cs
│   │   │   │   ├── UpdateUserRoleRequest.cs
│   │   │   │   └── UpdateTrialRequest.cs
│   │   │   ├── Theme/
│   │   │   │   ├── SaveThemeRequest.cs
│   │   │   │   └── DeleteThemeRequest.cs
│   │   │   ├── Publish/
│   │   │   │   ├── PublishItemRequest.cs
│   │   │   │   └── DeletePublishedItemRequest.cs
│   │   │   ├── Embed/
│   │   │   │   ├── EmbedAccessRequest.cs
│   │   │   │   └── ReportInfoRequest.cs
│   │   │   ├── PayPal/
│   │   │   │   └── SubscriptionRequest.cs
│   │   │   ├── Workload/
│   │   │   │   ├── WorkloadItemRequest.cs
│   │   │   │   └── WorkloadUpdateRequest.cs
│   │   │   └── Analytics/
│   │   │       └── AnalyticsEventRequest.cs
│   │   └── Responses/
│   │       ├── UserResponse.cs
│   │       ├── ThemeResponse.cs
│   │       ├── PublishedItemResponse.cs
│   │       ├── EmbedTokenResponse.cs
│   │       ├── PayPalSubscriptionResponse.cs
│   │       ├── WorkloadResponse.cs
│   │       └── AnalyticsResponse.cs
│   ├── Constants/
│   │   ├── RouteConstants.cs     # Centralized route definitions
│   │   ├── ThemeConstants.cs
│   │   ├── UserConstants.cs
│   │   ├── PayPalConstants.cs
│   │   ├── WorkloadConstants.cs
│   │   └── ErrorConstants.cs
│   └── Enums/
│       ├── UserRole.cs
│       ├── ThemeType.cs
│       ├── SubscriptionStatus.cs
│       ├── PublishedItemType.cs
│       ├── WorkloadItemType.cs
│       └── AnalyticsEventType.cs
│
├── Middleware/                   # Cross-cutting concerns
│   ├── AuthenticationMiddleware.cs
│   ├── LoggingMiddleware.cs
│   ├── ErrorHandlingMiddleware.cs
│   └── CorsMiddleware.cs
│
├── Utilities/                    # Helper classes and extensions
│   ├── Extensions/
│   │   ├── HttpRequestExtensions.cs
│   │   ├── StringExtensions.cs
│   │   ├── DateTimeExtensions.cs
│   │   └── ServiceCollectionExtensions.cs
│   ├── Helpers/
│   │   ├── StaticWebAppsAuth.cs  # SWA authentication helper
│   │   ├── JsonHelpers.cs
│   │   ├── ValidationHelpers.cs
│   │   ├── FileHelpers.cs
│   │   └── EncryptionHelpers.cs
│   └── Mappers/
│       ├── UserMapper.cs
│       ├── ThemeMapper.cs
│       ├── PublishedItemMapper.cs
│       ├── PayPalMapper.cs
│       └── WorkloadMapper.cs
│
├── Properties/                   # Assembly metadata
│   └── AssemblyInfo.cs
│
├── Program.cs                    # Application entry point & DI configuration
├── host.json                     # Azure Functions configuration
└── PowerBITips.Api.csproj       # Project configuration
```

## Domain-Specific API Endpoints

### User Management Domain (`UserController.cs`)

```
GET    /api/users                    # Get all users (admin only)
GET    /api/users/{userId}           # Get user info
POST   /api/users                   # Create/update user
PUT    /api/users/{userId}/role     # Update user role
POST   /api/users/login-event       # Track login event

### Admin User Management Extensions (`AdminController.cs`)

PATCH  /api/admin/users/{userId}/trial  # Update trial subscription (admin only)

> NOTE: Trial subscription update endpoint migrated from user scope (`PUT /api/users/{userId}/trial`) to admin scope (`PATCH /api/admin/users/{userId}/trial`) to enforce role-based authorization and align with principle of privileged operations residing under admin routes.
```

**Current Functions Mapping (Updated):**

- `get-users/` → `GET /api/users`
- `user-info/` → `GET /api/users/{userId}`
- `update-user-role/` → `PUT /api/users/{userId}/role`
- `update-user-trial-subscription/` → `PATCH /api/admin/users/{userId}/trial`
- `post-login-event/` → `POST /api/users/login-event`

### Theme Management Domain (`ThemeController.cs`)

```
GET    /api/themes                  # Get user themes
POST   /api/themes                  # Save theme
DELETE /api/themes/{themeId}        # Delete theme
GET    /api/themes/{themeId}/download # Download theme file
```

**Current Functions Mapping:**

- `get-user-themes/` → `GET /api/themes`
- `get-user-theme/` → `GET /api/themes/{themeId}`
- `save-theme/` → `POST /api/themes`
- `delete-user-theme/` → `DELETE /api/themes/{themeId}`
- `download-theme/` → `GET /api/themes/{themeId}/download`

### Publishing Domain (`PublishController.cs`)

```
GET    /api/published-items         # Get published items
POST   /api/published-items         # Publish item
DELETE /api/published-items/{itemId} # Delete published item
GET    /api/published-layouts       # Get published layouts
GET    /api/published-items/table   # Get items from table
GET    /api/published-items/{itemId}/download # Download published item
```

**Current Functions Mapping:**

- `get-published-items/` → `GET /api/published-items`
- `publish-item/` → `POST /api/published-items`
- `delete-published-item/` → `DELETE /api/published-items/{itemId}`
- `get-published-layouts/` → `GET /api/published-layouts`
- `get-published-items-from-table/` → `GET /api/published-items/table`
- `download-published-item/` → `GET /api/published-items/{itemId}/download`

### PowerBI Embedding Domain (`EmbedController.cs`)

```
POST   /api/embed/access            # Get embed access token
GET    /api/embed/reports/{reportId} # Get report info
POST   /api/embed/user-obo-exchange # User OBO token exchange
POST   /api/embed/onelake-obo-exchange # OneLake OBO token exchange
```

**Current Functions Mapping:**

- `embed-access/` → `POST /api/embed/access`
- `user-obo-exchange/` → `POST /api/embed/user-obo-exchange`
- `user-onelake-obo-exchange/` → `POST /api/embed/onelake-obo-exchange`

### PayPal Integration Domain (`PayPalController.cs`)

```
GET    /api/paypal/subscription-plans # Get subscription plans
POST   /api/paypal/subscription     # Create/manage subscription
```

**Current Functions Mapping:**

- `paypal-subscription-plans/` → `GET /api/paypal/subscription-plans`
- `paypal-subscription/` → `POST /api/paypal/subscription`

### Workload Management Domain (`WorkloadController.cs`)

```
GET    /api/workload               # Get workload info
GET    /api/workload/items/{itemId}/payload # Get item payload
PUT    /api/workload/items/{itemId} # Update workload item
```

**Current Functions Mapping:**

- `workload/` → `GET /api/workload`
- `workload-get-item-payload/` → `GET /api/workload/items/{itemId}/payload`
- `workload-item-update/` → `PUT /api/workload/items/{itemId}`

### Analytics Domain (`AnalyticsController.cs`)

```
POST   /api/analytics/google       # Post Google Analytics event
GET    /api/analytics/youtube/{videoId} # Get YouTube link
```

**Current Functions Mapping:**

- `post-google-analytics/` → `POST /api/analytics/google`
- `get-youtube-link/` → `GET /api/analytics/youtube/{videoId}`

### Resource Management Domain (`ResourceController.cs`)

```
GET    /api/resources/shared       # Get shared resources
POST   /api/resources/data/download # Download data
POST   /api/resources/preview/{itemId} # Update preview image
```

**Current Functions Mapping:**

- `get-shared-resources/` → `GET /api/resources/shared`
- `download-data/` → `POST /api/resources/data/download`
- `update-preview-image/` → `POST /api/resources/preview/{itemId}`

### Azure Subscription Domain (`AzureController.cs`)

```
GET    /api/azure/subscriptions    # Get Azure subscriptions
POST   /api/azure/subscriptions    # Manage Azure subscription
```

**Current Functions Mapping:**

- Functions using `azureSubscription.service.ts` → Azure subscription endpoints

## Service Layer Architecture

### Base Service Pattern

All services inherit from `ServiceBase<T>` and return standardized responses:

```csharp
public class ServiceResponse<T>
{
    public HttpStatusCode Status { get; set; }
    public T? Data { get; set; }
    public string? ErrorMessage { get; set; }
    public bool IsSuccess => Status == HttpStatusCode.OK;

    public static ServiceResponse<T> Success(T data) =>
        new() { Status = HttpStatusCode.OK, Data = data };

    public static ServiceResponse<T> Error(HttpStatusCode status, string message) =>
        new() { Status = status, ErrorMessage = message };
}
```

### Service Implementation Example

```csharp
public class UserService : ServiceBase<User>, IUserService
{
    private readonly IAzureTableStorage _tableStorage;
    private readonly IAzureBlobStorage _blobStorage;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IAzureTableStorage tableStorage,
        IAzureBlobStorage blobStorage,
        ILogger<UserService> logger) : base(tableStorage)
    {
        _tableStorage = tableStorage;
        _blobStorage = blobStorage;
        _logger = logger;
    }

    public async Task<ServiceResponse<User>> GetUserAsync(string userId)
    {
        try
        {
            var user = await _tableStorage.GetEntityAsync<User>(
                UserConstants.TableName,
                UserConstants.PartitionKey,
                userId);

            return ServiceResponse<User>.Success(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user {UserId}", userId);
            return ServiceResponse<User>.Error(
                HttpStatusCode.InternalServerError,
                $"Error retrieving user: {ex.Message}");
        }
    }
}
```

## Migration Strategy

### Phase 1: Foundation Setup (Week 1-2)

1. **Project Structure Creation**

   - Create new .NET 8.0 Azure Functions project
   - Set up folder structure and base classes
   - Configure dependency injection in `Program.cs`
   - Implement `ServiceResponse<T>` pattern

2. **Core Infrastructure**
   - Migrate Azure Table Storage client
   - Migrate Azure Blob Storage client
   - Create base service classes
   - Set up logging and error handling

### Phase 2: User Domain Migration (Week 2-3)

1. **User Management**
   - Migrate `UserService` from TypeScript to C#
   - Create `UserController` with all user endpoints
   - Implement authentication middleware
   - Test user CRUD operations

### Phase 3: Theme Domain Migration (Week 3-4)

1. **Theme Management**
   - Migrate `ThemeService` functionality
   - Create `ThemeController` with theme endpoints
   - Handle theme file uploads and downloads
   - Test theme operations

### Phase 4: Publishing & Embedding (Week 4-5)

1. **Publishing System**

   - Migrate publishing functionality
   - Create `PublishController`
   - Test published item operations

2. **PowerBI Embedding**
   - Migrate PowerBI service integration
   - Create `EmbedController`
   - Test embedding functionality

### Phase 5: External Integrations (Week 5-6)

1. **PayPal Integration**

   - Migrate PayPal service
   - Create `PayPalController`
   - Test subscription management

2. **Workload & Analytics**
   - Migrate workload operations
   - Migrate analytics functionality
   - Test all integrations

### Phase 6: Testing & Deployment (Week 6-7)

1. **Comprehensive Testing**

   - Unit tests for all services
   - Integration tests for controllers
   - End-to-end testing

2. **Performance Optimization**
   - Optimize database queries
   - Implement caching where appropriate
   - Monitor performance metrics

## Benefits of This Architecture

### 1. **Maintainability**

- Clear separation of concerns across layers
- Consistent patterns and conventions
- Centralized error handling and logging
- Easy to locate and modify functionality

### 2. **Testability**

- Dependency injection enables easy mocking
- Service layer can be tested in isolation
- Controllers have minimal logic to test
- Standardized response patterns

### 3. **Scalability**

- Services can be easily extended
- New domains can follow established patterns
- Horizontal scaling through stateless design
- Efficient resource utilization

### 4. **Developer Experience**

- IntelliSense and compile-time checking
- Consistent API contracts through interfaces
- Self-documenting code with clear structure
- Easier onboarding for new developers

### 5. **Performance**

- .NET 8.0 performance improvements
- Efficient async/await patterns
- Optimized Azure Functions runtime
- Better memory management

## Configuration Requirements

### Environment Variables

```json
{
  "STORAGE_USER_TABLE": "users",
  "STORAGE_THEME_TABLE": "themes",
  "STORAGE_PUBLISHED_TABLE": "published",
  "AZURE_STORAGE_CONNECTION_STRING": "...",
  "POWERBI_CLIENT_ID": "...",
  "POWERBI_CLIENT_SECRET": "...",
  "PAYPAL_CLIENT_ID": "...",
  "PAYPAL_CLIENT_SECRET": "...",
  "GOOGLE_ANALYTICS_MEASUREMENT_ID": "..."
}
```

### NuGet Packages Required

```xml
<PackageReference Include="Microsoft.Azure.Functions.Worker" Version="1.19.0" />
<PackageReference Include="Microsoft.Azure.Functions.Worker.Sdk" Version="1.16.4" />
<PackageReference Include="Microsoft.Azure.Functions.Worker.Extensions.Http" Version="3.0.13" />
<PackageReference Include="Azure.Data.Tables" Version="12.8.3" />
<PackageReference Include="Azure.Storage.Blobs" Version="12.19.1" />
<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Logging" Version="8.0.0" />
<PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
```

## Risk Mitigation

### 1. **Parallel Development**

- Keep existing TypeScript API running during migration
- Gradual endpoint migration with feature flags
- Blue-green deployment strategy

### 2. **Data Compatibility**

- Ensure data model compatibility
- Test migration scripts thoroughly
- Maintain backward compatibility during transition

### 3. **Testing Coverage**

- Comprehensive test suite before migration
- Automated testing pipeline
- Performance benchmarking

### 4. **Rollback Strategy**

- Maintain ability to rollback to TypeScript version
- Database versioning and migration scripts
- Monitoring and alerting for issues

## Success Metrics

1. **Performance**: 50% reduction in cold start times
2. **Maintainability**: 75% reduction in code duplication
3. **Development Speed**: 40% faster feature development
4. **Error Rates**: 60% reduction in runtime errors
5. **Test Coverage**: Maintain 90%+ test coverage

## Conclusion

This redesign will modernize the PowerBI.tips API architecture while maintaining all existing functionality. The migration to .NET 8.0 with a proper layered architecture will provide significant benefits in terms of maintainability, performance, and developer experience.

The phased approach ensures a smooth transition with minimal risk, while the standardized patterns will make future development more efficient and consistent.

---

_Document Version: 1.0_  
_Created: October 23, 2025_  
_Last Updated: October 23, 2025_
