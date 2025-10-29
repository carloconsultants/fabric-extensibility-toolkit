using Microsoft.AspNetCore.Mvc;
using TemplateWorkload.Utilities;

namespace TemplateWorkload.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly ILogger<HealthController> _logger;

        public HealthController(ILogger<HealthController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get()
        {
            _logger.LogInformation("Health check endpoint called");
            return Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow });
        }

        [HttpGet("detailed")]
        public IActionResult GetDetailed()
        {
            _logger.LogInformation("Detailed health check endpoint called");
            return Ok(new
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                Version = "1.0.0",
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown"
            });
        }
    }
}
