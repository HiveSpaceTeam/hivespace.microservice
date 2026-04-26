using MediatR;
using Microsoft.AspNetCore.Mvc;
using HiveSpace.NotificationService.Core.DomainModels;
using HiveSpace.NotificationService.Core.Features.Preferences.Commands.UpsertChannelPreference;
using HiveSpace.NotificationService.Core.Features.Preferences.Commands.UpsertGroupPreference;
using HiveSpace.NotificationService.Core.Features.Preferences.Queries.GetPreferences;

namespace HiveSpace.NotificationService.Api.Endpoints;

public static class PreferenceEndpoints
{
    public static IEndpointRouteBuilder MapPreferenceEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v1/notification-preferences")
            .RequireAuthorization();

        group.MapGet("/", async (ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetPreferencesQuery(), ct);
            return Results.Ok(result);
        });

        group.MapPut("{channel}", async (
            NotificationChannel channel,
            [FromBody] UpsertChannelRequest body,
            ISender sender,
            CancellationToken ct) =>
        {
            await sender.Send(new UpsertChannelPreferenceCommand(channel, body.Enabled), ct);
            return Results.NoContent();
        });

        group.MapPut("{channel}/{eventGroup}", async (
            NotificationChannel channel,
            string eventGroup,
            [FromBody] UpsertGroupRequest body,
            ISender sender,
            CancellationToken ct) =>
        {
            await sender.Send(new UpsertGroupPreferenceCommand(channel, eventGroup, body.Enabled), ct);
            return Results.NoContent();
        });

        return app;
    }
}

public record UpsertChannelRequest(bool Enabled);
public record UpsertGroupRequest(bool Enabled);
