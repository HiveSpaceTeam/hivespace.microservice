using Microsoft.AspNetCore.SignalR;
using HiveSpace.NotificationService.Core.Channels.InApp;
using HiveSpace.NotificationService.Core.Interfaces;

namespace HiveSpace.NotificationService.Api.Hubs;

public class NotificationHubContext(
    IHubContext<NotificationHub, INotificationClient> hubContext) : INotificationHubContext
{
    public Task SendToUserAsync(string userId, object payload, CancellationToken ct = default)
        => hubContext.Clients.Group(userId).ReceiveNotification(payload);
}
