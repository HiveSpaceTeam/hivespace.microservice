using Microsoft.AspNetCore.Mvc;

namespace ServiceName.Api.Controllers;

/// <summary>
/// Health check controller for ServiceName microservice
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { 
            Status = "Healthy", 
            Service = "ServiceName", 
            Timestamp = DateTime.UtcNow 
        });
    }
}