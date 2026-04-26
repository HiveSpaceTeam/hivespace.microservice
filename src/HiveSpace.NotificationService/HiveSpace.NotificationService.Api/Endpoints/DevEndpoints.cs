using HiveSpace.NotificationService.Core.Interfaces;

namespace HiveSpace.NotificationService.Api.Endpoints;

public static class DevEndpoints
{
    public static IEndpointRouteBuilder MapDevEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/dev");

        group.MapPost("notify", async (
            INotificationHubContext hub,
            Guid userId,
            string message = "Test notification from dev endpoint",
            CancellationToken ct = default) =>
        {
            await hub.SendToUserAsync(userId.ToString(), new
            {
                id        = Guid.NewGuid(),
                eventType = "dev.test",
                payload   = message,
                createdAt = DateTimeOffset.UtcNow,
            }, ct);

            return Results.Ok(new { sent = true, userId, message });
        });

        return app;
    }
}
