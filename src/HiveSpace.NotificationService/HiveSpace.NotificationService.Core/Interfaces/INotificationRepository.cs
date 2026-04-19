using HiveSpace.NotificationService.Core.DomainModels;

namespace HiveSpace.NotificationService.Core.Interfaces;

public interface INotificationRepository
{
    void AddAll(IEnumerable<Notification> notifications);
    Task SaveAllAsync(IEnumerable<Notification> notifications, CancellationToken ct = default);
    Task<Notification?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<(IReadOnlyList<Notification> Items, int Total)> GetByUserAsync(Guid userId, int page, int pageSize, CancellationToken ct = default);
    Task<int> CountUnreadInAppAsync(Guid userId, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
