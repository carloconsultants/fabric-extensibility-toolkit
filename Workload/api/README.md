# PBI.tips Workload API Template

## Overview
This is a .NET 8.0 Azure Functions API template for building Microsoft Fabric workloads, based on production patterns from PBI.tips and optimized for Azure Static Web Apps deployment.

## Architecture
This API follows a layered architecture with clean separation of concerns:

- **Controllers**: HTTP endpoint definitions with minimal logic
- **Services**: Business logic and orchestration
- **Core**: External service integrations (Azure Storage, Fabric APIs)
- **Models**: Entities, DTOs, enums, and constants
- **Utilities**: Helper classes and extensions

## Getting Started

### Prerequisites
- .NET 8.0 SDK
- Azure Functions Core Tools v4
- Azure Storage Emulator or Azure Storage Account
- Visual Studio Code or Visual Studio 2022

### Local Development

1. **Clone and Setup**
   ```bash
   cd api-dotnet
   dotnet restore
   ```

2. **Configure Local Settings**
   Copy `local.settings.example.json` to `local.settings.json` and update:
   ```json
   {
     "IsEncrypted": false,
     "Values": {
       "AzureWebJobsStorage": "your-storage-connection-string",
       "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
       "STORAGE_USER_TABLE": "users",
       "APP_ENVIRONMENT": "local"
     },
     "FeatureFlags": {
       "UseNewUserApi": true
     }
   }
   ```

3. **Build and Run**
   ```bash
   dotnet build
   func start
   ```

4. **Test Endpoints**
   ```bash
   # Get users (requires admin role)
   curl http://localhost:7071/api/users
   
   # Get user info
   curl http://localhost:7071/api/users/test-user
   ```

## Current Implementation Status

### ✅ Completed - User Domain
- **Models**: User entity, DTOs, enums, constants
- **Service**: Complete UserService with all business logic
- **Controller**: All user endpoints implemented
- **Features**:
  - Get users (admin only)
  - Get user info
  - Create user
  - Update user role
  - Update trial subscription
  - Post login events
  - User theme management
  - Published item management
  - Subscription management

### 🚧 Planned - Future Domains
- Theme Management
- Publishing System
- PowerBI Embedding
- PayPal Integration
- Workload Management
- Analytics & Tracking

## API Endpoints

### User Management
```
GET    /api/users              # Get all users (admin only)
GET    /api/users/{userId}     # Get user by ID
POST   /api/users              # Create new user
PUT    /api/users/{userId}/role           # Update user role
PUT    /api/users/{userId}/trial          # Update trial subscription
POST   /api/users/login-event             # Post login event
```

### Response Format
All endpoints return standardized responses:

```json
{
  "status": 200,
  "data": { ... },
  "errorMessage": null
}
```

### Error Responses
```json
{
  "status": 400,
  "data": null,
  "errorMessage": "Error description"
}
```

## Authentication & Authorization

### Static Web Apps Authentication
Uses Azure Static Web Apps authentication pattern:

```csharp
var clientPrincipal = StaticWebAppsAuth.Parse(req);
var userInfo = await _userService.GetUserAsync(clientPrincipal);
```

### Role-Based Authorization
- **Admin**: Full access to all operations
- **User**: Limited to own data and read-only operations

## Data Storage

### Azure Table Storage
Primary data store using Azure Table Storage:
- **Partition Key**: Environment (local, dev, prod)
- **Row Key**: User ID or entity identifier
- **Entities**: User, Theme, PublishedItem, etc.

### Complex Data Serialization
Complex objects stored as JSON strings in table properties:
- `SubscriptionJson`: PayPal subscription data
- `ThemesJson`: User themes array
- `PublishedJson`: Published items array

## Development Guidelines

### Adding New Endpoints

1. **Define Route Constant**
   ```csharp
   // Models/Constants/RouteConstants.cs
   public const string NewEndpoint = "new-endpoint/{id}";
   ```

2. **Create DTOs**
   ```csharp
   // Models/DTOs/Requests/NewRequest.cs
   public class NewRequest { ... }
   
   // Models/DTOs/Responses/NewResponse.cs  
   public class NewResponse { ... }
   ```

3. **Add Service Method**
   ```csharp
   // Services/Interfaces/IService.cs
   Task<ServiceResponse<NewResponse>> ProcessNewAsync(NewRequest request);
   
   // Services/Service.cs
   public async Task<ServiceResponse<NewResponse>> ProcessNewAsync(NewRequest request)
   {
       try
       {
           // Implementation
           return ServiceResponse<NewResponse>.Success(result);
       }
       catch (Exception ex)
       {
           _logger.LogError(ex, "Error processing new request");
           return ServiceResponse<NewResponse>.InternalServerError(ex.Message);
       }
   }
   ```

4. **Add Controller Method**
   ```csharp
   [Function("ProcessNew")]
   public async Task<HttpResponseData> ProcessNew(
       [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RouteConstants.NewEndpoint)] 
       HttpRequestData req, string id)
   {
       // Implementation following established pattern
   }
   ```

### Error Handling
Always use try-catch blocks and return appropriate ServiceResponse:

```csharp
try
{
    // Business logic
    return ServiceResponse<T>.Success(result);
}
catch (ArgumentException ex)
{
    return ServiceResponse<T>.BadRequest(ex.Message);
}
catch (UnauthorizedAccessException ex)
{
    return ServiceResponse<T>.Unauthorized(ex.Message);
}
catch (Exception ex)
{
    _logger.LogError(ex, "Unexpected error in {Method}", nameof(MethodName));
    return ServiceResponse<T>.InternalServerError("An unexpected error occurred");
}
```

### Logging
Use structured logging with appropriate log levels:

```csharp
_logger.LogInformation("Processing {Operation} for user {UserId}", operation, userId);
_logger.LogWarning("Invalid request received: {Error}", error);
_logger.LogError(ex, "Error in {Method} for user {UserId}", nameof(Method), userId);
```

## Testing

### Unit Tests (Planned)
```bash
dotnet test
```

### Integration Tests (Planned) 
Test against real Azure Storage and authentication.

### API Testing
Use REST client or Postman with the included collection.

## Deployment

### Azure Function App
Deploy using Azure Functions Core Tools:

```bash
func azure functionapp publish your-function-app-name
```

### Application Settings
Configure in Azure portal:
- Connection strings for storage
- Feature flag settings
- Environment variables

## Performance Monitoring

### Key Metrics
- **Response Times**: Target <500ms for all endpoints
- **Success Rates**: Target >99.5% success rate
- **Memory Usage**: Monitor for memory leaks
- **Cold Start Times**: Target <3 seconds

### Application Insights
Integrated for monitoring and diagnostics:
- Request tracking
- Exception logging
- Performance counters
- Custom telemetry

## Implementation Status

### Completed Features ✅
- [x] Project setup and infrastructure  
- [x] User management (all CRUD operations)
- [x] Theme management (save, retrieve, delete)
- [x] Authentication and authorization
- [x] File upload/download functionality
- [x] Legacy API compatibility endpoints
- [x] PowerBI embedding services

## Contributing

### Code Style
- Follow C# naming conventions
- Use async/await consistently
- Include XML documentation for public methods
- Maintain consistent error handling patterns

### Pull Request Process
1. Create feature branch from main
2. Implement changes following architecture patterns
3. Add appropriate tests
4. Update documentation
5. Submit PR with clear description

## Troubleshooting

### Common Issues

**Issue**: Function not starting locally
```bash
# Solution: Check .NET version and Azure Functions tools
dotnet --version
func --version
```

**Issue**: Azure Storage connection issues
```bash
# Solution: Start storage emulator or check connection string
azurite
```

**Issue**: Authentication not working
```bash
# Solution: Check Static Web Apps configuration
# Ensure x-ms-client-principal header is present
```

### Debug Mode
```bash
# Run with debug output
func start --verbose --debug
```

### Logs
Check local logs in:
- Console output during development
- Application Insights in Azure
- Azure Function App logs

## Support

For questions or issues:
1. Check this README and documentation
2. Review existing issues in repository
3. Create new issue with detailed description
4. Contact development team

---

**Last Updated**: October 23, 2025  
**Version**: 1.0  
**Status**: User Domain Complete - Ready for Testing