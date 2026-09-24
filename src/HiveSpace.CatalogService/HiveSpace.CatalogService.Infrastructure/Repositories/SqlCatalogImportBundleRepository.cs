using HiveSpace.CatalogService.Domain.CatalogImports;
using HiveSpace.CatalogService.Domain.CatalogImports.Enums;
using HiveSpace.CatalogService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HiveSpace.CatalogService.Infrastructure.Repositories;

public class SqlCatalogImportBundleRepository(CatalogDbContext context) : ICatalogImportBundleRepository
{
    public void Add(CatalogImportBundle bundle)
    {
        context.CatalogImportBundles.Add(bundle);
    }

    public void AddJob(CatalogImportJob job)
    {
        context.CatalogImportJobs.Add(job);
    }

    public void AddQueueOutboxMessage(CatalogImportQueueOutboxMessage message)
    {
        context.CatalogImportQueueOutboxMessages.Add(message);
    }

    public void AddExternalCategoryLink(ExternalCategoryLink categoryLink)
    {
        context.ExternalCategoryLinks.Add(categoryLink);
    }

    public void AddExternalCategoryAttributeLink(ExternalCategoryAttributeLink categoryAttributeLink)
    {
        context.ExternalCategoryAttributeLinks.Add(categoryAttributeLink);
    }

    public async Task<CatalogImportBundle?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await IncludeDetail(context.CatalogImportBundles)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<CatalogImportBundle?> GetBySourceFingerprintAsync(
        string sourceFingerprint,
        CancellationToken cancellationToken = default)
    {
        return await IncludeDetail(context.CatalogImportBundles)
            .FirstOrDefaultAsync(x => x.SourceFingerprint == sourceFingerprint, cancellationToken);
    }

    public async Task<CatalogImportJob?> GetJobByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.CatalogImportJobs
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<CatalogImportJob?> GetJobByOperationAndSourceFingerprintAsync(
        CatalogImportJobOperationType operationType,
        string sourceSystem,
        string sourceFingerprint,
        CancellationToken cancellationToken = default)
    {
        return await context.CatalogImportJobs
            .Where(x =>
                x.OperationType == operationType
                && x.SourceSystem == sourceSystem
                && x.SourceFingerprint == sourceFingerprint)
            .OrderByDescending(x => x.RequestedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<CatalogImportJob?> GetActiveJobByOperationAndSourceFingerprintAsync(
        CatalogImportJobOperationType operationType,
        string sourceSystem,
        string sourceFingerprint,
        CancellationToken cancellationToken = default)
    {
        return await context.CatalogImportJobs
            .Where(x =>
                x.OperationType == operationType
                && x.SourceSystem == sourceSystem
                && x.SourceFingerprint == sourceFingerprint
                && (x.Status == CatalogImportJobStatus.Pending || x.Status == CatalogImportJobStatus.Running))
            .OrderByDescending(x => x.RequestedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> TryStartJobAsync(Guid jobId, CancellationToken cancellationToken = default)
    {
        if (!context.Database.IsRelational())
        {
            var job = await GetJobByIdAsync(jobId, cancellationToken);
            if (job is null || job.Status != CatalogImportJobStatus.Pending)
                return false;

            job.Start();
            await SaveChangesAsync(cancellationToken);
            return true;
        }

        var startedAt = DateTimeOffset.UtcNow;
        var updated = await context.CatalogImportJobs
            .Where(x => x.Id == jobId && x.Status == CatalogImportJobStatus.Pending)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Status, CatalogImportJobStatus.Running)
                .SetProperty(x => x.StartedAt, startedAt)
                .SetProperty(x => x.LastActivityAt, startedAt)
                .SetProperty(x => x.ErrorSummary, (string?)null),
                cancellationToken);

        if (updated != 1)
            return false;

        var trackedEntry = context.ChangeTracker
            .Entries<CatalogImportJob>()
            .FirstOrDefault(x => x.Entity.Id == jobId);

        if (trackedEntry is not null)
        {
            trackedEntry.Property(x => x.Status).CurrentValue = CatalogImportJobStatus.Running;
            trackedEntry.Property(x => x.StartedAt).CurrentValue = startedAt;
            trackedEntry.Property(x => x.LastActivityAt).CurrentValue = startedAt;
            trackedEntry.Property(x => x.ErrorSummary).CurrentValue = null;
            trackedEntry.State = EntityState.Unchanged;
        }

        return true;
    }

    public async Task<bool> TryStartJobAsync(
        Guid jobId,
        CatalogImportJobOperationType operationType,
        int attempt,
        CancellationToken cancellationToken = default)
    {
        if (!context.Database.IsRelational())
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

        var startedAt = DateTimeOffset.UtcNow;
        var updated = await context.CatalogImportJobs
            .Where(x =>
                x.Id == jobId
                && x.OperationType == operationType
                && x.Attempt == attempt
                && x.Status == CatalogImportJobStatus.Pending)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Status, CatalogImportJobStatus.Running)
                .SetProperty(x => x.StartedAt, startedAt)
                .SetProperty(x => x.LastActivityAt, startedAt)
                .SetProperty(x => x.ErrorSummary, (string?)null),
                cancellationToken);

        if (updated != 1)
            return false;

        var trackedEntry = context.ChangeTracker
            .Entries<CatalogImportJob>()
            .FirstOrDefault(x => x.Entity.Id == jobId);

        if (trackedEntry is not null)
        {
            trackedEntry.Property(x => x.Status).CurrentValue = CatalogImportJobStatus.Running;
            trackedEntry.Property(x => x.StartedAt).CurrentValue = startedAt;
            trackedEntry.Property(x => x.LastActivityAt).CurrentValue = startedAt;
            trackedEntry.Property(x => x.ErrorSummary).CurrentValue = null;
            trackedEntry.State = EntityState.Unchanged;
        }

        return true;
    }

    public async Task<int> MarkJobFailedAsync(Guid jobId, string errorSummary, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(errorSummary))
            return 0;

        if (!context.Database.IsRelational())
        {
            var job = await GetJobByIdAsync(jobId, cancellationToken);
            if (job is null || job.Status != CatalogImportJobStatus.Running)
                return 0;

            job.Fail(errorSummary);
            return await SaveChangesAsync(cancellationToken);
        }

        var completedAt = DateTimeOffset.UtcNow;
        var updated = await context.CatalogImportJobs
            .Where(x =>
                x.Id == jobId
                && (x.Status == CatalogImportJobStatus.Pending || x.Status == CatalogImportJobStatus.Running))
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Status, CatalogImportJobStatus.Failed)
                .SetProperty(x => x.CompletedAt, completedAt)
                .SetProperty(x => x.LastActivityAt, completedAt)
                .SetProperty(x => x.ErrorSummary, errorSummary.Trim()),
                cancellationToken);

        var trackedEntry = context.ChangeTracker
            .Entries<CatalogImportJob>()
            .FirstOrDefault(x => x.Entity.Id == jobId);

        if (trackedEntry is not null)
        {
            trackedEntry.Property(x => x.Status).CurrentValue = CatalogImportJobStatus.Failed;
            trackedEntry.Property(x => x.CompletedAt).CurrentValue = completedAt;
            trackedEntry.Property(x => x.LastActivityAt).CurrentValue = completedAt;
            trackedEntry.Property(x => x.ErrorSummary).CurrentValue = errorSummary.Trim();
            trackedEntry.State = EntityState.Unchanged;
        }

        return updated;
    }

    public async Task<IReadOnlyList<CatalogImportJob>> GetPendingJobsAsync(
        int limit,
        CancellationToken cancellationToken = default)
    {
        return await context.CatalogImportJobs
            .Where(x => x.Status == CatalogImportJobStatus.Pending)
            .OrderBy(x => x.RequestedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CatalogImportJob>> GetRunningJobsInactiveSinceAsync(
        DateTimeOffset inactiveSince,
        int limit,
        CancellationToken cancellationToken = default)
    {
        return await context.CatalogImportJobs
            .Where(x =>
                x.Status == CatalogImportJobStatus.Running
                && x.LastActivityAt <= inactiveSince)
            .OrderBy(x => x.LastActivityAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<CatalogImportJob> Jobs, int TotalCount)> ListJobsAsync(
        int pageNumber,
        int pageSize,
        CatalogImportJobStatus? status = null,
        CatalogImportJobOperationType? operationType = null,
        string? sourceSystem = null,
        Guid? bundleId = null,
        DateTimeOffset? requestedFrom = null,
        DateTimeOffset? requestedTo = null,
        CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, pageNumber);
        var size = Math.Clamp(pageSize, 1, 100);
        var query = context.CatalogImportJobs.AsNoTracking();

        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);
        if (operationType.HasValue)
            query = query.Where(x => x.OperationType == operationType.Value);
        if (!string.IsNullOrWhiteSpace(sourceSystem))
            query = query.Where(x => x.SourceSystem == sourceSystem);
        if (bundleId.HasValue)
            query = query.Where(x => x.BundleId == bundleId.Value);
        if (requestedFrom.HasValue)
            query = query.Where(x => x.RequestedAt >= requestedFrom.Value);
        if (requestedTo.HasValue)
            query = query.Where(x => x.RequestedAt <= requestedTo.Value);

        var total = await query.CountAsync(cancellationToken);
        var jobs = await query
            .OrderByDescending(x => x.RequestedAt)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(cancellationToken);

        return (jobs, total);
    }

    public async Task<ExternalCategoryLink?> GetExternalCategoryLinkAsync(
        string sourceSystem,
        string externalCategoryId,
        CancellationToken cancellationToken = default)
    {
        return await context.ExternalCategoryLinks
            .FirstOrDefaultAsync(x =>
                x.SourceSystem == sourceSystem
                && x.ExternalCategoryId == externalCategoryId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<ExternalCategoryLink>> GetExternalCategoryLinksAsync(
        string sourceSystem,
        IEnumerable<string> externalCategoryIds,
        CancellationToken cancellationToken = default)
    {
        var ids = externalCategoryIds.ToHashSet(StringComparer.OrdinalIgnoreCase);
        return await context.ExternalCategoryLinks
            .Where(x => x.SourceSystem == sourceSystem && ids.Contains(x.ExternalCategoryId))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ExternalCategoryAttributeLink>> GetExternalCategoryAttributeLinksAsync(
        string sourceSystem,
        IEnumerable<string> externalCategoryIds,
        CancellationToken cancellationToken = default)
    {
        var ids = externalCategoryIds.ToHashSet(StringComparer.OrdinalIgnoreCase);
        return await context.ExternalCategoryAttributeLinks
            .Where(x => x.SourceSystem == sourceSystem && ids.Contains(x.ExternalCategoryId))
            .ToListAsync(cancellationToken);
    }

    public async Task<int?> GetImportedProductIdBySourceIdentityAsync(
        string sourceSystem,
        string externalProductId,
        CancellationToken cancellationToken = default)
    {
        return await context.CatalogImportBundles
            .Where(bundle => bundle.SourceSystem == sourceSystem)
            .SelectMany(bundle => bundle.Products)
            .Where(product => product.ExternalProductId == externalProductId && product.ImportedProductId.HasValue)
            .Select(product => product.ImportedProductId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task PrepareValidationIssueReplacementAsync(Guid bundleId, CancellationToken cancellationToken = default)
    {
        if (context.Database.IsRelational())
        {
            await context.Set<ImportValidationIssue>()
                .Where(x => x.BundleId == bundleId)
                .ExecuteDeleteAsync(cancellationToken);
        }

        foreach (var entry in context.ChangeTracker
            .Entries<ImportValidationIssue>()
            .Where(x => x.Entity.BundleId == bundleId)
            .ToList())
        {
            entry.State = EntityState.Detached;
        }
    }

    public Task DetachDeletedValidationIssuesAsync(Guid bundleId, CancellationToken cancellationToken = default)
    {
        context.ChangeTracker.DetectChanges();

        foreach (var entry in context.ChangeTracker
            .Entries<ImportValidationIssue>()
            .Where(x => x.Entity.BundleId == bundleId && x.State != EntityState.Added)
            .ToList())
        {
            entry.State = EntityState.Detached;
        }

        return Task.CompletedTask;
    }

    public async Task<(IReadOnlyList<CatalogImportBundle> Bundles, int TotalCount)> ListBundlesAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, pageNumber);
        var size = Math.Clamp(pageSize, 1, 100);
        var query = context.CatalogImportBundles.AsNoTracking();

        var total = await query.CountAsync(cancellationToken);
        var bundles = await query
            .OrderByDescending(x => x.SubmittedAt)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(cancellationToken);

        return (bundles, total);
    }

    public async Task<IReadOnlyList<CatalogImportBundle>> ListRecentAsync(
        int limit,
        CancellationToken cancellationToken = default)
    {
        return await context.CatalogImportBundles
            .AsNoTracking()
            .OrderByDescending(x => x.SubmittedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public Task<int> SaveJobProgressAsync(
        Guid jobId,
        int total,
        int processed,
        CancellationToken cancellationToken = default)
    {
        var lastActivityAt = DateTimeOffset.UtcNow;
        return context.CatalogImportJobs
            .Where(x => x.Id == jobId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.TotalCount, total)
                .SetProperty(x => x.ProcessedCount, processed)
                .SetProperty(x => x.LastActivityAt, lastActivityAt),
                cancellationToken);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => context.SaveChangesAsync(cancellationToken);

    private static IQueryable<CatalogImportBundle> IncludeDetail(IQueryable<CatalogImportBundle> query)
        => query
            .AsSplitQuery()
            .Include(x => x.Sellers)
            .Include(x => x.CategoryMappings)
            .Include(x => x.Products)
                .ThenInclude(x => x.Skus)
            .Include(x => x.Products)
                .ThenInclude(x => x.Attributes)
            .Include(x => x.Products)
                .ThenInclude(x => x.Images)
            .Include(x => x.ValidationIssues)
            .Include(x => x.DuplicateGroups)
            .Include(x => x.SellerOwnershipLinks);
}
