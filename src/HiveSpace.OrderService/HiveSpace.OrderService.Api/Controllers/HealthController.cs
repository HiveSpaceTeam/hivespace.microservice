using Microsoft.AspNetCore.Mvc;

namespace HiveSpace.OrderService.Api.Controllers;

/// <summary>
/// Health check controller for HiveSpace.OrderService microservice
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
            Service = "HiveSpace.OrderService", 
            Timestamp = DateTime.UtcNow 
        });
    }
}