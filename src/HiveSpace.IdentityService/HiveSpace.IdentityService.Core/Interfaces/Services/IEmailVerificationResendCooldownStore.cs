namespace HiveSpace.IdentityService.Core.Interfaces.Services;

public interface IEmailVerificationResendCooldownStore
{
    Task<DateTimeOffset?> GetCooldownEndsAtAsync(string normalizedEmail, CancellationToken cancellationToken = default);

    Task SetCooldownAsync(
        string normalizedEmail,
        DateTimeOffset cooldownEndsAt,
        CancellationToken cancellationToken = default);
}
