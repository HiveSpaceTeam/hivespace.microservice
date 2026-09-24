using FluentAssertions;
using HiveSpace.CatalogService.Application.CatalogImports.Jobs;
using HiveSpace.CatalogService.Application.CatalogImports.Commands.ProvisionImportedCategories;
using HiveSpace.CatalogService.Application.CatalogImports.Dtos;
using HiveSpace.CatalogService.Domain.Aggregates.CategoryAggregate;
using HiveSpace.CatalogService.Domain.CatalogImports;
using HiveSpace.CatalogService.Domain.Repositories;
using Xunit;

namespace HiveSpace.CatalogService.Tests.Application.CatalogImports;

public class ProvisionImportedCategoriesCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithNewCategory_CreatesCategoryAndExternalLink()
    {
        var repository = new CatalogImportBundleRepositoryFake();
        var categoryRepository = new CategoryRepositoryFake();
        var payload = CreateRequest("sha256:categories-1");

        var result = await new ProvisionImportedCategoriesCommandHandler(repository, new NullCatalogImportJobScheduler())
            .Handle(new ProvisionImportedCategoriesCommand(payload, Guid.NewGuid()), CancellationToken.None);
        await new CatalogImportJobProcessor(repository, categoryRepository)
            .ProcessJobAsync(result.JobId, CancellationToken.None);
        var job = await repository.GetJobByIdAsync(result.JobId, CancellationToken.None);

        result.Status.Should().Be("Pending");
        result.OperationType.Should().Be("ProvisionCategories");
        job!.Status.ToString().Should().Be("Completed");
        job.TotalCount.Should().Be(1);
        job.CreatedCount.Should().Be(1);
        categoryRepository.Categories.Should().ContainSingle(x =>
            x.ImageFileId == "category-file-1846"
            && x.ImageUrl == "https://cdn.example.com/categories/1846.png");
        repository.CategoryLinks.Should().ContainSingle(x =>
            x.SourceSystem == "tiki"
            && x.ExternalCategoryId == "1846"
            && x.HiveSpaceCategoryId == 1);
    }

    [Fact]
    public async Task Handle_WithExistingExternalLink_ReturnsMatchedWithoutDuplicateCategory()
    {
        var repository = new CatalogImportBundleRepositoryFake();
        var categoryRepository = new CategoryRepositoryFake();
        var payload = CreateRequest("sha256:categories-2");
        var handler = new ProvisionImportedCategoriesCommandHandler(repository, new NullCatalogImportJobScheduler());

        var first = await handler.Handle(new ProvisionImportedCategoriesCommand(payload, Guid.NewGuid()), CancellationToken.None);
        await new CatalogImportJobProcessor(repository, categoryRepository)
            .ProcessJobAsync(first.JobId, CancellationToken.None);
        var result = await handler.Handle(new ProvisionImportedCategoriesCommand(payload, Guid.NewGuid()), CancellationToken.None);

        result.JobId.Should().Be(first.JobId);
        repository.CategoryLinks.Should().ContainSingle(x => x.ExternalCategoryId == "1846");
        categoryRepository.Categories.Should().ContainSingle();
    }

    [Fact]
    public async Task Handle_WithExistingExternalLink_UpdatesCategoryImage()
    {
        var repository = new CatalogImportBundleRepositoryFake();
        var categoryRepository = new CategoryRepositoryFake();
        categoryRepository.Categories.Add(new Category(10, "Nha sach Tiki", isActive: true));
        repository.CategoryLinks.Add(ExternalCategoryLink.Create(
            "tiki",
            "1846",
            "Nha sach Tiki",
            null,
            null,
            10,
            "sha256:previous",
            Guid.NewGuid()));

        var result = await new ProvisionImportedCategoriesCommandHandler(repository, new NullCatalogImportJobScheduler())
            .Handle(new ProvisionImportedCategoriesCommand(CreateRequest("sha256:categories-4"), Guid.NewGuid()), CancellationToken.None);
        await new CatalogImportJobProcessor(repository, categoryRepository)
            .ProcessJobAsync(result.JobId, CancellationToken.None);

        categoryRepository.Categories.Should().ContainSingle(x =>
            x.Id == 10
            && x.ImageFileId == "category-file-1846"
            && x.ImageUrl == "https://cdn.example.com/categories/1846.png");
    }

    [Fact]
    public async Task Handle_WithExistingCategoryName_MatchesCategoryAndCreatesExternalLink()
    {
        var repository = new CatalogImportBundleRepositoryFake();
        var categoryRepository = new CategoryRepositoryFake();
        categoryRepository.Categories.Add(new Category(10, "Nha sach Tiki", isActive: true));

        var result = await new ProvisionImportedCategoriesCommandHandler(repository, new NullCatalogImportJobScheduler())
            .Handle(new ProvisionImportedCategoriesCommand(CreateRequest("sha256:categories-3"), Guid.NewGuid()), CancellationToken.None);
        await new CatalogImportJobProcessor(repository, categoryRepository)
            .ProcessJobAsync(result.JobId, CancellationToken.None);
        var job = await repository.GetJobByIdAsync(result.JobId, CancellationToken.None);

        job!.MatchedCount.Should().Be(1);
        categoryRepository.Categories.Should().ContainSingle();
    }

    [Fact]
    public async Task Handle_WithParentAndChildCategories_ProvisionsChildWithResolvedParentId()
    {
        var repository = new CatalogImportBundleRepositoryFake();
        var categoryRepository = new CategoryRepositoryFake();

        var result = await new ProvisionImportedCategoriesCommandHandler(repository, new NullCatalogImportJobScheduler())
            .Handle(new ProvisionImportedCategoriesCommand(CreateRequest(
                "sha256:categories-parent-child",
                [
                    new CategoryProvisioningCategoryDto(
                        "100",
                        null,
                        "Books",
                        ["Books"],
                        "100",
                        null,
                        null,
                        null),
                    new CategoryProvisioningCategoryDto(
                        "101",
                        "100",
                        "Fiction",
                        ["Books", "Fiction"],
                        "101",
                        null,
                        null,
                        null)
                ]), Guid.NewGuid()), CancellationToken.None);
        await new CatalogImportJobProcessor(repository, categoryRepository)
            .ProcessJobAsync(result.JobId, CancellationToken.None);

        categoryRepository.Categories.Should().ContainSingle(x => x.Id == 1 && x.ParentId == null && x.Name == "Books");
        categoryRepository.Categories.Should().ContainSingle(x => x.Id == 2 && x.ParentId == 1 && x.Name == "Fiction");
    }

    [Fact]
    public async Task Handle_WithGrandchildChain_ProvisionsCategoriesInDependencyOrder()
    {
        var repository = new CatalogImportBundleRepositoryFake();
        var categoryRepository = new CategoryRepositoryFake();

        var result = await new ProvisionImportedCategoriesCommandHandler(repository, new NullCatalogImportJobScheduler())
            .Handle(new ProvisionImportedCategoriesCommand(CreateRequest(
                "sha256:categories-chain",
                [
                    new CategoryProvisioningCategoryDto("102", "101", "Fantasy", ["Books", "Fiction", "Fantasy"], null, null, null, null),
                    new CategoryProvisioningCategoryDto("100", null, "Books", ["Books"], null, null, null, null),
                    new CategoryProvisioningCategoryDto("101", "100", "Fiction", ["Books", "Fiction"], null, null, null, null)
                ]), Guid.NewGuid()), CancellationToken.None);
        await new CatalogImportJobProcessor(repository, categoryRepository)
            .ProcessJobAsync(result.JobId, CancellationToken.None);

        categoryRepository.Categories.Should().ContainSingle(x => x.Id == 1 && x.ParentId == null && x.Name == "Books");
        categoryRepository.Categories.Should().ContainSingle(x => x.Id == 2 && x.ParentId == 1 && x.Name == "Fiction");
        categoryRepository.Categories.Should().ContainSingle(x => x.Id == 3 && x.ParentId == 2 && x.Name == "Fantasy");
    }

    [Fact]
    public async Task Handle_WithMissingParentCategory_MarksCategoryAsFailedInsteadOfCreatingRoot()
    {
        var repository = new CatalogImportBundleRepositoryFake();
        var categoryRepository = new CategoryRepositoryFake();

        var result = await new ProvisionImportedCategoriesCommandHandler(repository, new NullCatalogImportJobScheduler())
            .Handle(new ProvisionImportedCategoriesCommand(CreateRequest(
                "sha256:categories-missing-parent",
                [
                    new CategoryProvisioningCategoryDto(
                        "101",
                        "999",
                        "Fiction",
                        ["Books", "Fiction"],
                        null,
                        null,
                        null,
                        null)
                ]), Guid.NewGuid()), CancellationToken.None);
        await new CatalogImportJobProcessor(repository, categoryRepository)
            .ProcessJobAsync(result.JobId, CancellationToken.None);
        var job = await repository.GetJobByIdAsync(result.JobId, CancellationToken.None);

        categoryRepository.Categories.Should().BeEmpty();
        repository.CategoryLinks.Should().BeEmpty();
        job!.FailedCount.Should().Be(1);
        job.ResultSummaryJson.Should().Contain("UnresolvedParentCategory:999");
    }

    private static CategoryProvisioningRequestDto CreateRequest(
        string sourceFingerprint,
        IReadOnlyCollection<CategoryProvisioningCategoryDto>? categories = null)
        => new(
            "2026-07-30",
            new CategoryProvisioningSourceDto("tiki", "sellercenter_categories", "parent:2", "https://sellercenter.tiki.vn/api/tiki_api?path=catalog%2Fcategories"),
            new CategoryProvisioningCrawlDto(DateTimeOffset.UtcNow.AddMinutes(-5), DateTimeOffset.UtcNow, sourceFingerprint, null),
            categories ??
            [new CategoryProvisioningCategoryDto(
                "1846",
                null,
                "Nha sach Tiki",
                ["Nha sach Tiki"],
                "1846",
                "https://cdn.example.com/categories/1846.png",
                "category-file-1846",
                null)]);

    private sealed class CatalogImportBundleRepositoryFake : ICatalogImportBundleRepository
    {
        public List<ExternalCategoryLink> CategoryLinks { get; } = [];
        public List<CatalogImportJob> Jobs { get; } = [];

        public void Add(CatalogImportBundle bundle)
        {
        }

        public void AddJob(CatalogImportJob job)
            => Jobs.Add(job);

        public void AddExternalCategoryLink(ExternalCategoryLink categoryLink)
            => CategoryLinks.Add(categoryLink);

        public void AddExternalCategoryAttributeLink(ExternalCategoryAttributeLink categoryAttributeLink)
        {
        }
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
        {
            var ids = externalCategoryIds.ToHashSet(StringComparer.OrdinalIgnoreCase);
            return Task.FromResult<IReadOnlyList<ExternalCategoryLink>>(
                CategoryLinks.Where(x => x.SourceSystem == sourceSystem && ids.Contains(x.ExternalCategoryId)).ToList());
        }

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

        public Task<CatalogImportBundle?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult<CatalogImportBundle?>(null);

        public Task<CatalogImportBundle?> GetBySourceFingerprintAsync(string sourceFingerprint, CancellationToken cancellationToken = default)
            => Task.FromResult<CatalogImportBundle?>(null);

        public Task<CatalogImportJob?> GetJobByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(Jobs.FirstOrDefault(x => x.Id == id));

        public Task<CatalogImportJob?> GetJobByOperationAndSourceFingerprintAsync(
            HiveSpace.CatalogService.Domain.CatalogImports.Enums.CatalogImportJobOperationType operationType,
            string sourceSystem,
            string sourceFingerprint,
            CancellationToken cancellationToken = default)
            => Task.FromResult(Jobs.FirstOrDefault(x =>
                x.OperationType == operationType
                && x.SourceSystem == sourceSystem
                && x.SourceFingerprint == sourceFingerprint));

        public Task<CatalogImportJob?> GetActiveJobByOperationAndSourceFingerprintAsync(
            HiveSpace.CatalogService.Domain.CatalogImports.Enums.CatalogImportJobOperationType operationType,
            string sourceSystem,
            string sourceFingerprint,
            CancellationToken cancellationToken = default)
            => Task.FromResult(Jobs.FirstOrDefault(x =>
                x.OperationType == operationType
                && x.SourceSystem == sourceSystem
                && x.SourceFingerprint == sourceFingerprint
                && x.Status is HiveSpace.CatalogService.Domain.CatalogImports.Enums.CatalogImportJobStatus.Pending
                    or HiveSpace.CatalogService.Domain.CatalogImports.Enums.CatalogImportJobStatus.Running));

        public Task<IReadOnlyList<CatalogImportJob>> GetPendingJobsAsync(int limit, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<CatalogImportJob>>(Jobs
                .Where(x => x.Status == HiveSpace.CatalogService.Domain.CatalogImports.Enums.CatalogImportJobStatus.Pending)
                .Take(limit)
                .ToList());

        public Task<IReadOnlyList<CatalogImportJob>> GetRunningJobsInactiveSinceAsync(
            DateTimeOffset inactiveSince,
            int limit,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<CatalogImportJob>>(Jobs
                .Where(x =>
                    x.Status == HiveSpace.CatalogService.Domain.CatalogImports.Enums.CatalogImportJobStatus.Running
                    && x.LastActivityAt <= inactiveSince)
                .Take(limit)
                .ToList());


        public Task<(IReadOnlyList<CatalogImportJob> Jobs, int TotalCount)> ListJobsAsync(
            int pageNumber,
            int pageSize,
            HiveSpace.CatalogService.Domain.CatalogImports.Enums.CatalogImportJobStatus? status = null,
            HiveSpace.CatalogService.Domain.CatalogImports.Enums.CatalogImportJobOperationType? operationType = null,
            string? sourceSystem = null,
            Guid? bundleId = null,
            DateTimeOffset? requestedFrom = null,
            DateTimeOffset? requestedTo = null,
            CancellationToken cancellationToken = default)
            => Task.FromResult(((IReadOnlyList<CatalogImportJob>)Jobs, Jobs.Count));

        public Task<(IReadOnlyList<CatalogImportBundle> Bundles, int TotalCount)> ListBundlesAsync(
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
            => Task.FromResult(((IReadOnlyList<CatalogImportBundle>)[], 0));


        public Task<IReadOnlyList<CatalogImportBundle>> ListRecentAsync(int limit, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<CatalogImportBundle>>([]);

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(1);
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
        {
            Categories.Remove(category);
        }

        public Task<int> SaveChangesAsync()
            => Task.FromResult(1);
    }
}
