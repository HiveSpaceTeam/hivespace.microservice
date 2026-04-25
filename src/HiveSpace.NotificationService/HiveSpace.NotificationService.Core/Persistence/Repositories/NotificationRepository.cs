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

    public async Task<(IReadOnlyList<Notification> Items, bool HasMore)> GetByUserAsync(
        Guid userId, int page, int pageSize, bool unreadOnly = false, CancellationToken ct = default)
    {
        var query = db.Notifications
                      .Where(n => n.UserId == userId)
                      .Where(n => n.Channel == NotificationChannel.InApp)
                      .Where(n => n.Status == NotificationStatus.Sent || n.Status == NotificationStatus.Read)
                      .AsQueryable();

        if (unreadOnly)
            query = query.Where(n => n.Status == NotificationStatus.Sent);

        var items = await query
                          .OrderByDescending(n => n.CreatedAt)
                          .Skip((page - 1) * pageSize)
                          .Take(pageSize + 1)
                          .ToListAsync(ct);

        var hasMore = items.Count > pageSize;
        if (hasMore) items.RemoveAt(items.Count - 1);

        return (items, hasMore);
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
