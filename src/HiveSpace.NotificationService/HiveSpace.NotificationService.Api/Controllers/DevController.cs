using Microsoft.AspNetCore.Mvc;
using HiveSpace.NotificationService.Core.Interfaces;

namespace HiveSpace.NotificationService.Api.Controllers;

/// <summary>Development-only controller. Mapped only when ASPNETCORE_ENVIRONMENT=Development.</summary>
[ApiController]
[Route("api/dev")]
public class DevController(INotificationHubContext hub) : ControllerBase
{
    [HttpPost("notify")]
    public async Task<IActionResult> SendTestNotification(
        [FromQuery] Guid   userId,
        [FromQuery] string message = "Test notification from dev endpoint",
        CancellationToken  ct      = default)
    {
        await hub.SendToUserAsync(userId.ToString(), new
        {
            id        = Guid.NewGuid(),
            eventType = "dev.test",
            payload   = message,
            createdAt = DateTimeOffset.UtcNow,
        }, ct);

        return Ok(new { sent = true, userId, message });
    }
}
