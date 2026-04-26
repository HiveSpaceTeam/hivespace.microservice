using Microsoft.Extensions.Logging;
using HiveSpace.NotificationService.Core.DomainModels;
using HiveSpace.NotificationService.Core.Interfaces;
using HiveSpace.NotificationService.Core.Dispatch.Models;

namespace HiveSpace.NotificationService.Core.Infrastructure.Channels.InApp;

public class InAppChannelProvider(
    INotificationHubContext  hubContext,
    ILogger<InAppChannelProvider> logger) : IChannelProvider
{
    public NotificationChannel Channel => NotificationChannel.InApp;

    public async Task<DeliveryResult> SendAsync(
        Notification notification,
        Dictionary<string, object> templateData,
        CancellationToken ct = default)
    {
        try
        {
            await hubContext.SendToUserAsync(
                notification.UserId.ToString(),
                new
                {
                    notification.Id,
                    notification.EventType,
                    notification.Payload,
                    notification.CreatedAt,
                },
                ct);

            return DeliveryResult.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "SignalR delivery failed for UserId={UserId}", notification.UserId);
            return DeliveryResult.Fail(ex.Message);
        }
    }
}

/// <summary>Typed hub client contract — defined here so Core can reference it without the Hub class.</summary>
public interface INotificationClient
{
    Task ReceiveNotification(object payload);
}
