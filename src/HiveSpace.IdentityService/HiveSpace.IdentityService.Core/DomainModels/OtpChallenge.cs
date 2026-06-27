using HiveSpace.Domain.Shared.Entities;

namespace HiveSpace.IdentityService.Core.DomainModels;

public class OtpChallenge : Entity<OtpChallengeId>
{
    protected OtpChallenge() { }

    public string EmailNormalized { get; private set; } = default!;
    public OtpChallengePurpose Purpose { get; private set; }
    public string ChallengeToken { get; private set; } = default!;
    public string Code { get; private set; } = default!;
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset CanResendAt { get; private set; }
    public int AttemptCount { get; private set; }
    public bool IsUsed { get; private set; }
    public bool IsInvalidated { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public static OtpChallenge Create(
        string emailNormalized,
        OtpChallengePurpose purpose,
        string challengeToken,
        string code,
        DateTimeOffset expiresAt,
        DateTimeOffset canResendAt)
    {
        return new OtpChallenge
        {
            Id = OtpChallengeId.New(),
            EmailNormalized = emailNormalized,
            Purpose = purpose,
            ChallengeToken = challengeToken,
            Code = code,
            ExpiresAt = expiresAt,
            CanResendAt = canResendAt,
            AttemptCount = 0,
            IsUsed = false,
            IsInvalidated = false,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public bool IsActiveAt(DateTimeOffset now)
        => !IsUsed && !IsInvalidated && ExpiresAt > now;

    public void IncrementAttempt() => AttemptCount++;

    public void MarkUsed() => IsUsed = true;

    public void Invalidate() => IsInvalidated = true;
}
