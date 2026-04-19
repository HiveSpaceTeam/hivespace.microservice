using HiveSpace.NotificationService.Core.DomainModels;

namespace HiveSpace.NotificationService.Core.Interfaces;

public interface IChannelRouter
{
    Task SendAsync(Notification notification, Dictionary<string, object> templateData, CancellationToken ct = default);
}
