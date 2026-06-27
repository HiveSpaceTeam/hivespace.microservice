using FluentAssertions;
using HiveSpace.IdentityService.Core.DomainModels;
using HiveSpace.IdentityService.Core.Infrastructure;
using HiveSpace.IdentityService.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace HiveSpace.IdentityService.Tests.Application.Session;

public class OtpChallengeCleanupServiceTests
{
    [Fact]
    public async Task CleanupExpiredChallengesAsync_DeletesOnlyRowsPastGraceCutoff()
    {
        var repository = new RecordingOtpChallengeRepository();
        var services = new ServiceCollection()
            .AddScoped<IOtpChallengeRepository>(_ => repository)
            .BuildServiceProvider();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Otp:CleanupGraceHours"] = "24",
                ["Otp:CleanupIntervalHours"] = "6"
            })
            .Build();

        var service = new OtpChallengeCleanupService(
            services.GetRequiredService<IServiceScopeFactory>(),
            configuration,
            NullLogger<OtpChallengeCleanupService>.Instance);

        var deletedCount = await service.CleanupExpiredChallengesAsync();

        deletedCount.Should().Be(3);
        repository.DeleteExpiredCalled.Should().BeTrue();
        repository.LastCutoff.Should().NotBeNull();
        repository.LastCutoff.Should().BeBefore(DateTimeOffset.UtcNow.AddHours(-23));
        repository.LastCutoff.Should().BeAfter(DateTimeOffset.UtcNow.AddHours(-25));
    }

    private sealed class RecordingOtpChallengeRepository : IOtpChallengeRepository
    {
        public bool DeleteExpiredCalled { get; private set; }
        public DateTimeOffset? LastCutoff { get; private set; }

        public Task<OtpChallenge?> GetActiveByChallengeTokenAsync(string challengeToken, CancellationToken ct = default)
            => Task.FromResult<OtpChallenge?>(null);

        public Task<OtpChallenge?> GetLatestActiveByEmailAndPurposeAsync(string emailNormalized, OtpChallengePurpose purpose, CancellationToken ct = default)
            => Task.FromResult<OtpChallenge?>(null);

        public Task<int> DeleteExpiredOlderThanAsync(DateTimeOffset cutoff, CancellationToken ct = default)
        {
            DeleteExpiredCalled = true;
            LastCutoff = cutoff;
            return Task.FromResult(3);
        }

        public Task AddAsync(OtpChallenge challenge, CancellationToken ct = default)
            => Task.CompletedTask;

        public Task SaveChangesAsync(CancellationToken ct = default)
            => Task.CompletedTask;
    }
}
