using StackExchange.Redis;
using HiveSpace.NotificationService.Core.Interfaces;

namespace HiveSpace.NotificationService.Core.Services;

public class DeduplicationService(IConnectionMultiplexer redis) : IDeduplicationService
{
    private static readonly TimeSpan Ttl = TimeSpan.FromHours(24);

    public async Task<bool> IsDuplicateAsync(string idempotencyKey, CancellationToken ct = default)
    {
        var db  = redis.GetDatabase();
        var key = $"dedup:{idempotencyKey}";

        // SET key "1" EX 86400 NX — returns true if key was SET (not a duplicate)
        var wasSet = await db.StringSetAsync(key, "1", Ttl, When.NotExists);
        return !wasSet;
    }
}
