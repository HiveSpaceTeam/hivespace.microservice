using HiveSpace.MediaService.Func.Core.DomainModels;
using HiveSpace.MediaService.Func.Core.Interfaces;
using HiveSpace.MediaService.Func.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HiveSpace.MediaService.Func.Core.Services;

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
        int totalFailed = 0;
        int batchCount = 0;

        try
        {
            // Keep processing batches until no more expired assets
            while (!cancellationToken.IsCancellationRequested)
            {
                // Step 1: Get a batch of expired assets
                var expiredAssets = await dbContext.MediaAssets
                    .Where(x => x.Status == MediaStatus.Pending && x.CreatedAt < cutoffTime)
                    .OrderBy(x => x.CreatedAt)
                    .Take(BatchSize)
                    .ToListAsync(cancellationToken);

                // No more assets to clean up
                if (expiredAssets.Count == 0)
                    break;

                batchCount++;
                logger.LogInformation("Processing batch {Batch}: {Count} assets", batchCount, expiredAssets.Count);

                // Delete database records for expired assets
                // Blobs will be deleted automatically by Azure lifecycle policy
                dbContext.MediaAssets.RemoveRange(expiredAssets);
                await dbContext.SaveChangesAsync(cancellationToken);
                
                totalCleaned += expiredAssets.Count;
                
                logger.LogInformation("Removed {Count} expired assets from database", expiredAssets.Count);

                // If we got less than batch size, we're done
                if (expiredAssets.Count < BatchSize)
                    break;
            }

            var duration = DateTimeOffset.UtcNow - startTime;
            
            logger.LogInformation(
                "Cleanup complete: {Cleaned} cleaned, {Failed} failed, {Duration}s",
                totalCleaned,
                totalFailed,
                duration.TotalSeconds);

            return new CleanupResult(totalCleaned, totalFailed, batchCount, duration);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Cleanup failed");
            throw;
        }
    }

}
