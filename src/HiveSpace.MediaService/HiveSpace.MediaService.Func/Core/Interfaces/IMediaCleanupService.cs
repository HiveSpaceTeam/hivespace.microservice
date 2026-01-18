namespace HiveSpace.MediaService.Func.Core.Interfaces;

public interface IMediaCleanupService
{
    Task<CleanupResult> CleanupExpiredPendingAssetsAsync(CancellationToken cancellationToken = default);
}

public record CleanupResult(
    int TotalProcessed,
    int TotalErrors,
    int BatchesProcessed,
    TimeSpan Duration
);
