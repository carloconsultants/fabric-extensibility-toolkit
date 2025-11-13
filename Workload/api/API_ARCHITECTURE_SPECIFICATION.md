# Intelexos API Architecture Specification

## Overview

This document outlines the architectural patterns, design principles, and organizational structure of the Intelexos API. This specification serves as a guide for maintaining consistency across the codebase and can be referenced when requesting AI assistance for improvements and feature development.

## Table of Contents

1. [Framework & Technology Stack](#framework--technology-stack)
2. [Project Structure](#project-structure)
3. [Architectural Patterns](#architectural-patterns)
4. [Design Principles](#design-principles)
5. [Code Organization](#code-organization)
6. [Development Guidelines](#development-guidelines)
7. [Best Practices](#best-practices)

## Framework & Technology Stack

### Core Framework
- **.NET 8.0** - Target framework
- **Azure Functions v4** - Serverless compute platform
- **ASP.NET Core** - Web framework integration
- **C#** with nullable reference types enabled

### Key Dependencies
- **Microsoft.Azure.Functions.Worker** - Isolated worker process model
- **Microsoft.PowerBI.Api** - Power BI REST API client
- **Gremlin.Net** - Graph database connectivity
- **Azure.Security.KeyVault.Secrets** - Secure configuration management
- **Azure.Storage.Blobs** - File storage operations
- **Newtonsoft.Json** - JSON serialization
- **Swashbuckle.AspNetCore** - API documentation

## Project Structure

```
api/
├── Controllers/           # HTTP endpoint definitions
├── Services/             # Business logic layer
├── Core/                 # External service integrations
├── Models/               # Data transfer objects and entities
├── Middleware/           # Cross-cutting concerns
├── Utilities/            # Helper classes and extensions
├── DSL/                  # Domain-specific language extensions
├── Properties/           # Assembly metadata
├── Bruno/                # API testing suite
├── Program.cs            # Application entry point
├── host.json             # Azure Functions configuration
└── intelexos_api.csproj  # Project configuration
```

## Architectural Patterns

### 1. Layered Architecture

The API follows a traditional layered architecture:

```
┌─────────────────┐
│   Controllers   │ ← HTTP endpoints, routing, request/response handling
├─────────────────┤
│    Services     │ ← Business logic, orchestration, validation
├─────────────────┤
│      Core       │ ← External integrations (PowerBI, Azure, Graph DB)
├─────────────────┤
│     Models      │ ← Data structures, DTOs, domain entities
└─────────────────┘
```

### 2. Dependency Injection Pattern

All services are registered in `Program.cs` using the built-in DI container:

```csharp
// Core services
builder.Services.AddSingleton<IGraphDb, GraphDb>();
builder.Services.AddScoped<IPowerBIApi, PowerBIApi>();

// Business services  
builder.Services.AddScoped<IReportService, ReportService>();
```

### 3. Repository/Service Pattern

Each domain entity has corresponding service interfaces and implementations:
- Interface defines contract (`IReportService`)
- Implementation contains business logic (`ReportService`)
- Base classes provide common functionality (`ServiceBase<T>`)

### 4. Response Wrapper Pattern

All service methods return standardized responses:

```csharp
public class ServiceResponse<T>
{
    public HttpStatusCode Status { get; set; }
    public T? Data { get; set; }
    public string? ErrorMessage { get; set; }
}
```

## Design Principles

### 1. Separation of Concerns
- **Controllers**: Handle HTTP concerns only (routing, authentication, serialization)
- **Services**: Contain business logic and orchestration
- **Core**: Manage external service integrations
- **Models**: Define data structures and validation

### 2. Interface Segregation
- Services implement focused interfaces
- Base interfaces provide common operations (`IServiceBase<T>`)
- Specialized interfaces extend base functionality

### 3. Single Responsibility
- Each class has one reason to change
- Controllers delegate to services
- Services orchestrate core components

### 4. Dependency Inversion
- High-level modules depend on abstractions
- Interfaces define contracts between layers
- Implementation details are injected

## Code Organization

### Controllers Structure

Controllers are organized by domain and follow Azure Functions patterns:

```csharp
[Function("FunctionName")]
public async Task<IActionResult> MethodName(
    [HttpTrigger(AuthorizationLevel.Anonymous, "post", 
    Route = RouteConstants.RoutePattern)] HttpRequest req,
    string environmentId)
{
    // 1. Extract and validate parameters
    // 2. Delegate to service layer
    // 3. Return standardized response
}
```

**Naming Conventions:**
- Class: `{Domain}Functions` (e.g., `ReportFunctions`)
- Methods: Descriptive action names (e.g., `AddReport`, `GetReports`)
- Functions: Match method names for consistency

### Services Structure

Services inherit from `ServiceBase<T>` and implement domain-specific interfaces:

```csharp
public class ReportService : ServiceBase<Report>, IReportService
{
    // Dependency injection in constructor
    public ReportService(IGraphDb graphDb, IPowerBIApi powerBIApi) : base(graphDb)
    {
        _powerBIApi = powerBIApi;
    }

    // Business logic methods
    public async Task<ServiceResponse<Report>> CreateReport(...)
    {
        // Implementation
    }
}
```

**Key Characteristics:**
- Generic base class provides common CRUD operations
- Domain-specific methods extend base functionality
- All methods return `ServiceResponse<T>`
- Async/await pattern used throughout

### Core Integrations

Core classes handle external service communications:

```csharp
public class PowerBIApi : IPowerBIApi
{
    // External API methods
    public async Task<ServiceResponse<Export>> ExportToFile(ExportToFileRequest request)
    {
        // Handle external service calls
        // Map responses to internal models
        // Provide error handling
    }
}
```

### Models Organization

Models are organized by domain with clear responsibilities:

```csharp
// Domain entities
public class Report : IVertexObject
{
    public string? Id { get; set; }
    // Entity properties
}

// Request/Response DTOs
public class ExportReportRequest
{
    // Request parameters
}

// Constants and enums
public static class ReportConstants
{
    public const string ReportLabel = "report";
}
```

### Route Management

Routes are centralized in `RouteConstants.cs`:

```csharp
public static class RouteConstants
{
    public const string Reports = "environments/{environmentId}/reports";
    public const string Report = "environments/{environmentId}/reports/{reportId}";
}
```

## Development Guidelines

### 1. Error Handling

**Service Layer:**
```csharp
try
{
    // Business logic
    return ServiceResponse<T>.Success(data);
}
catch (Exception ex)
{
    return ServiceResponse<T>.Error(HttpStatusCode.InternalServerError, ex.Message);
}
```

**Controller Layer:**
```csharp
var result = await _service.Method();
if (result.Status != HttpStatusCode.OK)
{
    return new ObjectResult(result.ErrorMessage) 
    { 
        StatusCode = (int)result.Status 
    };
}
return new OkObjectResult(result.Data);
```

### 2. Authentication & Authorization

Use Static Web Apps authentication pattern:
```csharp
var userClaims = StaticWebAppsAuth.Parse(req);
string callerId = userClaims.UserId;
```

### 3. Logging

Leverage built-in ILogger:
```csharp
_logger.LogInformation("Processing {FunctionName} for user {UserId}", 
    nameof(AddReport), callerId);
```

### 4. Configuration

Use Azure Functions configuration patterns:
- `host.json` for function-level settings
- `local.settings.json` for development
- Key Vault for secrets

### 5. Validation

Validate at appropriate layers:
```csharp
// Controller validation
if (string.IsNullOrEmpty(environmentId))
    return new BadRequestObjectResult("Environment ID required");

// Service validation  
if (report.PbiReportId == null)
    return ServiceResponse<byte[]>.Error(HttpStatusCode.BadRequest, 
        "Report is not properly configured");
```

## Best Practices

### 1. Consistent Patterns

**Always follow established patterns:**
- Use `ServiceResponse<T>` for all service methods
- Implement interfaces for all services
- Use dependency injection for all dependencies
- Follow async/await patterns consistently

### 2. Error Messages

**Provide meaningful error messages:**
```csharp
// Good
return ServiceResponse<T>.Error(HttpStatusCode.NotFound, 
    $"Report {reportId} not found in environment {environmentId}");

// Avoid
return ServiceResponse<T>.Error(HttpStatusCode.NotFound, "Not found");
```

### 3. Resource Management

**Proper disposal of resources:**
```csharp
using var client = new HttpClient();
using var memoryStream = new MemoryStream();
```

### 4. Null Safety

**Leverage C# nullable reference types:**
```csharp
public string? OptionalProperty { get; set; }
public required string RequiredProperty { get; set; }
```

### 5. Performance Considerations

**Async operations:**
```csharp
// Use ConfigureAwait(false) in library code
var result = await _service.GetDataAsync().ConfigureAwait(false);

// Parallel operations when appropriate
var tasks = reports.Select(r => ProcessReportAsync(r));
await Task.WhenAll(tasks);
```

### 6. Testing Structure

**Maintain comprehensive testing:**
- Unit tests for service layer logic
- Integration tests for controller endpoints
- Bruno collection for API testing

### 7. Documentation

**Document complex business logic:**
```csharp
/// <summary>
/// Exports a Power BI report with RLS and filtering support.
/// Includes profile context for copied reports.
/// </summary>
/// <param name="callerId">User requesting the export</param>
/// <param name="environmentId">Target environment</param>
/// <param name="reportId">Report to export</param>
/// <returns>Exported file as byte array</returns>
```

## AI Assistance Guidelines

When requesting AI assistance for this codebase, reference this specification and:

1. **Maintain architectural consistency** - Follow established patterns
2. **Preserve existing abstractions** - Don't break service interfaces
3. **Follow naming conventions** - Use established patterns
4. **Include proper error handling** - Use ServiceResponse pattern
5. **Add appropriate logging** - Use ILogger throughout
6. **Update tests** - Maintain test coverage
7. **Consider performance** - Use async patterns appropriately
8. **Document changes** - Update relevant documentation

## Version History

- **v1.0** - Initial specification based on current codebase architecture
- Created: October 2025
- Last Updated: October 2025

---

*This specification is a living document that should be updated as the architecture evolves.*