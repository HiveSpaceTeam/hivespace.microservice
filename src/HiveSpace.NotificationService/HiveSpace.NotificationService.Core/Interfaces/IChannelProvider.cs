using HiveSpace.NotificationService.Core.DomainModels;
using HiveSpace.NotificationService.Core.Dispatch.Models;

namespace HiveSpace.NotificationService.Core.Interfaces;

public interface IChannelProvider
{
    NotificationChannel Channel { get; }
    Task<DeliveryResult> SendAsync(Notification notification, Dictionary<string, object> templateData, CancellationToken ct = default);
}
