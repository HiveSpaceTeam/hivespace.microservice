using HiveSpace.IdentityService.Core.DomainModels;
using HiveSpace.IdentityService.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HiveSpace.IdentityService.Core.Persistence.Repositories;

public class OtpChallengeRepository(IdentityDbContext dbContext) : IOtpChallengeRepository
{
    public Task<OtpChallenge?> GetActiveByChallengeTokenAsync(string challengeToken, CancellationToken ct = default)
        => dbContext.Set<OtpChallenge>()
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(
                x => x.ChallengeToken == challengeToken
                    && !x.IsUsed
                    && !x.IsInvalidated
                    && x.ExpiresAt > DateTimeOffset.UtcNow,
                ct);

    public Task<OtpChallenge?> GetLatestActiveByEmailAndPurposeAsync(string emailNormalized, OtpChallengePurpose purpose, CancellationToken ct = default)
        => dbContext.Set<OtpChallenge>()
            .Where(x => x.EmailNormalized == emailNormalized
                && x.Purpose == purpose
                && !x.IsUsed
                && !x.IsInvalidated
                && x.ExpiresAt > DateTimeOffset.UtcNow)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(ct);

    public async Task<int> DeleteExpiredOlderThanAsync(DateTimeOffset cutoff, CancellationToken ct = default)
    {
        var expiredChallenges = await dbContext.Set<OtpChallenge>()
            .Where(x => x.ExpiresAt <= cutoff)
            .ToListAsync(ct);

        if (expiredChallenges.Count == 0)
            return 0;

        dbContext.RemoveRange(expiredChallenges);
        return await dbContext.SaveChangesAsync(ct);
    }

    public Task AddAsync(OtpChallenge challenge, CancellationToken ct = default)
        => dbContext.Set<OtpChallenge>().AddAsync(challenge, ct).AsTask();

    public Task SaveChangesAsync(CancellationToken ct = default)
        => dbContext.SaveChangesAsync(ct);
}
