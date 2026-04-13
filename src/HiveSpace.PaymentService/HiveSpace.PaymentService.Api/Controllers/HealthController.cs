using Microsoft.AspNetCore.Mvc;

namespace HiveSpace.PaymentService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
        => Ok(new { Status = "Healthy", Service = "PaymentService", Timestamp = DateTime.UtcNow });
}
