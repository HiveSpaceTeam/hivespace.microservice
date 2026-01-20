using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using HiveSpace.MediaService.Func.Core.Interfaces;

namespace HiveSpace.MediaService.Func.Functions.Timer;

public class CleanupPendingAssetsFunction(
    ILogger<CleanupPendingAssetsFunction> logger,
    IMediaCleanupService cleanupService
    )
{
    [Function(nameof(CleanupPendingAssetsFunction))]
    public async Task Run([TimerTrigger("0 0 17 * * *")] TimerInfo timerInfo) 
    {
        logger.LogInformation("Cleanup pending assets function triggered at: {RunTime}", DateTime.UtcNow);

        try
        {
            var result = await cleanupService.CleanupExpiredPendingAssetsAsync();
            
            logger.LogInformation(
                "Cleanup function completed successfully. Processed: {Processed}, Duration: {Duration}s",
                result.TotalProcessed,
                result.Duration.TotalSeconds);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Cleanup function failed");
            throw; // Re-throw to mark the function execution as failed
        }
    }
}
