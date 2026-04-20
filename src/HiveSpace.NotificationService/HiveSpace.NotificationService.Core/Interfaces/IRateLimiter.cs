using HiveSpace.NotificationService.Core.DomainModels;

namespace HiveSpace.NotificationService.Core.Interfaces;

public interface IRateLimiter
{
    Task<bool> AllowAsync(Guid userId, NotificationChannel channel, CancellationToken ct = default);
}
