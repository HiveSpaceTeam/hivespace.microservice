using HiveSpace.NotificationService.Core.DomainModels;

namespace HiveSpace.NotificationService.Core.Interfaces;

public interface INotificationTemplateRepository
{
    Task<NotificationTemplate?> GetAsync(string eventType, NotificationChannel channel, string locale, CancellationToken ct = default);
    Task UpsertAsync(NotificationTemplate template, CancellationToken ct = default);
}
