# Copilot Instructions for API Development

## Overview
This API follows a clean architecture pattern with clear separation of concerns. When building new features, follow these patterns and conventions.

## Directory Structure
- **Controllers/**: API endpoints and request/response handling
- **Services/**: Business logic and external service integrations
- **Models/**: Data transfer objects and domain models
- **Core/**: Core services like FabricApi, KeyVaultAccess, Configuration
- **Utilities/**: Shared utilities like ServiceResponse

## Key Patterns

### 1. Service Response Pattern
Always use `ServiceResponse<T>` for API responses:
```csharp
// Success
return ServiceResponse<MyModel>.Success(data);

// Error
return ServiceResponse<MyModel>.Error(HttpStatusCode.BadRequest, "Error message");
```

### 2. Controller Pattern
Controllers should be thin and delegate to services:
```csharp
[ApiController]
[Route("api/[controller]")]
public class MyController : ControllerBase
{
    private readonly IMyService _service;
    
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var result = await _service.GetDataAsync();
        return ServiceResponse.ServiceResponseToIActionResult(result);
    }
}
```

### 3. Service Pattern
Services contain business logic and use dependency injection:
```csharp
public interface IMyService
{
    Task<ServiceResponse<MyModel>> GetDataAsync();
}

public class MyService : IMyService
{
    private readonly ILogger<MyService> _logger;
    
    public MyService(ILogger<MyService> logger)
    {
        _logger = logger;
    }
    
    public async Task<ServiceResponse<MyModel>> GetDataAsync()
    {
        // Implementation
    }
}
```

### 4. Configuration
Use the `StorageConfig` class for environment variables:
```csharp
var tableName = StorageConfig.TableNames.MyTable;
var containerName = StorageConfig.BlobContainers.MyContainer;
```

### 5. Logging
Always use structured logging:
```csharp
_logger.LogInformation("Processing request for {UserId}", userId);
_logger.LogError(ex, "Error processing request for {UserId}", userId);
```

## Common Services Available
- `IAzureTableClient`: Azure Table Storage operations
- `IBlobStorageClient`: Azure Blob Storage operations
- `IKeyVaultAccess`: Azure Key Vault operations
- `IFabricApi`: Microsoft Fabric API operations

## Environment Variables
All configuration should be stored in environment variables and accessed through `StorageConfig` class. See `.env.dev`, `.env.test`, `.env.prod` files for examples.

## Error Handling
- Use try-catch blocks for external service calls
- Log errors with context
- Return appropriate HTTP status codes
- Use ServiceResponse pattern for consistent error handling

## Testing
- Write unit tests for services
- Use dependency injection for testability
- Mock external dependencies
- Test both success and error scenarios
