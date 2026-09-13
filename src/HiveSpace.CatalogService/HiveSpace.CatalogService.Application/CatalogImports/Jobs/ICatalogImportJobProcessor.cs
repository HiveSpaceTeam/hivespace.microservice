namespace HiveSpace.CatalogService.Application.CatalogImports.Jobs;

public interface ICatalogImportJobProcessor
{
    Task<int> ProcessPendingJobsAsync(int limit, CancellationToken cancellationToken = default);
    Task<int> RecoverStaleRunningJobsAsync(TimeSpan staleAfter, int limit, CancellationToken cancellationToken = default);
    Task ProcessJobAsync(Guid jobId, CancellationToken cancellationToken = default);
}
