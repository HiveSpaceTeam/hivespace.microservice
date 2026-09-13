using FluentAssertions;
using HiveSpace.CatalogService.Application.CatalogImports.Queries.ListCatalogImportJobs;
using HiveSpace.CatalogService.Domain.CatalogImports;
using HiveSpace.CatalogService.Domain.CatalogImports.Enums;
using Xunit;

namespace HiveSpace.CatalogService.Tests.Application.CatalogImports;

public class ListCatalogImportJobsQueryHandlerTests
{
    [Fact]
    public async Task Handle_WithRequestedWindow_ReturnsOnlyMatchingJobs()
    {
        var oldJob = CreateJob(CatalogImportJobOperationType.SubmitBundle, DateTimeOffset.UtcNow.AddDays(-3));
        var inRangeJob = CreateJob(CatalogImportJobOperationType.ValidateBundle, DateTimeOffset.UtcNow.AddDays(-1));
        var recentJob = CreateJob(CatalogImportJobOperationType.ImportReadyProducts, DateTimeOffset.UtcNow);
        var repository = new RepositoryFake([oldJob, inRangeJob, recentJob]);

        var result = await new ListCatalogImportJobsQueryHandler(repository).Handle(
            new ListCatalogImportJobsQuery(
                1,
                20,
                null,
                null,
                null,
                null,
                null,
                DateTimeOffset.UtcNow.AddDays(-2),
                DateTimeOffset.UtcNow.AddHours(-12)),
            CancellationToken.None);

        result.Data.Should().ContainSingle();
        result.Data.Single().JobId.Should().Be(inRangeJob.Id);
    }

    [Fact]
    public async Task Handle_WithRequestedFromOnly_ExcludesOlderJobs()
    {
        var oldJob = CreateJob(CatalogImportJobOperationType.SubmitBundle, DateTimeOffset.UtcNow.AddDays(-3));
        var recentJob = CreateJob(CatalogImportJobOperationType.ValidateBundle, DateTimeOffset.UtcNow.AddHours(-4));
        var repository = new RepositoryFake([oldJob, recentJob]);

        var result = await new ListCatalogImportJobsQueryHandler(repository).Handle(
            new ListCatalogImportJobsQuery(
                1,
                20,
                null,
                null,
                null,
                null,
                null,
                DateTimeOffset.UtcNow.AddDays(-1),
                null),
            CancellationToken.None);

        result.Data.Should().ContainSingle();
        result.Data.Single().JobId.Should().Be(recentJob.Id);
    }

    private static CatalogImportJob CreateJob(CatalogImportJobOperationType operationType, DateTimeOffset requestedAt)
    {
        var job = CatalogImportJob.Create(
            operationType,
            "tiki",
            Guid.NewGuid(),
            sourceFingerprint: $"sha256:{Guid.NewGuid():N}",
            requestPayloadJson: "{}");

        typeof(CatalogImportJob)
            .GetProperty(nameof(CatalogImportJob.RequestedAt))!
            .SetValue(job, requestedAt);

        return job;
    }

    private sealed class RepositoryFake(IReadOnlyList<CatalogImportJob> jobs) : ICatalogImportBundleRepository
    {
        public void Add(CatalogImportBundle bundle) => throw new NotSupportedException();
        public void AddJob(CatalogImportJob job) => throw new NotSupportedException();
        public void AddExternalCategoryLink(ExternalCategoryLink categoryLink) => throw new NotSupportedException();
        public void AddExternalCategoryAttributeLink(ExternalCategoryAttributeLink categoryAttributeLink) => throw new NotSupportedException();
        public Task<CatalogImportBundle?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<CatalogImportBundle?> GetBySourceFingerprintAsync(string sourceFingerprint, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<CatalogImportJob?> GetJobByIdAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<CatalogImportJob?> GetJobByOperationAndSourceFingerprintAsync(CatalogImportJobOperationType operationType, string sourceSystem, string sourceFingerprint, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<CatalogImportJob?> GetActiveJobByOperationAndSourceFingerprintAsync(CatalogImportJobOperationType operationType, string sourceSystem, string sourceFingerprint, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<IReadOnlyList<CatalogImportJob>> GetPendingJobsAsync(int limit, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<IReadOnlyList<CatalogImportJob>> GetRunningJobsInactiveSinceAsync(DateTimeOffset inactiveSince, int limit, CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task<(IReadOnlyList<CatalogImportJob> Jobs, int TotalCount)> ListJobsAsync(
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
            IEnumerable<CatalogImportJob> query = jobs;

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

            var filtered = query
                .OrderByDescending(x => x.RequestedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Task.FromResult(((IReadOnlyList<CatalogImportJob>)filtered, filtered.Count));
        }

        public Task<ExternalCategoryLink?> GetExternalCategoryLinkAsync(string sourceSystem, string externalCategoryId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<IReadOnlyList<ExternalCategoryLink>> GetExternalCategoryLinksAsync(string sourceSystem, IEnumerable<string> externalCategoryIds, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<IReadOnlyList<ExternalCategoryAttributeLink>> GetExternalCategoryAttributeLinksAsync(string sourceSystem, IEnumerable<string> externalCategoryIds, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<int?> GetImportedProductIdBySourceIdentityAsync(string sourceSystem, string externalProductId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<(IReadOnlyList<CatalogImportBundle> Bundles, int TotalCount)> ListBundlesAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<IReadOnlyList<CatalogImportBundle>> ListRecentAsync(int limit, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
