using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using HiveSpace.MediaService.Core.Data;
using HiveSpace.MediaService.Core.Interfaces;
using HiveSpace.MediaService.Core.DomainModels;

namespace HiveSpace.MediaService.Func.Functions;

public class CleanupPendingAssetsFunction(
    ILogger<CleanupPendingAssetsFunction> logger,
    IConfiguration configuration,
    IStorageService storageService,
    MediaDbContext dbContext)
{
    private string TempContainerName => configuration["AzureStorage:TempContainer"] ?? "temp-media-upload";

    [Function(nameof(CleanupPendingAssetsFunction))]
    public async Task Run([TimerTrigger("0 0 * * * *")] TimerInfo myTimer) // Run every hour
    {
        logger.LogInformation($"Cleanup pending assets function executed at: {DateTime.Now}");

        var cutoffTime = DateTimeOffset.UtcNow.AddHours(-24);

        try
        {
            var pendingAssets = await dbContext.MediaAssets
                .Where(x => x.Status == MediaStatus.Pending && x.CreatedAt < cutoffTime)
                .ToListAsync();

            if (pendingAssets.Count == 0)
            {
                logger.LogInformation("No expired pending assets found.");
                return;
            }

            logger.LogInformation($"Found {pendingAssets.Count} expired pending assets. Starting cleanup...");

            int successCount = 0;
            int errorCount = 0;

            foreach (var asset in pendingAssets)
            {
                try
                {
                    // 1. Delete from Blob Storage
                    // StoragePath should contain the blob name/path inside the container
                    if (!string.IsNullOrEmpty(asset.StoragePath))
                    {
                        await storageService.DeleteBlobAsync(TempContainerName, asset.StoragePath);
                        logger.LogInformation($"Deleted blob: {asset.StoragePath} from container: {TempContainerName}");
                    }

                    // 2. Remove from Database context
                    dbContext.MediaAssets.Remove(asset);
                    successCount++;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Error cleaning up asset {asset.Id} (StoragePath: {asset.StoragePath})");
                    errorCount++;
                    // If storage deletion failed, we ideally shouldn't delete from DB yet, or we retry next time.
                    // By not adding to deletion tracking (or rolling back?), wait, Remove(asset) is synchronous on context.
                    // If I want to skip DB deletion for this item, I should not call Remove(asset).
                    // In this try/catch, if exception occurs at DeleteBlobAsync, Remove() is not reached.
                }
            }

            if (successCount > 0)
            {
                await dbContext.SaveChangesAsync();
                logger.LogInformation($"Successfully removed {successCount} assets from database.");
            }

            if (errorCount > 0)
            {
                logger.LogWarning($"{errorCount} assets failed to process during cleanup.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Critical error during cleanup function execution.");
        }
    }
}
