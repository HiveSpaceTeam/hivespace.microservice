namespace HiveSpace.IdentityService.Core.DomainModels;

public readonly record struct OtpChallengeId(Guid Value)
{
    public static OtpChallengeId New() => new(Guid.NewGuid());
}
