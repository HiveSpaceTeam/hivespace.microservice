using Microsoft.AspNetCore.Mvc;

namespace ServiceName.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { Status = "Healthy", Service = "ServiceName", Timestamp = DateTimeOffset.UtcNow });
}
