using Microsoft.EntityFrameworkCore;
using HiveSpace.NotificationService.Core.DomainModels;
using HiveSpace.NotificationService.Core.Interfaces;

namespace HiveSpace.NotificationService.Core.Persistence.Repositories;

public class NotificationTemplateRepository(NotificationDbContext db) : INotificationTemplateRepository
{
    public Task<NotificationTemplate?> GetAsync(
        string eventType, NotificationChannel channel, string locale, CancellationToken ct = default)
        => db.NotificationTemplates.FirstOrDefaultAsync(
               t => t.EventType == eventType && t.Channel == channel && t.Locale == locale,
               ct);

    public async Task UpsertAsync(NotificationTemplate template, CancellationToken ct = default)
    {
        var existing = await db.NotificationTemplates.FirstOrDefaultAsync(
            t => t.EventType == template.EventType
              && t.Channel == template.Channel
              && t.Locale == template.Locale,
            ct);

        if (existing is null)
            db.NotificationTemplates.Add(template);
        else
            existing.Update(template.Subject, template.BodyTemplate);

        await db.SaveChangesAsync(ct);
    }
}
