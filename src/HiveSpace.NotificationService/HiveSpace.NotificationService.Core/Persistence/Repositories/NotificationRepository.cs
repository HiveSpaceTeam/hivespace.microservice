using Microsoft.EntityFrameworkCore;
using HiveSpace.NotificationService.Core.DomainModels;
using HiveSpace.NotificationService.Core.Interfaces;

namespace HiveSpace.NotificationService.Core.Persistence.Repositories;

public class NotificationRepository(NotificationDbContext db) : INotificationRepository
{
    public void AddAll(IEnumerable<Notification> notifications)
        => db.Notifications.AddRange(notifications);

    public async Task SaveAllAsync(IEnumerable<Notification> notifications, CancellationToken ct = default)
    {
        db.Notifications.AddRange(notifications);
        await db.SaveChangesAsync(ct);
    }

    public Task<Notification?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => db.Notifications
             .Include(n => n.Attempts)
             .FirstOrDefaultAsync(n => n.Id == id, ct);

    public async Task<(IReadOnlyList<Notification> Items, int Total)> GetByUserAsync(
        Guid userId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = db.Notifications
                      .Where(n => n.UserId == userId)
                      .Where(n => n.Channel == NotificationChannel.InApp) // Only in-app for listing
                      .OrderByDescending(n => n.CreatedAt);

        var total = await query.CountAsync(ct);
        var items = await query
                          .Skip((page - 1) * pageSize)
                          .Take(pageSize)
                          .ToListAsync(ct);

        return (items, total);
    }

    public Task<int> CountUnreadInAppAsync(Guid userId, CancellationToken ct = default)
        => db.Notifications.CountAsync(
               n => n.UserId == userId
                 && n.Channel == NotificationChannel.InApp
                 && n.Status == NotificationStatus.Sent,
               ct);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => db.SaveChangesAsync(ct);
}
