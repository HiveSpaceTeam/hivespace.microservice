using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HiveSpace.NotificationService.Core.Interfaces;

namespace HiveSpace.NotificationService.Api.Controllers;

[ApiController]
[Route("api/v1/notifications")]
[Authorize]
public class NotificationsController(
    INotificationRepository repo) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetMyNotifications(
        [FromQuery] int page     = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct     = default)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var (items, total) = await repo.GetByUserAsync(userId, page, pageSize, ct);
        return Ok(new { items, total, page, pageSize });
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount(CancellationToken ct = default)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var count = await repo.CountUnreadInAppAsync(userId, ct);
        return Ok(new { count });
    }

    [HttpPut("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken ct = default)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var notification = await repo.GetByIdAsync(id, ct);
        if (notification is null || notification.UserId != userId)
            return NotFound();

        notification.MarkRead();
        await repo.SaveChangesAsync(ct);
        return NoContent();
    }

    private Guid GetUserId()
    {
        var sub = User.FindFirst("sub")?.Value
               ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(sub, out var id) ? id : Guid.Empty;
    }
}
