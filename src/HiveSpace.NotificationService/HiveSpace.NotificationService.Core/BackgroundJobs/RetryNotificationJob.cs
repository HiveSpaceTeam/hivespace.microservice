using System.Text.Json;
using HiveSpace.NotificationService.Core.DomainModels;
using HiveSpace.NotificationService.Core.Interfaces;

namespace HiveSpace.NotificationService.Core.BackgroundJobs;

public class RetryNotificationJob(
    INotificationRepository repo,
    IChannelRouter          router)
{
    public async Task ExecuteAsync(Guid notificationId)
    {
        var notification = await repo.GetByIdAsync(notificationId);
        if (notification is null) return;

        if (notification.Status is not (NotificationStatus.Failed or NotificationStatus.Throttled))
            return;

        var templateData = JsonSerializer.Deserialize<Dictionary<string, object>>(notification.Payload)
                        ?? new Dictionary<string, object>();

        await router.SendAsync(notification, templateData);
    }
}
