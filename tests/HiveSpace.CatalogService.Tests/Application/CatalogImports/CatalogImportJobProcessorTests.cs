using FluentAssertions;
using HiveSpace.CatalogService.Application.CatalogImports.Commands.ProvisionImportedCategories;
using HiveSpace.CatalogService.Application.CatalogImports.Dtos;
using HiveSpace.CatalogService.Application.CatalogImports.Jobs;
using HiveSpace.CatalogService.Application.CatalogImports.Queueing;
using HiveSpace.CatalogService.Domain.Aggregates.AttributeAggregate;
using HiveSpace.CatalogService.Domain.Aggregates.CategoryAggregate;
using HiveSpace.CatalogService.Domain.CatalogImports;
using HiveSpace.CatalogService.Domain.CatalogImports.Enums;
using HiveSpace.CatalogService.Domain.Repositories;
using System.Reflection;
using Xunit;

namespace HiveSpace.CatalogService.Tests.Application.CatalogImports;

public class CatalogImportJobProcessorTests
{
    [Fact]
    public async Task ProcessJobAsync_WhenJobClaimFails_LeavesJobUnchanged()
    {
        var repository = new CatalogImportBundleRepositoryFake { TryStartJobResult = false };
        var job = CreateProvisionCategoriesJob();
        repository.AddJob(job);

        await new CatalogImportJobProcessor(
            repository,
            new CategoryRepositoryFake(),
            new AttributeRepositoryFake())
            .ProcessJobAsync(job.Id, CancellationToken.None);

        job.Status.Should().Be(CatalogImportJobStatus.Pending);
        repository.SaveChangesCallCount.Should().Be(0);
    }

    [Fact]
    public async Task ProcessJobAsync_WhenTrackedJobStatusIsStaleAfterClaim_CompletesJob()
    {
        var repository = new CatalogImportBundleRepositoryFake { TryStartMutatesJob = false };
        var job = CreateProvisionCategoriesJob();
        repository.AddJob(job);

        await new CatalogImportJobProcessor(
            repository,
            new CategoryRepositoryFake(),
            new AttributeRepositoryFake())
            .ProcessJobAsync(job.Id, CancellationToken.None);

        job.Status.Should().Be(CatalogImportJobStatus.Completed);
        job.ProcessedCount.Should().Be(1);
    }

    [Fact]
    public async Task ProcessQueuedJobAsync_WhenPendingJobMatchesWorkItem_ClaimsAndCompletesJob()
    {
        var repository = new CatalogImportBundleRepositoryFake { TryStartMutatesJob = false };
        var job = CreateProvisionCategoriesJob();
        repository.AddJob(job);
        var workItem = CreateWorkItem(job);

        await new CatalogImportJobProcessor(
            repository,
            new CategoryRepositoryFake(),
            new AttributeRepositoryFake())
            .ProcessQueuedJobAsync(workItem, CancellationToken.None);

        job.Status.Should().Be(CatalogImportJobStatus.Completed);
        job.ProcessedCount.Should().Be(1);
        repository.TryStartJobWithAttemptCallCount.Should().Be(1);
    }

    [Fact]
    public async Task ProcessQueuedJobAsync_WhenAttemptIsStale_LeavesJobUnchanged()
    {
        var repository = new CatalogImportBundleRepositoryFake();
        var job = CreateProvisionCategoriesJob();
        repository.AddJob(job);
        var workItem = CreateWorkItem(job) with { Attempt = job.Attempt + 1 };

        await new CatalogImportJobProcessor(
            repository,
            new CategoryRepositoryFake(),
            new AttributeRepositoryFake())
            .ProcessQueuedJobAsync(workItem, CancellationToken.None);

        job.Status.Should().Be(CatalogImportJobStatus.Pending);
        repository.TryStartJobWithAttemptCallCount.Should().Be(0);
        repository.SaveChangesCallCount.Should().Be(0);
    }

    [Fact]
    public async Task ProcessQueuedJobAsync_WhenOperationMismatches_LeavesJobUnchanged()
    {
        var repository = new CatalogImportBundleRepositoryFake();
        var job = CreateProvisionCategoriesJob();
        repository.AddJob(job);
        var workItem = CreateWorkItem(job) with { OperationType = CatalogImportJobOperationType.ValidateBundle };

        await new CatalogImportJobProcessor(
            repository,
            new CategoryRepositoryFake(),
            new AttributeRepositoryFake())
            .ProcessQueuedJobAsync(workItem, CancellationToken.None);

        job.Status.Should().Be(CatalogImportJobStatus.Pending);
        repository.TryStartJobWithAttemptCallCount.Should().Be(0);
        repository.SaveChangesCallCount.Should().Be(0);
    }

    [Fact]
    public async Task ProcessQueuedJobAsync_WhenJobIsTerminal_LeavesJobUnchanged()
    {
        var repository = new CatalogImportBundleRepositoryFake();
        var job = CreateProvisionCategoriesJob();
        job.Start();
        job.Complete("{}");
        repository.AddJob(job);
        var workItem = CreateWorkItem(job);

        await new CatalogImportJobProcessor(
            repository,
            new CategoryRepositoryFake(),
            new AttributeRepositoryFake())
            .ProcessQueuedJobAsync(workItem, CancellationToken.None);

        job.Status.Should().Be(CatalogImportJobStatus.Completed);
        repository.TryStartJobWithAttemptCallCount.Should().Be(0);
    }

    [Fact]
    public async Task RecoverStaleRunningJobsAsync_WhenRunningJobIsStale_MarksFailed()
    {
        var repository = new CatalogImportBundleRepositoryFake();
        var job = CreateProvisionCategoriesJob();
        job.Start();
        SetProperty(job, nameof(CatalogImportJob.LastActivityAt), DateTimeOffset.UtcNow.AddMinutes(-10));
        repository.AddJob(job);

        var recovered = await new CatalogImportJobProcessor(
            repository,
            new CategoryRepositoryFake(),
            new AttributeRepositoryFake())
            .RecoverStaleRunningJobsAsync(TimeSpan.FromMinutes(5), 5, CancellationToken.None);

        recovered.Should().Be(1);
        job.Status.Should().Be(CatalogImportJobStatus.Failed);
        job.ErrorSummary.Should().Contain("stalled while running");
    }

    [Fact]
    public async Task RecoverStaleRunningJobsAsync_WhenRunningJobHasRecentActivity_DoesNotMarkFailed()
    {
        var repository = new CatalogImportBundleRepositoryFake();
        var job = CreateProvisionCategoriesJob();
        job.Start();
        repository.AddJob(job);

        var recovered = await new CatalogImportJobProcessor(
            repository,
            new CategoryRepositoryFake(),
            new AttributeRepositoryFake())
            .RecoverStaleRunningJobsAsync(TimeSpan.FromMinutes(5), 5, CancellationToken.None);

        recovered.Should().Be(0);
        job.Status.Should().Be(CatalogImportJobStatus.Running);
    }

    [Fact]
    public async Task ProcessJobAsync_WhenProvisioningCategoryAttributes_PersistsProgressBeforeCompletion()
    {
        var repository = new CatalogImportBundleRepositoryFake();
        var categoryRepository = new CategoryRepositoryFake();
        var attributeRepository = new AttributeRepositoryFake();
        var requestedByUserId = Guid.NewGuid();

        categoryRepository.Categories.Add(new Category(1, "Nha sach Tiki", isActive: true));
        repository.CategoryLinks.Add(ExternalCategoryLink.Create(
            "tiki",
            "1846",
            "Nha sach Tiki",
            null,
            "[\"Nha sach Tiki\"]",
            1,
            "sha256:categories",
            requestedByUserId));

        var job = CatalogImportJob.Create(
            CatalogImportJobOperationType.ProvisionCategoryAttributes,
            "tiki",
            requestedByUserId,
            sourceFingerprint: "sha256:attrs-heartbeat",
            requestPayloadJson: System.Text.Json.JsonSerializer.Serialize(new CategoryAttributeProvisioningRequestDto(
                "2026-08-16",
                new CategoryProvisioningSourceDto("tiki", "sellercenter_category_attributes", "parent:2", "https://sellercenter.tiki.vn/api/tiki_api?path=catalog%2Fattributes"),
                new CategoryProvisioningCrawlDto(DateTimeOffset.UtcNow.AddMinutes(-5), DateTimeOffset.UtcNow, "sha256:attrs-heartbeat", null),
                [
                    new CategoryAttributeProvisioningCategoryDto(
                        "1846",
                        "1846",
                        [
                            new CategoryAttributeProvisioningAttributeDto(
                                "brand",
                                "Brand",
                                "Dropdown",
                                true,
                                [
                                    new CategoryAttributeProvisioningAttributeValueDto("apple", "apple", "Apple"),
                                    new CategoryAttributeProvisioningAttributeValueDto("samsung", "samsung", "Samsung")
                                ])
                        ])
                ])));
        repository.AddJob(job);

        await new CatalogImportJobProcessor(
            repository,
            categoryRepository,
            attributeRepository)
            .ProcessJobAsync(job.Id, CancellationToken.None);

        job.Status.Should().Be(CatalogImportJobStatus.Completed);
        job.ProcessedCount.Should().Be(1);
        repository.SaveChangesCallCount.Should().BeGreaterThanOrEqualTo(7);
    }

    [Fact]
    public async Task ProcessJobAsync_WhenSubmittingBundleWithMissingCategoryLink_AddsUnmappedPlaceholder()
    {
        var repository = new CatalogImportBundleRepositoryFake();
        var request = SubmitCatalogImportBundleCommandHandlerTestsHelper.CreateRequest("sha256:missing-category-link");
        var job = CatalogImportJob.Create(
            CatalogImportJobOperationType.SubmitBundle,
            "tiki",
            Guid.NewGuid(),
            sourceFingerprint: request.Crawl.SourceFingerprint,
            requestPayloadJson: System.Text.Json.JsonSerializer.Serialize(request));
        repository.AddJob(job);

        await new CatalogImportJobProcessor(
            repository,
            new CategoryRepositoryFake(),
            new AttributeRepositoryFake())
            .ProcessJobAsync(job.Id, CancellationToken.None);

        var bundle = repository.Bundles.Should().ContainSingle().Subject;
        bundle.CategoryLinks.Should().ContainSingle(x =>
            x.ExternalCategoryId == "1846"
            && !x.HiveSpaceCategoryId.HasValue
            && x.MappingStatus == ImportedCategoryMappingStatus.Unmapped);
    }

    [Fact]
    public async Task ProcessJobAsync_WhenValidatingBundle_PreparesValidationIssueReplacement()
    {
        var repository = new CatalogImportBundleRepositoryFake();
        var bundle = ValidateCatalogImportBundleCommandHandlerTests.CreateBundle(provisionCategory: true, priceAmount: 125000);
        repository.Add(bundle);
        var job = CatalogImportJob.Create(
            CatalogImportJobOperationType.ValidateBundle,
            bundle.SourceSystem,
            Guid.NewGuid(),
            sourceFingerprint: bundle.SourceFingerprint,
            bundleId: bundle.Id);
        repository.AddJob(job);

        await new CatalogImportJobProcessor(
            repository,
            new CategoryRepositoryFake(),
            new AttributeRepositoryFake(),
            productRepository: new ValidateCatalogImportBundleCommandHandlerTests.ProductRepositoryFake(),
            currencyPolicyRepository: new ValidateCatalogImportBundleCommandHandlerTests.PlatformCurrencyPolicyRefRepositoryFake(enabled: true))
            .ProcessJobAsync(job.Id, CancellationToken.None);

        job.Status.Should().Be(CatalogImportJobStatus.Completed);
        repository.PreparedValidationIssueReplacementBundleIds.Should().ContainSingle().Which.Should().Be(bundle.Id);
        repository.DetachedDeletedValidationIssueBundleIds.Should().ContainSingle().Which.Should().Be(bundle.Id);
    }

    private static CatalogImportJob CreateProvisionCategoriesJob()
        => CatalogImportJob.Create(
            CatalogImportJobOperationType.ProvisionCategories,
            "tiki",
            Guid.NewGuid(),
            sourceFingerprint: "sha256:categories-test",
            requestPayloadJson: System.Text.Json.JsonSerializer.Serialize(new CategoryProvisioningRequestDto(
                "2026-07-30",
                new CategoryProvisioningSourceDto("tiki", "sellercenter_categories", "parent:2", "https://sellercenter.tiki.vn/api/tiki_api?path=catalog%2Fcategories"),
                new CategoryProvisioningCrawlDto(DateTimeOffset.UtcNow.AddMinutes(-5), DateTimeOffset.UtcNow, "sha256:categories-test", null),
                [
                    new CategoryProvisioningCategoryDto(
                        "1846",
                        null,
                        "Nha sach Tiki",
                        ["Nha sach Tiki"],
                        "1846",
                        "https://cdn.example.com/categories/1846.png",
                    "category-file-1846",
                    null)
                ])));

    private static CatalogImportQueueWorkItem CreateWorkItem(CatalogImportJob job)
        => new(
            job.Id,
            job.OperationType,
            job.Attempt,
            Guid.NewGuid().ToString("N"),
            DateTimeOffset.UtcNow,
            job.RequestedByUserId,
            job.BundleId);

    private static void SetProperty(object target, string propertyName, object? value)
    {
        var property = target.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException($"Property '{propertyName}' was not found.");
        property.SetValue(target, value);
    }

    private sealed class CatalogImportBundleRepositoryFake : ICatalogImportBundleRepository
    {
        public List<CatalogImportJob> Jobs { get; } = [];
        public List<ExternalCategoryLink> CategoryLinks { get; } = [];
        public List<CatalogImportBundle> Bundles { get; } = [];
        public List<Guid> PreparedValidationIssueReplacementBundleIds { get; } = [];
        public List<Guid> DetachedDeletedValidationIssueBundleIds { get; } = [];
        public int SaveChangesCallCount { get; private set; }
        public int TryStartJobWithAttemptCallCount { get; private set; }
        public bool TryStartJobResult { get; init; } = true;
        public bool TryStartMutatesJob { get; init; } = true;

        public void Add(CatalogImportBundle bundle)
        {
            Bundles.Add(bundle);
        }

        public void AddJob(CatalogImportJob job)
            => Jobs.Add(job);

        public void AddExternalCategoryLink(ExternalCategoryLink categoryLink)
            => CategoryLinks.Add(categoryLink);

        public void AddExternalCategoryAttributeLink(ExternalCategoryAttributeLink categoryAttributeLink)
        {
        }

        public Task<CatalogImportBundle?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(Bundles.FirstOrDefault(x => x.Id == id));

        public Task<CatalogImportBundle?> GetBySourceFingerprintAsync(string sourceFingerprint, CancellationToken cancellationToken = default)
            => Task.FromResult<CatalogImportBundle?>(null);

        public Task<CatalogImportJob?> GetJobByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(Jobs.FirstOrDefault(x => x.Id == id));

        public Task<CatalogImportJob?> GetJobByOperationAndSourceFingerprintAsync(
            CatalogImportJobOperationType operationType,
            string sourceSystem,
            string sourceFingerprint,
            CancellationToken cancellationToken = default)
            => Task.FromResult(Jobs.FirstOrDefault(x =>
                x.OperationType == operationType
                && x.SourceSystem == sourceSystem
                && x.SourceFingerprint == sourceFingerprint));

        public Task<CatalogImportJob?> GetActiveJobByOperationAndSourceFingerprintAsync(
            CatalogImportJobOperationType operationType,
            string sourceSystem,
            string sourceFingerprint,
            CancellationToken cancellationToken = default)
            => Task.FromResult(Jobs.FirstOrDefault(x =>
                x.OperationType == operationType
                && x.SourceSystem == sourceSystem
                && x.SourceFingerprint == sourceFingerprint
                && x.Status is CatalogImportJobStatus.Pending or CatalogImportJobStatus.Running));

        public Task<bool> TryStartJobAsync(Guid jobId, CancellationToken cancellationToken = default)
        {
            if (!TryStartJobResult)
                return Task.FromResult(false);

            var job = Jobs.FirstOrDefault(x => x.Id == jobId);
            if (job is null || job.Status != CatalogImportJobStatus.Pending)
                return Task.FromResult(false);

            if (TryStartMutatesJob)
                job.Start();
            SaveChangesCallCount++;
            return Task.FromResult(true);
        }

        public Task<bool> TryStartJobAsync(
            Guid jobId,
            CatalogImportJobOperationType operationType,
            int attempt,
            CancellationToken cancellationToken = default)
        {
            TryStartJobWithAttemptCallCount++;
            if (!TryStartJobResult)
                return Task.FromResult(false);

            var job = Jobs.FirstOrDefault(x => x.Id == jobId);
            if (job is null
                || job.OperationType != operationType
                || !job.CanProcessAttempt(attempt))
            {
                return Task.FromResult(false);
            }

            if (TryStartMutatesJob)
                job.Start();
            SaveChangesCallCount++;
            return Task.FromResult(true);
        }

        public Task<int> MarkJobFailedAsync(Guid jobId, string errorSummary, CancellationToken cancellationToken = default)
        {
            var job = Jobs.FirstOrDefault(x => x.Id == jobId);
            if (job is null || job.Status is not (CatalogImportJobStatus.Pending or CatalogImportJobStatus.Running))
                return Task.FromResult(0);

            var completedAt = DateTimeOffset.UtcNow;
            SetProperty(job, nameof(CatalogImportJob.Status), CatalogImportJobStatus.Failed);
            SetProperty(job, nameof(CatalogImportJob.CompletedAt), completedAt);
            SetProperty(job, nameof(CatalogImportJob.LastActivityAt), completedAt);
            SetProperty(job, nameof(CatalogImportJob.ErrorSummary), errorSummary.Trim());
            SaveChangesCallCount++;
            return Task.FromResult(1);
        }

        public Task<IReadOnlyList<CatalogImportJob>> GetPendingJobsAsync(int limit, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<CatalogImportJob>>(Jobs
                .Where(x => x.Status == CatalogImportJobStatus.Pending)
                .Take(limit)
                .ToList());

        public Task<IReadOnlyList<CatalogImportJob>> GetRunningJobsInactiveSinceAsync(
            DateTimeOffset inactiveSince,
            int limit,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<CatalogImportJob>>(Jobs
                .Where(x =>
                    x.Status == CatalogImportJobStatus.Running
                    && x.LastActivityAt <= inactiveSince)
                .Take(limit)
                .ToList());

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
            => Task.FromResult(((IReadOnlyList<CatalogImportJob>)Jobs, Jobs.Count));

        public Task<ExternalCategoryLink?> GetExternalCategoryLinkAsync(
            string sourceSystem,
            string externalCategoryId,
            CancellationToken cancellationToken = default)
            => Task.FromResult(CategoryLinks.FirstOrDefault(x =>
                x.SourceSystem == sourceSystem && x.ExternalCategoryId == externalCategoryId));

        public Task<IReadOnlyList<ExternalCategoryLink>> GetExternalCategoryLinksAsync(
            string sourceSystem,
            IEnumerable<string> externalCategoryIds,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<ExternalCategoryLink>>(CategoryLinks
                .Where(x => x.SourceSystem == sourceSystem && externalCategoryIds.Contains(x.ExternalCategoryId))
                .ToList());

        public Task<IReadOnlyList<ExternalCategoryAttributeLink>> GetExternalCategoryAttributeLinksAsync(
            string sourceSystem,
            IEnumerable<string> externalCategoryIds,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<ExternalCategoryAttributeLink>>([]);

        public Task<int?> GetImportedProductIdBySourceIdentityAsync(
            string sourceSystem,
            string externalProductId,
            CancellationToken cancellationToken = default)
            => Task.FromResult<int?>(null);

        public Task PrepareValidationIssueReplacementAsync(Guid bundleId, CancellationToken cancellationToken = default)
        {
            PreparedValidationIssueReplacementBundleIds.Add(bundleId);
            return Task.CompletedTask;
        }

        public Task DetachDeletedValidationIssuesAsync(Guid bundleId, CancellationToken cancellationToken = default)
        {
            DetachedDeletedValidationIssueBundleIds.Add(bundleId);
            return Task.CompletedTask;
        }

        public Task<(IReadOnlyList<CatalogImportBundle> Bundles, int TotalCount)> ListBundlesAsync(
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
            => Task.FromResult(((IReadOnlyList<CatalogImportBundle>)[], 0));

        public Task<IReadOnlyList<CatalogImportBundle>> ListRecentAsync(int limit, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<CatalogImportBundle>>([]);

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesCallCount++;
            return Task.FromResult(1);
        }
    }

    private sealed class AttributeRepositoryFake : IAttributeRepository
    {
        private int _nextAttributeId = 1;
        private int _nextAttributeValueId = 1;

        public Task<AttributeDefinition?> GetByIdAsync(Guid id)
            => Task.FromResult<AttributeDefinition?>(null);

        public Task<List<AttributeDefinition>> GetAllAsync()
            => Task.FromResult(new List<AttributeDefinition>());

        public Task<List<AttributeDefinition>> GetByNamesAsync(IReadOnlyCollection<string> names)
            => Task.FromResult(new List<AttributeDefinition>());

        public Task AddAsync(AttributeDefinition attribute)
        {
            AssignAttributeId(attribute);
            AssignValueIds(attribute);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(AttributeDefinition attribute)
        {
            AssignAttributeId(attribute);
            AssignValueIds(attribute);
            return Task.CompletedTask;
        }

        public void Remove(AttributeDefinition attribute)
        {
        }

        public Task<int> SaveChangesAsync()
            => Task.FromResult(1);

        private void AssignAttributeId(AttributeDefinition attribute)
        {
            if (attribute.Id > 0)
                return;

            SetProperty(attribute, "Id", _nextAttributeId++);
        }

        private void AssignValueIds(AttributeDefinition attribute)
        {
            foreach (var value in attribute.Values.Where(x => x.Id <= 0))
                SetProperty(value, "Id", _nextAttributeValueId++);
        }
    }

    private sealed class CategoryRepositoryFake : ICategoryRepository
    {
        public List<Category> Categories { get; } = [];

        public Task<Category?> GetByIdAsync(int id)
            => Task.FromResult(Categories.FirstOrDefault(x => x.Id == id));

        public Task<List<Category>> GetAllAsync()
            => Task.FromResult(Categories);

        public Task AddAsync(Category category)
        {
            Categories.Add(category);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Category category)
            => Task.CompletedTask;

        public void Remove(Category category)
            => Categories.Remove(category);

        public Task<int> SaveChangesAsync()
            => Task.FromResult(1);
    }
}
