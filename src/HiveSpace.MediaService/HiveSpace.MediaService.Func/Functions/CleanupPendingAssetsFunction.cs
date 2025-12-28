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
            
            int errorCount = 0;

            foreach (var asset in pendingAssets)
            {
                try
                {
                    // 1. Delete from Blob Storage
                    // StoragePath should contain the blob name/path inside the container
                    if (!string.IsNullOrEmpty(asset.StoragePath))
                    {
                        try 
                        {
                            await storageService.DeleteBlobAsync(TempContainerName, asset.StoragePath);
                            logger.LogInformation($"Deleted blob: {asset.StoragePath} from container: {TempContainerName}");
                        }
                        catch (Exception ex)
                        {
                            // If delete fails (e.g. network), we rethrow to skip DB deletion for this item.
                            // If it's 404 (already gone), ideally we proceed, but without knowing the specific exception type from generic IStorageService,
                            // we rely on it throwing for actual failures.
                            // NOTE: If the blob is already gone, this might fail depending on implementation. 
                            // Assuming typical idempotency or that exception implies retry needed.
                            throw;
                        }
                    }

                    // 2. Remove from Database and Save per item
                    dbContext.MediaAssets.Remove(asset);
                    await dbContext.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Error cleaning up asset {asset.Id} (StoragePath: {asset.StoragePath})");
                    errorCount++;

                    // Critical: If SaveChanges failed, the entity is still tracked as 'Deleted'.
                    // We must reset its state so it doesn't cause the next SaveChangesAsync to fail or retry removing it.
                    try 
                    {
                        var entry = dbContext.Entry(asset);
                        if (entry.State == EntityState.Deleted)
                        {
                            // Revert to Unchanged so it is not processed in future SaveChanges calls in this loop
                            entry.State = EntityState.Unchanged;
                        }
                    }
                    catch (Exception stateEx)
                    {
                        logger.LogError(stateEx, "Failed to reset entity state for asset {AssetId}", asset.Id);
                    }
                }
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
