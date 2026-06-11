using HiveSpace.IdentityService.Core.Interfaces.Services;
using Microsoft.Extensions.Caching.Distributed;

namespace HiveSpace.IdentityService.Api.Services;

public class DistributedEmailVerificationResendCooldownStore(IDistributedCache cache)
    : IEmailVerificationResendCooldownStore
{
    private const string KeyPrefix = "identity:email-verification:cooldown:";

    public async Task<DateTimeOffset?> GetCooldownEndsAtAsync(
        string normalizedEmail,
        CancellationToken cancellationToken = default)
    {
        var value = await cache.GetStringAsync(BuildKey(normalizedEmail), cancellationToken);
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return DateTimeOffset.TryParse(value, out var cooldownEndsAt)
            ? cooldownEndsAt
            : null;
    }

    public Task SetCooldownAsync(
        string normalizedEmail,
        DateTimeOffset cooldownEndsAt,
        CancellationToken cancellationToken = default)
    {
        return cache.SetStringAsync(
            BuildKey(normalizedEmail),
            cooldownEndsAt.ToString("O"),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = cooldownEndsAt
            },
            cancellationToken);
    }

    private static string BuildKey(string normalizedEmail) => $"{KeyPrefix}{normalizedEmail}";
}
