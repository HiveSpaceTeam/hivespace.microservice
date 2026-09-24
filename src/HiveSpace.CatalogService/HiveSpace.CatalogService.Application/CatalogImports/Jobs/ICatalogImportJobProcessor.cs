using HiveSpace.CatalogService.Application.CatalogImports.Queueing;

namespace HiveSpace.CatalogService.Application.CatalogImports.Jobs;

public interface ICatalogImportJobProcessor
{
    Task<int> RecoverStaleRunningJobsAsync(TimeSpan staleAfter, int limit, CancellationToken cancellationToken = default);
    Task ProcessJobAsync(Guid jobId, CancellationToken cancellationToken = default);
    Task ProcessQueuedJobAsync(CatalogImportQueueWorkItem workItem, CancellationToken cancellationToken = default);
    Task ProcessClaimedQueuedJobAsync(CatalogImportQueueWorkItem workItem, CancellationToken cancellationToken = default);
}
