namespace HiveSpace.CatalogService.Domain.CatalogImports;

public interface ICatalogImportBundleRepository
{
    void Add(CatalogImportBundle bundle);
    void AddJob(CatalogImportJob job);
    void AddQueueOutboxMessage(CatalogImportQueueOutboxMessage message)
    {
    }
    void AddExternalCategoryLink(ExternalCategoryLink link);
    void AddExternalCategoryAttributeLink(ExternalCategoryAttributeLink link);
    Task<CatalogImportBundle?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CatalogImportBundle?> GetBySourceFingerprintAsync(string sourceFingerprint, CancellationToken cancellationToken = default);
    Task<CatalogImportJob?> GetJobByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CatalogImportJob?> GetJobByOperationAndSourceFingerprintAsync(
        Enums.CatalogImportJobOperationType operationType,
        string sourceSystem,
        string sourceFingerprint,
        CancellationToken cancellationToken = default);
    Task<CatalogImportJob?> GetActiveJobByOperationAndSourceFingerprintAsync(
        Enums.CatalogImportJobOperationType operationType,
        string sourceSystem,
        string sourceFingerprint,
        CancellationToken cancellationToken = default);
    async Task<bool> TryStartJobAsync(Guid jobId, CancellationToken cancellationToken = default)
    {
        var job = await GetJobByIdAsync(jobId, cancellationToken);
        if (job is null || job.Status != Enums.CatalogImportJobStatus.Pending)
            return false;

        job.Start();
        await SaveChangesAsync(cancellationToken);
        return true;
    }
    async Task<bool> TryStartJobAsync(
        Guid jobId,
        Enums.CatalogImportJobOperationType operationType,
        int attempt,
        CancellationToken cancellationToken = default)
    {
        var job = await GetJobByIdAsync(jobId, cancellationToken);
        if (job is null
            || job.OperationType != operationType
            || !job.CanProcessAttempt(attempt))
        {
            return false;
        }

        job.Start();
        await SaveChangesAsync(cancellationToken);
        return true;
    }
    async Task<int> MarkJobFailedAsync(Guid jobId, string errorSummary, CancellationToken cancellationToken = default)
    {
        var job = await GetJobByIdAsync(jobId, cancellationToken);
        if (job is null || job.Status != Enums.CatalogImportJobStatus.Running)
            return 0;

        job.Fail(errorSummary);
        return await SaveChangesAsync(cancellationToken);
    }
    Task<IReadOnlyList<CatalogImportJob>> GetPendingJobsAsync(int limit, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CatalogImportJob>> GetRunningJobsInactiveSinceAsync(
        DateTimeOffset inactiveSince,
        int limit,
        CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<CatalogImportJob> Jobs, int TotalCount)> ListJobsAsync(
        int pageNumber,
        int pageSize,
        Enums.CatalogImportJobStatus? status = null,
        Enums.CatalogImportJobOperationType? operationType = null,
        string? sourceSystem = null,
        Guid? bundleId = null,
        DateTimeOffset? requestedFrom = null,
        DateTimeOffset? requestedTo = null,
        CancellationToken cancellationToken = default);
    Task<ExternalCategoryLink?> GetExternalCategoryLinkAsync(string sourceSystem, string externalCategoryId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExternalCategoryLink>> GetExternalCategoryLinksAsync(string sourceSystem, IEnumerable<string> externalCategoryIds, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExternalCategoryAttributeLink>> GetExternalCategoryAttributeLinksAsync(string sourceSystem, IEnumerable<string> externalCategoryIds, CancellationToken cancellationToken = default);
    Task<int?> GetImportedProductIdBySourceIdentityAsync(string sourceSystem, string externalProductId, CancellationToken cancellationToken = default);
    Task PrepareValidationIssueReplacementAsync(Guid bundleId, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
    Task DetachDeletedValidationIssuesAsync(Guid bundleId, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
    Task<(IReadOnlyList<CatalogImportBundle> Bundles, int TotalCount)> ListBundlesAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CatalogImportBundle>> ListRecentAsync(int limit, CancellationToken cancellationToken = default);
    Task<int> SaveJobProgressAsync(Guid jobId, int total, int processed, CancellationToken cancellationToken = default)
        => SaveChangesAsync(cancellationToken);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
