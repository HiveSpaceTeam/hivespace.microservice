using HiveSpace.IdentityService.Core.DomainModels;

namespace HiveSpace.IdentityService.Core.Interfaces;

public interface IOtpChallengeRepository
{
    Task<OtpChallenge?> GetActiveByChallengeTokenAsync(string challengeToken, CancellationToken ct = default);
    Task<OtpChallenge?> GetLatestActiveByEmailAndPurposeAsync(string emailNormalized, OtpChallengePurpose purpose, CancellationToken ct = default);
    Task<int> DeleteExpiredOlderThanAsync(DateTimeOffset cutoff, CancellationToken ct = default);
    Task AddAsync(OtpChallenge challenge, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
