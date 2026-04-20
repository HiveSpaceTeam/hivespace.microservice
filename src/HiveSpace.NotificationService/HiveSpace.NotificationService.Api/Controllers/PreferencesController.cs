using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HiveSpace.NotificationService.Core.DomainModels;
using HiveSpace.NotificationService.Core.Interfaces;

namespace HiveSpace.NotificationService.Api.Controllers;

[ApiController]
[Route("api/v1/notification-preferences")]
[Authorize]
public class PreferencesController(
    IUserPreferenceRepository prefs) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetMyPreferences(CancellationToken ct = default)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var result = await prefs.GetAllForUserAsync(userId, ct);
        return Ok(result);
    }

    [HttpPut]
    public async Task<IActionResult> UpsertPreference(
        [FromBody] UpsertPreferenceRequest request,
        CancellationToken ct = default)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var preference = UserPreference.Create(userId, request.Channel, request.EventType, request.Enabled);
        if (request.QuietHoursJson is not null)
            preference.Update(request.Enabled, request.QuietHoursJson);

        await prefs.UpsertAsync(preference, ct);
        return NoContent();
    }

    private Guid GetUserId()
    {
        var sub = User.FindFirst("sub")?.Value
               ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(sub, out var id) ? id : Guid.Empty;
    }
}

public record UpsertPreferenceRequest(
    NotificationChannel Channel,
    string              EventType,
    bool                Enabled,
    string?             QuietHoursJson = null);
