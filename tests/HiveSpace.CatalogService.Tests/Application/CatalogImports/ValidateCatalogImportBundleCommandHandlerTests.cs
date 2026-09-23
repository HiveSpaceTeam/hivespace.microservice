using FluentAssertions;
using HiveSpace.CatalogService.Application.CatalogImports.Commands.RetryCatalogImportJob;
using HiveSpace.CatalogService.Application.CatalogImports.Commands.ValidateCatalogImportBundle;
using HiveSpace.CatalogService.Application.CatalogImports.Jobs;
using HiveSpace.CatalogService.Domain.Aggregates.AttributeAggregate;
using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;
using HiveSpace.CatalogService.Domain.Aggregates.External;
using HiveSpace.CatalogService.Domain.CatalogImports;
using HiveSpace.CatalogService.Domain.Enums;
using HiveSpace.CatalogService.Domain.Repositories;
using HiveSpace.CatalogService.Domain.Repositories.External;
using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.Testing.Shared.Doubles;
using Xunit;

namespace HiveSpace.CatalogService.Tests.Application.CatalogImports;

public class ValidateCatalogImportBundleCommandHandlerTests
{
    private const string SellerLogoUrl = "https://cdn.example.com/sellers/tiki-trading.png";

    [Fact]
    public async Task Handle_WithUnmappedCategory_CreatesBlockingIssue()
    {
        var bundle = CreateBundle(provisionCategory: false, priceAmount: 125000);
        var repository = new CatalogImportBundleRepositoryFake(bundle);

        var job = await ExecuteValidationAsync(
            repository,
            bundle.Id,
            new ProductRepositoryFake(),
            new PlatformCurrencyPolicyRefRepositoryFake(enabled: true));

        job.BlockedCount.Should().Be(1);
        bundle.CategoryLinks.Should().ContainSingle(i =>
            i.ExternalCategoryId == "1846"
            && !i.HiveSpaceCategoryId.HasValue
            && i.MappingStatus == HiveSpace.CatalogService.Domain.CatalogImports.Enums.ImportedCategoryMappingStatus.Unmapped);
        var issue = bundle.ValidationIssues.Should().ContainSingle(i =>
            i.EntityType == "Product"
            && i.ReasonCode == "UnprovisionedCategory").Subject;
        issue.MetadataJson.Should().Contain("missingExternalCategoryIds");
        issue.MetadataJson.Should().Contain("1846");
    }

    [Fact]
    public async Task Handle_WithInvalidMoney_CreatesSkuBlockingIssue()
    {
        var bundle = CreateBundle(provisionCategory: true, priceAmount: null);
        var repository = new CatalogImportBundleRepositoryFake(bundle);

        var job = await ExecuteValidationAsync(
            repository,
            bundle.Id,
            new ProductRepositoryFake(),
            new PlatformCurrencyPolicyRefRepositoryFake(enabled: true));

        job.BlockedCount.Should().Be(1);
        bundle.ValidationIssues.Should().Contain(i => i.EntityType == "Sku" && i.ReasonCode == "InvalidVndPrice");
    }

    [Fact]
    public async Task Handle_WithDuplicateGroupUnresolved_BlocksDuplicateMembers()
    {
        var bundle = CreateBundle(provisionCategory: true, priceAmount: 125000);
        var product = bundle.Products.Single();
        bundle.AddDuplicateGroup("tiki:product-1", ["product-1", "product-1-copy"], [product.Id, Guid.NewGuid()]);
        var repository = new CatalogImportBundleRepositoryFake(bundle);

        var job = await ExecuteValidationAsync(
            repository,
            bundle.Id,
            new ProductRepositoryFake(),
            new PlatformCurrencyPolicyRefRepositoryFake(enabled: true));

        job.BlockedCount.Should().Be(1);
        bundle.ValidationIssues.Should().Contain(i => i.ReasonCode == "UnresolvedDuplicate");
    }

    [Fact]
    public async Task Handle_WithSimilarExistingProductTitle_CreatesDuplicateRiskBlockingIssue()
    {
        var bundle = CreateBundle(provisionCategory: true, priceAmount: 125000);
        bundle.Products.Single().AddImage("https://cdn.tiki.vn/book.jpg", "Thumbnail", null);
        var secondProduct = bundle.AddProduct("product-2", "seller-1", "Notebook Stand", ["1846"], null, "Description", null);
        secondProduct.AddSku("sku-2", "TIKI-SKU-2", "{}", 125000, "125000", "VND", null, 10, true);
        secondProduct.AddImage("https://cdn.tiki.vn/another-book.jpg", "Thumbnail", null);
        var repository = new CatalogImportBundleRepositoryFake(bundle);
        var productRepository = new ProductRepositoryFake();
        productRepository.Products.Add(Product.CreateProduct(
            "Book",
            "book",
            "Existing seed product",
            null,
            ProductStatus.Available,
            Guid.NewGuid(),
            ProductCondition.New,
            false,
            [],
            [],
            [],
            [],
            [],
            DateTimeOffset.UtcNow,
            "seed"));

        var job = await ExecuteValidationAsync(
            repository,
            bundle.Id,
            productRepository,
            new PlatformCurrencyPolicyRefRepositoryFake(enabled: true));

        job.BlockedCount.Should().Be(1);
        bundle.ValidationIssues.Should().Contain(i =>
            i.ReasonCode == "ExistingProductDuplicateRisk"
            && i.Field == "title");
        productRepository.GetAllCallCount.Should().Be(1);
        productRepository.FindSimilarByTitleCallCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WithMissingUsableProductMedia_CreatesBlockingIssue()
    {
        var bundle = CreateBundle(provisionCategory: true, priceAmount: 125000);
        var repository = new CatalogImportBundleRepositoryFake(bundle);

        var job = await ExecuteValidationAsync(
            repository,
            bundle.Id,
            new ProductRepositoryFake(),
            new PlatformCurrencyPolicyRefRepositoryFake(enabled: true));

        job.BlockedCount.Should().Be(1);
        bundle.ValidationIssues.Should().Contain(i =>
            i.ReasonCode == "MissingUsableProductMedia"
            && i.Field == "images");
    }

    [Fact]
    public async Task Handle_WithProvisionedSelectableAttributeValue_MatchesWithoutBlockingIssue()
    {
        var bundle = CreateBundle(provisionCategory: true, priceAmount: 125000);
        var product = bundle.Products.Single();
        product.AddAttribute("Brand", "Apple", "brand", "apple");
        product.AddImage("https://cdn.tiki.vn/book.jpg", "Thumbnail", null);
        var repository = new CatalogImportBundleRepositoryFake(bundle);
        repository.AttributeLinks.Add(ExternalCategoryAttributeLink.Create(
            "tiki",
            "1846",
            "brand",
            "Brand",
            "Dropdown",
            true,
            1,
            100,
            [new ExternalCategoryAttributeValueLink("apple", "apple", "Apple", 900)],
            "sha256:attrs",
            Guid.NewGuid()));

        var job = await ExecuteValidationAsync(
            repository,
            bundle.Id,
            new ProductRepositoryFake(),
            new PlatformCurrencyPolicyRefRepositoryFake(enabled: true));

        job.BlockedCount.Should().Be(0);
        product.Attributes.Single().HiveSpaceAttributeDefinitionId.Should().Be(100);
        product.Attributes.Single().MatchedAttributeValueIds.Should().ContainSingle().Which.Should().Be(900);
    }
    [Fact]
    public async Task Handle_WithPendingValidationJob_ReturnsExistingJob()
    {
        var bundle = CreateBundle(provisionCategory: true, priceAmount: 125000);
        var repository = new CatalogImportBundleRepositoryFake(bundle);

        var first = await new ValidateCatalogImportBundleCommandHandler(
            repository,
            new FakeUserContext { UserId = Guid.NewGuid() },
            new NullCatalogImportJobScheduler())
            .Handle(new ValidateCatalogImportBundleCommand(bundle.Id), CancellationToken.None);

        var second = await new ValidateCatalogImportBundleCommandHandler(
            repository,
            new FakeUserContext { UserId = Guid.NewGuid() },
            new NullCatalogImportJobScheduler())
            .Handle(new ValidateCatalogImportBundleCommand(bundle.Id), CancellationToken.None);

        second.JobId.Should().Be(first.JobId);
        repository.JobCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_AfterCompletedValidationJob_CreatesNewJob()
    {
        var bundle = CreateBundle(provisionCategory: true, priceAmount: 125000);
        var repository = new CatalogImportBundleRepositoryFake(bundle);

        var completed = await ExecuteValidationAsync(
            repository,
            bundle.Id,
            new ProductRepositoryFake(),
            new PlatformCurrencyPolicyRefRepositoryFake(enabled: true));

        var next = await new ValidateCatalogImportBundleCommandHandler(
            repository,
            new FakeUserContext { UserId = Guid.NewGuid() },
            new NullCatalogImportJobScheduler())
            .Handle(new ValidateCatalogImportBundleCommand(bundle.Id), CancellationToken.None);

        next.JobId.Should().NotBe(completed.Id);
        repository.JobCount.Should().Be(2);
    }

    [Fact]
    public async Task RetryHandle_WhenSiblingValidationJobIsActive_ReturnsActiveJob()
    {
        var bundle = CreateBundle(provisionCategory: true, priceAmount: 125000);
        var repository = new CatalogImportBundleRepositoryFake(bundle);
        var failed = CatalogImportJob.Create(
            HiveSpace.CatalogService.Domain.CatalogImports.Enums.CatalogImportJobOperationType.ValidateBundle,
            bundle.SourceSystem,
            Guid.NewGuid(),
            sourceFingerprint: bundle.SourceFingerprint,
            bundleId: bundle.Id);
        failed.Start();
        failed.Fail("previous validation failed");
        var active = CatalogImportJob.Create(
            HiveSpace.CatalogService.Domain.CatalogImports.Enums.CatalogImportJobOperationType.ValidateBundle,
            bundle.SourceSystem,
            Guid.NewGuid(),
            sourceFingerprint: bundle.SourceFingerprint,
            bundleId: bundle.Id);
        active.Start();
        repository.AddJob(failed);
        repository.AddJob(active);

        var result = await new RetryCatalogImportJobCommandHandler(
            repository,
            new NullCatalogImportJobScheduler())
            .Handle(new RetryCatalogImportJobCommand(failed.Id), CancellationToken.None);

        result.JobId.Should().Be(active.Id);
        failed.Status.Should().Be(HiveSpace.CatalogService.Domain.CatalogImports.Enums.CatalogImportJobStatus.Failed);
    }

    internal static async Task<CatalogImportJob> ExecuteValidationAsync(
        CatalogImportBundleRepositoryFake repository,
        Guid bundleId,
        IProductRepository productRepository,
        IPlatformCurrencyPolicyRefRepository currencyPolicyRepository)
    {
        var submission = await new ValidateCatalogImportBundleCommandHandler(
            repository,
            new FakeUserContext { UserId = Guid.NewGuid() },
            new NullCatalogImportJobScheduler())
            .Handle(new ValidateCatalogImportBundleCommand(bundleId), CancellationToken.None);

        await new CatalogImportJobProcessor(
            repository,
            new CategoryRepositoryFake(),
            new AttributeRepositoryFake(),
            productRepository,
            currencyPolicyRepository)
            .ProcessJobAsync(submission.JobId, CancellationToken.None);

        return (await repository.GetJobByIdAsync(submission.JobId, CancellationToken.None))!;
    }

    internal static CatalogImportBundle CreateBundle(bool provisionCategory, long? priceAmount)
    {
        var bundle = CatalogImportBundle.Create(
            "2026-07-24",
            "tiki",
            "category",
            "1846",
            $"sha256:{Guid.NewGuid():N}",
            DateTimeOffset.UtcNow,
            Guid.NewGuid());

        bundle.AddSeller("seller-1", "Tiki Trading", null, null, null, SellerLogoUrl)
            .MarkProvisioned(Guid.NewGuid(), Guid.NewGuid(), created: false);
        if (provisionCategory)
            bundle.AddCategoryLink("1846", null, "Nha sach Tiki", 1);
        var product = bundle.AddProduct("product-1", "seller-1", "Book", ["1846"], null, "Description", null);
        product.AddSku("sku-1", "TIKI-SKU-1", "{}", priceAmount, priceAmount?.ToString(), "VND", null, 10, true);

        return bundle;
    }

    internal sealed class CatalogImportBundleRepositoryFake(CatalogImportBundle bundle) : ICatalogImportBundleRepository
    {
        private readonly List<CatalogImportJob> _jobs = [];
        private readonly List<ExternalCategoryLink> _categoryLinks = bundle.CategoryLinks
            .Select(x => ExternalCategoryLink.Create(
                bundle.SourceSystem,
                x.ExternalCategoryId,
                x.ExternalCategoryName,
                x.ExternalParentCategoryId,
                x.PathJson,
                x.HiveSpaceCategoryId.GetValueOrDefault(),
                "sha256:categories",
                Guid.NewGuid()))
            .ToList();
        public List<ExternalCategoryAttributeLink> AttributeLinks { get; } = [];

        public void Add(CatalogImportBundle bundle)
        {
        }

        public void AddJob(CatalogImportJob job)
        {
            _jobs.Add(job);
        }

        public int JobCount => _jobs.Count;

        public void AddExternalCategoryLink(ExternalCategoryLink categoryLink)
            => _categoryLinks.Add(categoryLink);

        public void AddExternalCategoryAttributeLink(ExternalCategoryAttributeLink categoryAttributeLink)
        {
        }

        public Task<ExternalCategoryLink?> GetExternalCategoryLinkAsync(
            string sourceSystem,
            string externalCategoryId,
            CancellationToken cancellationToken = default)
            => Task.FromResult(_categoryLinks.FirstOrDefault(x =>
                string.Equals(x.SourceSystem, sourceSystem, StringComparison.OrdinalIgnoreCase)
                && string.Equals(x.ExternalCategoryId, externalCategoryId, StringComparison.OrdinalIgnoreCase)));

        public Task<IReadOnlyList<ExternalCategoryLink>> GetExternalCategoryLinksAsync(
            string sourceSystem,
            IEnumerable<string> externalCategoryIds,
            CancellationToken cancellationToken = default)
        {
            var ids = externalCategoryIds.ToHashSet(StringComparer.OrdinalIgnoreCase);
            return Task.FromResult<IReadOnlyList<ExternalCategoryLink>>(
                _categoryLinks.Where(x => string.Equals(x.SourceSystem, sourceSystem, StringComparison.OrdinalIgnoreCase)
                    && ids.Contains(x.ExternalCategoryId)).ToList());
        }

        public Task<IReadOnlyList<ExternalCategoryAttributeLink>> GetExternalCategoryAttributeLinksAsync(
            string sourceSystem,
            IEnumerable<string> externalCategoryIds,
            CancellationToken cancellationToken = default)
        {
            var ids = externalCategoryIds.ToHashSet(StringComparer.OrdinalIgnoreCase);
            return Task.FromResult<IReadOnlyList<ExternalCategoryAttributeLink>>(
                AttributeLinks.Where(x =>
                    string.Equals(x.SourceSystem, sourceSystem, StringComparison.OrdinalIgnoreCase)
                    && ids.Contains(x.ExternalCategoryId)).ToList());
        }

        public Task<int?> GetImportedProductIdBySourceIdentityAsync(
            string sourceSystem,
            string externalProductId,
            CancellationToken cancellationToken = default)
            => Task.FromResult<int?>(null);

        public Task<CatalogImportBundle?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult<CatalogImportBundle?>(id == bundle.Id ? bundle : null);

        public Task<CatalogImportBundle?> GetBySourceFingerprintAsync(string sourceFingerprint, CancellationToken cancellationToken = default)
            => Task.FromResult<CatalogImportBundle?>(bundle.SourceFingerprint == sourceFingerprint ? bundle : null);

        public Task<CatalogImportJob?> GetJobByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_jobs.FirstOrDefault(x => x.Id == id));

        public Task<CatalogImportJob?> GetJobByOperationAndSourceFingerprintAsync(
            HiveSpace.CatalogService.Domain.CatalogImports.Enums.CatalogImportJobOperationType operationType,
            string sourceSystem,
            string sourceFingerprint,
            CancellationToken cancellationToken = default)
            => Task.FromResult(_jobs.FirstOrDefault(x =>
                x.OperationType == operationType
                && x.SourceSystem == sourceSystem
                && x.SourceFingerprint == sourceFingerprint));

        public Task<CatalogImportJob?> GetActiveJobByOperationAndSourceFingerprintAsync(
            HiveSpace.CatalogService.Domain.CatalogImports.Enums.CatalogImportJobOperationType operationType,
            string sourceSystem,
            string sourceFingerprint,
            CancellationToken cancellationToken = default)
            => Task.FromResult(_jobs.FirstOrDefault(x =>
                x.OperationType == operationType
                && x.SourceSystem == sourceSystem
                && x.SourceFingerprint == sourceFingerprint
                && x.Status is HiveSpace.CatalogService.Domain.CatalogImports.Enums.CatalogImportJobStatus.Pending
                    or HiveSpace.CatalogService.Domain.CatalogImports.Enums.CatalogImportJobStatus.Running));

        public Task<IReadOnlyList<CatalogImportJob>> GetPendingJobsAsync(int limit, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<CatalogImportJob>>(_jobs
                .Where(x => x.Status == HiveSpace.CatalogService.Domain.CatalogImports.Enums.CatalogImportJobStatus.Pending)
                .Take(limit)
                .ToList());

        public Task<IReadOnlyList<CatalogImportJob>> GetRunningJobsInactiveSinceAsync(
            DateTimeOffset inactiveSince,
            int limit,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<CatalogImportJob>>(_jobs
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
            => Task.FromResult(((IReadOnlyList<CatalogImportJob>)_jobs, _jobs.Count));

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

    internal sealed class CategoryRepositoryFake : HiveSpace.CatalogService.Domain.Repositories.ICategoryRepository
    {
        public Task<HiveSpace.CatalogService.Domain.Aggregates.CategoryAggregate.Category?> GetByIdAsync(int id)
            => Task.FromResult<HiveSpace.CatalogService.Domain.Aggregates.CategoryAggregate.Category?>(null);

        public Task<List<HiveSpace.CatalogService.Domain.Aggregates.CategoryAggregate.Category>> GetAllAsync()
            => Task.FromResult(new List<HiveSpace.CatalogService.Domain.Aggregates.CategoryAggregate.Category>());

        public Task AddAsync(HiveSpace.CatalogService.Domain.Aggregates.CategoryAggregate.Category category)
            => Task.CompletedTask;

        public Task UpdateAsync(HiveSpace.CatalogService.Domain.Aggregates.CategoryAggregate.Category category)
            => Task.CompletedTask;

        public void Remove(HiveSpace.CatalogService.Domain.Aggregates.CategoryAggregate.Category category)
        {
        }

        public Task<int> SaveChangesAsync()
            => Task.FromResult(1);
    }

    internal sealed class PlatformCurrencyPolicyRefRepositoryFake(bool enabled) : IPlatformCurrencyPolicyRefRepository
    {
        public Task<PlatformCurrencyPolicyRef?> GetCurrentAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<PlatformCurrencyPolicyRef?>(new PlatformCurrencyPolicyRef(Guid.NewGuid(), "VND", 1, DateTimeOffset.UtcNow, enabled ? ["VND"] : ["USD"]));

        public void Add(PlatformCurrencyPolicyRef policyRef)
        {
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(1);
    }

    internal sealed class AttributeRepositoryFake : IAttributeRepository
    {
        public Task<AttributeDefinition?> GetByIdAsync(Guid id)
            => Task.FromResult<AttributeDefinition?>(null);

        public Task<List<AttributeDefinition>> GetAllAsync()
            => Task.FromResult(new List<AttributeDefinition>());

        public Task<List<AttributeDefinition>> GetByNamesAsync(IReadOnlyCollection<string> names)
            => Task.FromResult(new List<AttributeDefinition>());


        public Task AddAsync(AttributeDefinition attribute)
            => Task.CompletedTask;

        public Task UpdateAsync(AttributeDefinition attribute)
            => Task.CompletedTask;

        public void Remove(AttributeDefinition attribute)
        {
        }

        public Task<int> SaveChangesAsync()
            => Task.FromResult(1);
    }

    internal sealed class ProductRepositoryFake : IProductRepository
    {
        public List<Product> Products { get; } = [];
        public int GetAllCallCount { get; private set; }
        public int FindSimilarByTitleCallCount { get; private set; }

        public Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => Task.FromResult<Product?>(Products.FirstOrDefault(x => x.Id == id));

        public Task<List<Product>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            GetAllCallCount++;
            return Task.FromResult(Products);
        }

        public Task<Product?> GetDetailByIdAsync(int id, bool noTracking, CancellationToken cancellationToken = default)
            => Task.FromResult<Product?>(Products.FirstOrDefault(x => x.Id == id));

        public Task AddAsync(Product product, CancellationToken cancellationToken = default)
        {
            Products.Add(product);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public void Remove(Product product)
            => Products.Remove(product);

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(1);

        public Task<(IReadOnlyList<Product> Items, int Total)> GetPagedAsync(string keyword, int pageIndex, int pageSize, string sort, Guid sellerId, CancellationToken cancellationToken = default)
            => Task.FromResult<(IReadOnlyList<Product> Items, int Total)>((Products, Products.Count));

        public Task<(IReadOnlyList<Product> Items, int Total)> GetSummariesPagedAsync(string keyword, int pageIndex, int pageSize, string sort, CancellationToken cancellationToken = default)
            => Task.FromResult<(IReadOnlyList<Product> Items, int Total)>((Products, Products.Count));

        public Task<IReadOnlyList<Product>> FindSimilarByTitleAsync(string title, CancellationToken cancellationToken = default)
        {
            FindSimilarByTitleCallCount++;
            return Task.FromResult<IReadOnlyList<Product>>(Products.Where(x => string.Equals(x.Name, title, StringComparison.OrdinalIgnoreCase)).ToList());
        }
    }
}
