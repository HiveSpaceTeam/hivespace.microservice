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
    public async Task Run([TimerTrigger("0 0 0 * * *")] TimerInfo timerInfo) // Run daily at midnight UTC
    {
        logger.LogInformation("Cleanup pending assets function executed at: {RunTime}", DateTime.Now);

        var cutoffTime = DateTimeOffset.UtcNow.AddHours(-24);
        const int batchSize = 100; // Process in batches to avoid memory issues
        int totalProcessed = 0;
        int totalErrors = 0;

        try
        {
            // Process in batches using pagination
            bool hasMore = true;
            
            while (hasMore)
            {
                var pendingAssets = await dbContext.MediaAssets
                    .Where(x => x.Status == MediaStatus.Pending && x.CreatedAt < cutoffTime)
                    .Take(batchSize)
                    .ToListAsync();

                if (pendingAssets.Count == 0)
                {
                    hasMore = false;
                    break;
                }

                logger.LogInformation("Processing batch of {Count} expired pending assets...", pendingAssets.Count);

                // Batch delete from blob storage
                var blobDeleteTasks = pendingAssets
                    .Where(asset => !string.IsNullOrEmpty(asset.StoragePath))
                    .Select(asset => DeleteBlobSafelyAsync(asset));

                var deleteResults = await Task.WhenAll(blobDeleteTasks);

                // Remove successfully deleted assets from database
                var successfulDeletes = pendingAssets
                    .Where((asset, index) => 
                        string.IsNullOrEmpty(asset.StoragePath) || 
                        deleteResults[pendingAssets.IndexOf(asset)])
                    .ToList();

                if (successfulDeletes.Any())
                {
                    dbContext.MediaAssets.RemoveRange(successfulDeletes);
                    await dbContext.SaveChangesAsync();
                    totalProcessed += successfulDeletes.Count;
                }

                totalErrors += pendingAssets.Count - successfulDeletes.Count;

                // If we got less than batch size, we're done
                if (pendingAssets.Count < batchSize)
                {
                    hasMore = false;
                }
            }

            if (totalProcessed == 0)
            {
                logger.LogInformation("No expired pending assets found.");
            }
            else
            {
                logger.LogInformation(
                    "Cleanup completed. Processed: {Processed}, Errors: {Errors}", 
                    totalProcessed, 
                    totalErrors);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Critical error during cleanup function execution.");
        }
    }

    private async Task<bool> DeleteBlobSafelyAsync(MediaAsset asset)
    {
        try
        {
            await storageService.DeleteBlobAsync(TempContainerName, asset.StoragePath);
            logger.LogDebug("Deleted blob: {StoragePath}", asset.StoragePath);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex, 
                "Failed to delete blob for asset {AssetId} (StoragePath: {StoragePath})", 
                asset.Id, 
                asset.StoragePath);
            return false;
        }
    }
}
