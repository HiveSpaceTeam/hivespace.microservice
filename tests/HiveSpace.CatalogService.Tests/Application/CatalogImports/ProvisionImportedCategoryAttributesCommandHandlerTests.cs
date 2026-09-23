using FluentAssertions;
using HiveSpace.CatalogService.Application.CatalogImports.Commands.ProvisionImportedCategoryAttributes;
using HiveSpace.CatalogService.Application.CatalogImports.Dtos;
using HiveSpace.CatalogService.Application.CatalogImports.Jobs;
using HiveSpace.CatalogService.Domain.Aggregates.AttributeAggregate;
using HiveSpace.CatalogService.Domain.Aggregates.CategoryAggregate;
using HiveSpace.CatalogService.Domain.CatalogImports;
using HiveSpace.CatalogService.Domain.CatalogImports.Enums;
using HiveSpace.CatalogService.Domain.Repositories;
using System.Reflection;
using Xunit;

namespace HiveSpace.CatalogService.Tests.Application.CatalogImports;

public class ProvisionImportedCategoryAttributesCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithNewFingerprint_ReturnsAcceptedJobAndPersistsAttributeLinks()
    {
        var repository = new CatalogImportBundleRepositoryFake();
        var categoryRepository = new CategoryRepositoryFake();
        var attributeRepository = new AttributeRepositoryFake();
        categoryRepository.Categories.Add(new Category(1, "Nha sach Tiki", isActive: true));
        repository.CategoryLinks.Add(ExternalCategoryLink.Create(
            "tiki",
            "1846",
            "Nha sach Tiki",
            null,
            "[\"Nha sach Tiki\"]",
            1,
            "sha256:categories",
            Guid.NewGuid()));

        var result = await new ProvisionImportedCategoryAttributesCommandHandler(repository, new NullCatalogImportJobScheduler())
            .Handle(new ProvisionImportedCategoryAttributesCommand(CreateRequest("sha256:attrs-1"), Guid.NewGuid()), CancellationToken.None);

        await new CatalogImportJobProcessor(
            repository,
            categoryRepository,
            attributeRepository)
            .ProcessJobAsync(result.JobId, CancellationToken.None);

        result.Status.Should().Be("Pending");
        result.OperationType.Should().Be("ProvisionCategoryAttributes");
        repository.AttributeLinks.Should().ContainSingle(x =>
            x.ExternalCategoryId == "1846"
            && x.SourceAttributeId == "brand"
            && x.SelectableValues.Count == 2);
        categoryRepository.Categories.Single().CategoryAttributes.Should().ContainSingle(x => x.AttributeId == attributeRepository.Attributes.Single().Id);
        attributeRepository.Attributes.Should().ContainSingle(x => x.Name == "Brand");
        attributeRepository.Attributes.Single().Values.Should().HaveCount(2);
        repository.AttributeLinks.Single().SelectableValues.Should().OnlyContain(x => x.HiveSpaceAttributeValueId > 0);
    }

    [Fact]
    public async Task Handle_WithExistingFingerprint_ReturnsExistingJob()
    {
        var repository = new CatalogImportBundleRepositoryFake();
        var handler = new ProvisionImportedCategoryAttributesCommandHandler(repository, new NullCatalogImportJobScheduler());
        var request = CreateRequest("sha256:attrs-2");

        var first = await handler.Handle(new ProvisionImportedCategoryAttributesCommand(request, Guid.NewGuid()), CancellationToken.None);
        var second = await handler.Handle(new ProvisionImportedCategoryAttributesCommand(request, Guid.NewGuid()), CancellationToken.None);

        second.JobId.Should().Be(first.JobId);
    }

    private static CategoryAttributeProvisioningRequestDto CreateRequest(string sourceFingerprint)
        => new(
            "2026-08-16",
            new CategoryProvisioningSourceDto("tiki", "sellercenter_category_attributes", "parent:2", "https://sellercenter.tiki.vn/api/tiki_api?path=catalog%2Fattributes"),
            new CategoryProvisioningCrawlDto(DateTimeOffset.UtcNow.AddMinutes(-5), DateTimeOffset.UtcNow, sourceFingerprint, null),
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
            ]);

    private sealed class CatalogImportBundleRepositoryFake : ICatalogImportBundleRepository
    {
        public List<ExternalCategoryLink> CategoryLinks { get; } = [];
        public List<ExternalCategoryAttributeLink> AttributeLinks { get; } = [];
        public List<CatalogImportJob> Jobs { get; } = [];

        public void Add(CatalogImportBundle bundle)
        {
        }

        public void AddJob(CatalogImportJob job)
            => Jobs.Add(job);

        public void AddExternalCategoryLink(ExternalCategoryLink categoryLink)
            => CategoryLinks.Add(categoryLink);

        public void AddExternalCategoryAttributeLink(ExternalCategoryAttributeLink categoryAttributeLink)
            => AttributeLinks.Add(categoryAttributeLink);

        public Task<CatalogImportBundle?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult<CatalogImportBundle?>(null);

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

        public Task<IReadOnlyList<CatalogImportJob>> GetPendingJobsAsync(int limit, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<CatalogImportJob>>(Jobs.Where(x => x.Status == CatalogImportJobStatus.Pending).Take(limit).ToList());

        public Task<IReadOnlyList<CatalogImportJob>> GetRunningJobsInactiveSinceAsync(DateTimeOffset inactiveSince, int limit, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<CatalogImportJob>>(Jobs.Where(x => x.Status == CatalogImportJobStatus.Running).Take(limit).ToList());

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

        public Task<ExternalCategoryLink?> GetExternalCategoryLinkAsync(string sourceSystem, string externalCategoryId, CancellationToken cancellationToken = default)
            => Task.FromResult(CategoryLinks.FirstOrDefault(x => x.SourceSystem == sourceSystem && x.ExternalCategoryId == externalCategoryId));

        public Task<IReadOnlyList<ExternalCategoryLink>> GetExternalCategoryLinksAsync(string sourceSystem, IEnumerable<string> externalCategoryIds, CancellationToken cancellationToken = default)
        {
            var ids = externalCategoryIds.ToHashSet(StringComparer.OrdinalIgnoreCase);
            return Task.FromResult<IReadOnlyList<ExternalCategoryLink>>(CategoryLinks.Where(x => x.SourceSystem == sourceSystem && ids.Contains(x.ExternalCategoryId)).ToList());
        }

        public Task<IReadOnlyList<ExternalCategoryAttributeLink>> GetExternalCategoryAttributeLinksAsync(string sourceSystem, IEnumerable<string> externalCategoryIds, CancellationToken cancellationToken = default)
        {
            var ids = externalCategoryIds.ToHashSet(StringComparer.OrdinalIgnoreCase);
            return Task.FromResult<IReadOnlyList<ExternalCategoryAttributeLink>>(AttributeLinks.Where(x => x.SourceSystem == sourceSystem && ids.Contains(x.ExternalCategoryId)).ToList());
        }

        public Task<int?> GetImportedProductIdBySourceIdentityAsync(string sourceSystem, string externalProductId, CancellationToken cancellationToken = default)
            => Task.FromResult<int?>(null);

        public Task<(IReadOnlyList<CatalogImportBundle> Bundles, int TotalCount)> ListBundlesAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
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
            => Categories.Remove(category);

        public Task<int> SaveChangesAsync()
            => Task.FromResult(1);
    }

    private sealed class AttributeRepositoryFake : IAttributeRepository
    {
        public List<AttributeDefinition> Attributes { get; } = [];
        private int _nextAttributeId = 1;
        private int _nextAttributeValueId = 1;

        public Task<AttributeDefinition?> GetByIdAsync(Guid id)
            => Task.FromResult<AttributeDefinition?>(null);

        public Task<List<AttributeDefinition>> GetAllAsync()
            => Task.FromResult(Attributes);

        public Task<List<AttributeDefinition>> GetByNamesAsync(IReadOnlyCollection<string> names)
        {
            var requested = names.ToHashSet(StringComparer.OrdinalIgnoreCase);
            return Task.FromResult(Attributes.Where(x => requested.Contains(x.Name)).ToList());
        }

        public Task AddAsync(AttributeDefinition attribute)
        {
            AssignAttributeId(attribute);
            AssignValueIds(attribute);
            Attributes.Add(attribute);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(AttributeDefinition attribute)
        {
            AssignAttributeId(attribute);
            AssignValueIds(attribute);
            return Task.CompletedTask;
        }

        public void Remove(AttributeDefinition attribute)
            => Attributes.Remove(attribute);

        public Task<int> SaveChangesAsync()
            => Task.FromResult(1);

        private void AssignAttributeId(AttributeDefinition attribute)
        {
            if (attribute.Id > 0)
                return;

            SetId(attribute, _nextAttributeId++);
        }

        private void AssignValueIds(AttributeDefinition attribute)
        {
            foreach (var value in attribute.Values.Where(x => x.Id <= 0))
                SetId(value, _nextAttributeValueId++);
        }

        private static void SetId(object target, int id)
        {
            var property = target.GetType().GetProperty("Id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            property?.SetValue(target, id);
        }
    }
}
