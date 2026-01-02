using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using HiveSpace.MediaService.Func.Infrastructure.Data;
using HiveSpace.MediaService.Func.Core.Interfaces;
using HiveSpace.MediaService.Func.Core.DomainModels;

namespace HiveSpace.MediaService.Func.Functions.Timer;

public class CleanupPendingAssetsFunction(
    ILogger<CleanupPendingAssetsFunction> logger,
    IConfiguration configuration,
    IStorageService storageService,
    MediaDbContext dbContext)
{
    private string TempContainerName => configuration["AzureStorage:TempContainer"] ?? "temp-media-upload";

    [Function(nameof(CleanupPendingAssetsFunction))]
    public async Task Run([TimerTrigger("0 0 * * * *")] TimerInfo timerInfo) // Run every hour
    {
        logger.LogInformation("Cleanup pending assets function executed at: {RunTime}", DateTime.Now);

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

            logger.LogInformation("Found {Count} expired pending assets. Starting cleanup...", pendingAssets.Count);
            
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
                            logger.LogInformation("Deleted blob: {StoragePath} from container: {ContainerName}", asset.StoragePath, TempContainerName);
                        }
                        catch 
                        {
                            // If delete fails (e.g. network), we rethrow to skip DB deletion for this item.
                            throw;
                        }
                    }

                    // 2. Remove from Database and Save per item
                    dbContext.MediaAssets.Remove(asset);
                    await dbContext.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error cleaning up asset {AssetId} (StoragePath: {StoragePath})", asset.Id, asset.StoragePath);
                    errorCount++;

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
                logger.LogWarning("{ErrorCount} assets failed to process during cleanup.", errorCount);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Critical error during cleanup function execution.");
        }
    }
}
