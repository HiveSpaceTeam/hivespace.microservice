using HiveSpace.IdentityService.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HiveSpace.IdentityService.Core.Infrastructure;

public class OtpChallengeCleanupService(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<OtpChallengeCleanupService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = GetCleanupInterval();

        while (!stoppingToken.IsCancellationRequested)
        {
            await CleanupExpiredChallengesAsync(stoppingToken);

            try
            {
                await Task.Delay(interval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    public async Task<int> CleanupExpiredChallengesAsync(CancellationToken cancellationToken = default)
    {
        using var scope = scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IOtpChallengeRepository>();
        var cutoff = DateTimeOffset.UtcNow.Add(GetCleanupGrace().Negate());
        var deletedCount = await repository.DeleteExpiredOlderThanAsync(cutoff, cancellationToken);

        if (deletedCount > 0)
            logger.LogInformation("Deleted {DeletedCount} expired OTP challenges older than {Cutoff}", deletedCount, cutoff);

        return deletedCount;
    }

    private TimeSpan GetCleanupInterval()
    {
        var configuredHours = configuration.GetValue("Otp:CleanupIntervalHours", 6);
        return TimeSpan.FromHours(Math.Max(1, configuredHours));
    }

    private TimeSpan GetCleanupGrace()
    {
        var configuredHours = configuration.GetValue("Otp:CleanupGraceHours", 24);
        return TimeSpan.FromHours(Math.Max(1, configuredHours));
    }
}
