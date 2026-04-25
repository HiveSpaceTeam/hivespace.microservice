using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HiveSpace.NotificationService.Core.DomainModels;
using HiveSpace.NotificationService.Core.Dtos;
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

        var channelPrefs = await prefs.GetAllChannelPrefsAsync(userId, ct);
        var groupPrefs   = await prefs.GetAllGroupPrefsAsync(userId, ct);

        var channelDict = channelPrefs.ToDictionary(p => p.Channel);
        var groupDict   = groupPrefs.ToDictionary(p => (p.Channel, p.EventGroup));
        var groups      = NotificationEventGroup.ForRole(GetRole());

        var result = Enum.GetValues<NotificationChannel>()
            .Select(ch => new ChannelPreferenceDto(
                ch,
                channelDict.TryGetValue(ch, out var cp) ? cp.Enabled : ch == NotificationChannel.InApp,
                groups
                    .Select(g => new GroupPreferenceDto(
                        g,
                        groupDict.TryGetValue((ch, g), out var gp) ? gp.Enabled : false))
                    .ToList()))
            .ToList();

        return Ok(result);
    }

    [HttpPut("{channel}")]
    public async Task<IActionResult> UpsertChannelPreference(
        NotificationChannel channel,
        [FromBody] UpsertChannelRequest request,
        CancellationToken ct = default)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var pref = UserChannelPreference.Create(userId, channel, request.Enabled);
        await prefs.UpsertChannelAsync(pref, ct);
        return NoContent();
    }

    [HttpPut("{channel}/{eventGroup}")]
    public async Task<IActionResult> UpsertGroupPreference(
        NotificationChannel channel,
        string eventGroup,
        [FromBody] UpsertGroupRequest request,
        CancellationToken ct = default)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        if (!NotificationEventGroup.ForRole(GetRole()).Contains(eventGroup))
            return BadRequest();

        var pref = UserGroupPreference.Create(userId, channel, eventGroup, request.Enabled);
        await prefs.UpsertGroupAsync(pref, ct);
        return NoContent();
    }

    private Guid GetUserId()
    {
        var sub = User.FindFirst("sub")?.Value
               ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(sub, out var id) ? id : Guid.Empty;
    }

    private string? GetRole()
        => User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
        ?? User.FindFirst("role")?.Value;
}

public record UpsertChannelRequest(bool Enabled);
public record UpsertGroupRequest(bool Enabled);
