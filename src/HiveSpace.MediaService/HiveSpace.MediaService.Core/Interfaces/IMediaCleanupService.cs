namespace HiveSpace.MediaService.Core.Interfaces;

public interface IMediaCleanupService
{
    Task<CleanupResult> CleanupExpiredPendingAssetsAsync(CancellationToken cancellationToken = default);
}

public record CleanupResult(
    int TotalProcessed,
    TimeSpan Duration
);
