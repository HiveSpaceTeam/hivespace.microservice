using MediatR;
using HiveSpace.NotificationService.Core.Features.Notifications.Commands.MarkNotificationRead;
using HiveSpace.NotificationService.Core.Features.Notifications.Queries.GetNotifications;
using HiveSpace.NotificationService.Core.Features.Notifications.Queries.GetUnreadCount;

namespace HiveSpace.NotificationService.Api.Endpoints;

public static class NotificationEndpoints
{
    public static IEndpointRouteBuilder MapNotificationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v1/notifications")
            .RequireAuthorization();

        group.MapGet("/", async (
            ISender sender,
            int page = 1,
            int pageSize = 20,
            bool unreadOnly = false,
            CancellationToken ct = default) =>
        {
            var result = await sender.Send(new GetNotificationsQuery(page, pageSize, unreadOnly), ct);
            return Results.Ok(result);
        });

        group.MapGet("unread-count", async (ISender sender, CancellationToken ct) =>
        {
            var count = await sender.Send(new GetUnreadCountQuery(), ct);
            return Results.Ok(new { count });
        });

        group.MapPut("{id:guid}/read", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new MarkNotificationReadCommand(id), ct);
            return Results.NoContent();
        });

        return app;
    }
}
