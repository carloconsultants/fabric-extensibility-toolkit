using Microsoft.AspNetCore.Mvc;
using TemplateWorkload.Utilities;
using TemplateWorkload.Services;
using Microsoft.Extensions.Logging;

namespace TemplateWorkload.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExampleController : ControllerBase
    {
        private readonly ILogger<ExampleController> _logger;
        private readonly IAzureTableClient _tableClient;
        private readonly IBlobStorageClient _blobClient;

        public ExampleController(
            ILogger<ExampleController> logger,
            IAzureTableClient tableClient,
            IBlobStorageClient blobClient)
        {
            _logger = logger;
            _tableClient = tableClient;
            _blobClient = blobClient;
        }

        [HttpGet]
        public IActionResult Get()
        {
            _logger.LogInformation("Example GET endpoint called");
            
            var response = new
            {
                Message = "Hello from the template API!",
                Timestamp = DateTime.UtcNow,
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown"
            };

            return ServiceResponse.ServiceResponseToIActionResult(ServiceResponse<object>.Success(response));
        }

        [HttpGet("workspaces")]
        public async Task<IActionResult> GetWorkspaces()
        {
            try
            {
                _logger.LogInformation("Getting workspaces information");
                
                // This would typically call a service that uses the Fabric API
                var workspaces = new[]
                {
                    new { Id = "workspace1", Name = "Sample Workspace 1" },
                    new { Id = "workspace2", Name = "Sample Workspace 2" }
                };

                return ServiceResponse.ServiceResponseToIActionResult(ServiceResponse<object[]>.Success(workspaces));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting workspaces");
                return ServiceResponse.ServiceResponseToIActionResult(
                    ServiceResponse<object[]>.Error(System.Net.HttpStatusCode.InternalServerError, ex.Message));
            }
        }

        [HttpPost("data")]
        public async Task<IActionResult> CreateData([FromBody] CreateDataRequest request)
        {
            try
            {
                _logger.LogInformation("Creating data with name: {Name}", request.Name);
                
                if (string.IsNullOrEmpty(request.Name))
                {
                    return ServiceResponse.ServiceResponseToIActionResult(
                        ServiceResponse<object>.Error(System.Net.HttpStatusCode.BadRequest, "Name is required"));
                }

                // Example of using the table client
                var tableClient = await _tableClient.GetTableClientAsync("ExampleData");
                
                // Example of using the blob client
                var containerClient = await _blobClient.GetContainerClientAsync("example-data");

                var response = new
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = request.Name,
                    CreatedAt = DateTime.UtcNow,
                    Message = "Data created successfully"
                };

                return ServiceResponse.ServiceResponseToIActionResult(ServiceResponse<object>.Success(response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating data");
                return ServiceResponse.ServiceResponseToIActionResult(
                    ServiceResponse<object>.Error(System.Net.HttpStatusCode.InternalServerError, ex.Message));
            }
        }
    }

    public class CreateDataRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
