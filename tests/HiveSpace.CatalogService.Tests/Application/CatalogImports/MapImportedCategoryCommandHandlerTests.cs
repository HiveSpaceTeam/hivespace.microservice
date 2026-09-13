using FluentAssertions;
using HiveSpace.CatalogService.Application.CatalogImports.Commands.MapImportedCategory;
using HiveSpace.CatalogService.Domain.Aggregates.CategoryAggregate;
using HiveSpace.CatalogService.Domain.CatalogImports;
using HiveSpace.CatalogService.Domain.CatalogImports.Enums;
using HiveSpace.CatalogService.Domain.Repositories;
using Xunit;

namespace HiveSpace.CatalogService.Tests.Application.CatalogImports;

public class MapImportedCategoryCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithActiveCategory_MapsCategoryLinkAndReturnsAffectedProductCount()
    {
        var bundle = ValidateCatalogImportBundleCommandHandlerTests.CreateBundle(
            provisionCategory: false,
            priceAmount: 125000);
        bundle.AddCategory("1846", null, "Nha sach Tiki", null);
        var category = new Category(12, "Books", isActive: true);
        var repository = new CatalogImportBundleRepositoryFake(bundle);
        var handler = new MapImportedCategoryCommandHandler(repository, new CategoryRepositoryFake(category));

        var result = await handler.Handle(
            new MapImportedCategoryCommand("1846", bundle.Id, category.Id, Guid.NewGuid()),
            CancellationToken.None);

        result.BundleId.Should().Be(bundle.Id);
        result.ExternalCategoryId.Should().Be("1846");
        result.HiveSpaceCategoryId.Should().Be(category.Id);
        result.MappingStatus.Should().Be("Mapped");
        result.AffectedProductCount.Should().Be(1);
        bundle.CategoryMappings.Single().HiveSpaceCategoryId.Should().Be(category.Id);
    }

    private sealed class CatalogImportBundleRepositoryFake(CatalogImportBundle bundle) : ICatalogImportBundleRepository
    {
        public void Add(CatalogImportBundle bundle)
        {
        }

        public void AddJob(CatalogImportJob job)
        {
        }

        public void AddExternalCategoryLink(ExternalCategoryLink link)
        {
        }

        public void AddExternalCategoryAttributeLink(ExternalCategoryAttributeLink link)
        {
        }

        public Task<CatalogImportBundle?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult<CatalogImportBundle?>(id == bundle.Id ? bundle : null);

        public Task<CatalogImportBundle?> GetBySourceFingerprintAsync(string sourceFingerprint, CancellationToken cancellationToken = default)
            => Task.FromResult<CatalogImportBundle?>(null);

        public Task<CatalogImportJob?> GetJobByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult<CatalogImportJob?>(null);

        public Task<CatalogImportJob?> GetJobByOperationAndSourceFingerprintAsync(
            CatalogImportJobOperationType operationType,
            string sourceSystem,
            string sourceFingerprint,
            CancellationToken cancellationToken = default)
            => Task.FromResult<CatalogImportJob?>(null);

        public Task<CatalogImportJob?> GetActiveJobByOperationAndSourceFingerprintAsync(
            CatalogImportJobOperationType operationType,
            string sourceSystem,
            string sourceFingerprint,
            CancellationToken cancellationToken = default)
            => Task.FromResult<CatalogImportJob?>(null);

        public Task<IReadOnlyList<CatalogImportJob>> GetPendingJobsAsync(int limit, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<CatalogImportJob>>([]);

        public Task<IReadOnlyList<CatalogImportJob>> GetRunningJobsInactiveSinceAsync(
            DateTimeOffset inactiveSince,
            int limit,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<CatalogImportJob>>([]);

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
            => Task.FromResult(((IReadOnlyList<CatalogImportJob>)[], 0));

        public Task<ExternalCategoryLink?> GetExternalCategoryLinkAsync(
            string sourceSystem,
            string externalCategoryId,
            CancellationToken cancellationToken = default)
            => Task.FromResult<ExternalCategoryLink?>(null);

        public Task<IReadOnlyList<ExternalCategoryLink>> GetExternalCategoryLinksAsync(
            string sourceSystem,
            IEnumerable<string> externalCategoryIds,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<ExternalCategoryLink>>([]);

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

        public Task<(IReadOnlyList<CatalogImportBundle> Bundles, int TotalCount)> ListBundlesAsync(
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
            => Task.FromResult(((IReadOnlyList<CatalogImportBundle>)[bundle], 1));

        public Task<IReadOnlyList<CatalogImportBundle>> ListRecentAsync(int limit, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<CatalogImportBundle>>([bundle]);

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(1);
    }

    private sealed class CategoryRepositoryFake(Category category) : ICategoryRepository
    {
        public Task<Category?> GetByIdAsync(int id)
            => Task.FromResult<Category?>(id == category.Id ? category : null);

        public Task<List<Category>> GetAllAsync()
            => Task.FromResult(new List<Category> { category });

        public Task AddAsync(Category category)
            => Task.CompletedTask;

        public Task UpdateAsync(Category category)
            => Task.CompletedTask;

        public void Remove(Category category)
        {
        }

        public Task<int> SaveChangesAsync()
            => Task.FromResult(1);
    }
}
