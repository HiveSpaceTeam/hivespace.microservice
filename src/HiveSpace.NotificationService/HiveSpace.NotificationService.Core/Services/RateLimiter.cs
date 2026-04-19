using StackExchange.Redis;
using HiveSpace.NotificationService.Core.DomainModels;
using HiveSpace.NotificationService.Core.Interfaces;

namespace HiveSpace.NotificationService.Core.Services;

public class RateLimiter(IConnectionMultiplexer redis) : IRateLimiter
{
    private const int MaxPerHour = 10;
    private static readonly TimeSpan Window = TimeSpan.FromHours(1);

    public async Task<bool> AllowAsync(
        Guid userId, NotificationChannel channel,
        CancellationToken ct = default)
    {
        var db    = redis.GetDatabase();
        var key   = $"ratelimit:{userId}:{channel}";
        var count = await db.StringIncrementAsync(key);

        if (count == 1)
            await db.KeyExpireAsync(key, Window);

        return count <= MaxPerHour;
    }
}
