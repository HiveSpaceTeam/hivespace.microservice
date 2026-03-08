using HiveSpace.MediaService.Core.DomainModels;
using HiveSpace.MediaService.Core.Interfaces;
using HiveSpace.MediaService.Core.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HiveSpace.MediaService.Core.Services;

public class MediaCleanupService(
    MediaDbContext dbContext,
    ILogger<MediaCleanupService> logger,
    IConfiguration configuration) : IMediaCleanupService
{
    private int ExpirationHours { get; init; } = configuration.GetValue<int>("MediaCleanup:ExpirationHours", 24);
    private int BatchSize { get; init; } = configuration.GetValue<int>("MediaCleanup:BatchSize", 100);

    public async Task<CleanupResult> CleanupExpiredPendingAssetsAsync(CancellationToken cancellationToken = default)
    {
        var startTime = DateTimeOffset.UtcNow;
        var cutoffTime = DateTimeOffset.UtcNow.AddHours(-ExpirationHours);

        logger.LogInformation("Starting cleanup of assets older than {Hours} hours", ExpirationHours);

        int totalCleaned = 0;
        int batchCount = 0;

        try
        {
            // Keep processing batches until no more expired assets
            while (!cancellationToken.IsCancellationRequested)
            {
                var expiredAssets = await dbContext.MediaAssets
                    .Where(x => x.Status == MediaStatus.Pending && x.CreatedAt < cutoffTime)
                    .OrderBy(x => x.CreatedAt)
                    .Take(BatchSize)
                    .ToListAsync(cancellationToken);

                if (expiredAssets.Count == 0)
                    break;

                batchCount++;
                logger.LogInformation("Processing batch {Batch}: {Count} assets", batchCount, expiredAssets.Count);

                dbContext.MediaAssets.RemoveRange(expiredAssets);
                await dbContext.SaveChangesAsync(cancellationToken);

                totalCleaned += expiredAssets.Count;

                logger.LogInformation("Removed {Count} expired assets from database", expiredAssets.Count);

                if (expiredAssets.Count < BatchSize)
                    break;
            }

            var duration = DateTimeOffset.UtcNow - startTime;

            logger.LogInformation(
                "Cleanup complete: {Cleaned} cleaned, {Duration}s",
                totalCleaned,
                duration.TotalSeconds);

            return new CleanupResult(totalCleaned, duration);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Cleanup failed");
            throw;
        }
    }
}
