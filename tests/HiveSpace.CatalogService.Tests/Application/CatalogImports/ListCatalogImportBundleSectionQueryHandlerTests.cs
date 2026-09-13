using FluentAssertions;
using HiveSpace.CatalogService.Application.CatalogImports.Dtos;
using HiveSpace.CatalogService.Application.CatalogImports.Queries.ListCatalogImportBundleSection;
using HiveSpace.CatalogService.Domain.CatalogImports;
using HiveSpace.CatalogService.Domain.CatalogImports.Enums;
using HiveSpace.CatalogService.Domain.Repositories;
using System.Text.Json;
using Xunit;

namespace HiveSpace.CatalogService.Tests.Application.CatalogImports;

public class ListCatalogImportBundleSectionQueryHandlerTests
{
    private const string SellerLogoUrl = "https://cdn.example.com/sellers/tiki-trading.png";

    [Fact]
    public async Task Handle_WithSellerOwnershipCandidatesSection_ReturnsPagedCandidates()
    {
        var targetUserId = Guid.NewGuid();
        var targetStoreId = Guid.NewGuid();
        var bundle = CreateBundleWithOwnershipCandidate(targetUserId, targetStoreId);
        var handler = new ListCatalogImportBundleSectionQueryHandler(
            new ValidateCatalogImportBundleCommandHandlerTests.CatalogImportBundleRepositoryFake(bundle));

        var result = await handler.Handle(
            new ListCatalogImportBundleSectionQuery(bundle.Id, "seller-ownership-candidates", 1, 20, null, "Conflict", bundle.Sellers.Single().Id.ToString(), null, null, null),
            CancellationToken.None);

        var page = result.Should().BeOfType<CatalogImportPagedResponseDto<ExistingStoreCandidateDto>>().Subject;
        page.Data.Should().ContainSingle(candidate =>
            candidate.UserId == targetUserId
            && candidate.StoreId == targetStoreId
            && candidate.StoreName == "Existing Seed Store");
    }

    [Fact]
    public async Task Handle_WithSellerOwnershipCandidatesSectionAndNoSuggestions_ReturnsEmptyPage()
    {
        var bundle = ValidateCatalogImportBundleCommandHandlerTests.CreateBundle(provisionCategory: true, priceAmount: 125000);
        var handler = new ListCatalogImportBundleSectionQueryHandler(
            new ValidateCatalogImportBundleCommandHandlerTests.CatalogImportBundleRepositoryFake(bundle));

        var result = await handler.Handle(
            new ListCatalogImportBundleSectionQuery(bundle.Id, "seller-ownership-candidates", 1, 20, null, null, null, null, null, null),
            CancellationToken.None);

        var page = result.Should().BeOfType<CatalogImportPagedResponseDto<ExistingStoreCandidateDto>>().Subject;
        page.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithProductsSection_SearchesEntireFilteredBundleBeforePagination()
    {
        var bundle = CreateSearchableBundle();
        var handler = CreateHandler(bundle);

        var result = await handler.Handle(
            new ListCatalogImportBundleSectionQuery(bundle.Id, "products", 1, 2, "bundle-match", "Ready", "seller-1", null, null, null),
            CancellationToken.None);

        var page = result.Should().BeOfType<CatalogImportPagedResponseDto<ImportedProductDto>>().Subject;
        page.Data.Should().HaveCount(2);
        page.Data.Should().OnlyContain(x => x.ExternalSellerId == "seller-1");
        page.Pagination.TotalItems.Should().Be(3);
        page.Pagination.TotalPages.Should().Be(2);
    }

    [Fact]
    public async Task Handle_WithSellersSection_SearchTermCombinesWithStatusFilter()
    {
        var bundle = CreateSearchableBundle();
        var handler = CreateHandler(bundle);

        var result = await handler.Handle(
            new ListCatalogImportBundleSectionQuery(bundle.Id, "sellers", 1, 20, "store match", "Matched", null, null, null, null),
            CancellationToken.None);

        var page = result.Should().BeOfType<CatalogImportPagedResponseDto<ImportedSellerDto>>().Subject;
        page.Data.Should().ContainSingle(x =>
            x.DisplayName == "Store Match Seller"
            && x.LogoUrl == SellerLogoUrl);
    }

    [Fact]
    public async Task Handle_WithValidationIssuesSection_SearchTermCombinesWithExistingFilters()
    {
        var bundle = CreateSearchableBundle();
        var handler = CreateHandler(bundle);

        var result = await handler.Handle(
            new ListCatalogImportBundleSectionQuery(bundle.Id, "validation-issues", 1, 20, "field-match", null, null, null, "Product", "MissingField"),
            CancellationToken.None);

        var page = result.Should().BeOfType<CatalogImportPagedResponseDto<ImportValidationIssueDto>>().Subject;
        page.Data.Should().ContainSingle(x => x.Message.Contains("field-match", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Handle_WithValidationIssuesSection_ReturnsMetadata()
    {
        var bundle = ValidateCatalogImportBundleCommandHandlerTests.CreateBundle(provisionCategory: false, priceAmount: 125000);
        var issue = CatalogImportValidator.Validate(bundle, vndEnabled: true)
            .Single(x => x.EntityType == "Product" && x.ReasonCode == "UnprovisionedCategory");
        bundle.ReplaceValidationIssues([issue]);
        var handler = CreateHandler(bundle);

        var result = await handler.Handle(
            new ListCatalogImportBundleSectionQuery(bundle.Id, "validation-issues", 1, 20, null, null, null, null, null, null),
            CancellationToken.None);

        var page = result.Should().BeOfType<CatalogImportPagedResponseDto<ImportValidationIssueDto>>().Subject;
        page.Data.Should().ContainSingle()
            .Which.Metadata.Should().NotBeNull();
        page.Data.Single().Metadata!.MissingExternalCategoryIds.Should().ContainSingle().Which.Should().Be("1846");
    }

    [Fact]
    public async Task Handle_WithDuplicateGroupsSection_SearchTermCombinesWithStatusFilter()
    {
        var bundle = CreateSearchableBundle();
        var handler = CreateHandler(bundle);

        var result = await handler.Handle(
            new ListCatalogImportBundleSectionQuery(bundle.Id, "duplicate-groups", 1, 20, "bundle-match", "Unresolved", null, null, null, null),
            CancellationToken.None);

        var page = result.Should().BeOfType<CatalogImportPagedResponseDto<ImportDuplicateGroupDto>>().Subject;
        page.Data.Should().ContainSingle(x => x.ExternalProductIds.Contains("bundle-match-001"));
    }

    [Fact]
    public async Task Handle_WithCategoryLinksSection_SearchTermCombinesWithStatusFilter()
    {
        var bundle = CreateSearchableBundle();
        var handler = CreateHandler(bundle);

        var result = await handler.Handle(
            new ListCatalogImportBundleSectionQuery(bundle.Id, "category-links", 1, 20, "kitchen", "Mapped", null, null, null, null),
            CancellationToken.None);

        var page = result.Should().BeOfType<CatalogImportPagedResponseDto<ImportedCategoryMappingDto>>().Subject;
        page.Data.Should().ContainSingle(x => x.CategoryName == "Kitchen Bundle Match");
    }

    [Fact]
    public async Task Handle_WithWhitespaceSearchTerm_ReturnsUnfilteredProducts()
    {
        var bundle = CreateSearchableBundle();
        var handler = CreateHandler(bundle);

        var result = await handler.Handle(
            new ListCatalogImportBundleSectionQuery(bundle.Id, "products", 1, 20, "   ", null, null, null, null, null),
            CancellationToken.None);

        var page = result.Should().BeOfType<CatalogImportPagedResponseDto<ImportedProductDto>>().Subject;
        page.Pagination.TotalItems.Should().Be(bundle.Products.Count);
    }

    [Fact]
    public async Task Handle_WithUnknownSearchTerm_ReturnsEmptyProductsPage()
    {
        var bundle = CreateSearchableBundle();
        var handler = CreateHandler(bundle);

        var result = await handler.Handle(
            new ListCatalogImportBundleSectionQuery(bundle.Id, "products", 1, 20, "does-not-exist", null, null, null, null, null),
            CancellationToken.None);

        var page = result.Should().BeOfType<CatalogImportPagedResponseDto<ImportedProductDto>>().Subject;
        page.Data.Should().BeEmpty();
        page.Pagination.TotalItems.Should().Be(0);
        page.Pagination.TotalPages.Should().Be(0);
    }

    private static ListCatalogImportBundleSectionQueryHandler CreateHandler(CatalogImportBundle bundle)
        => new(new CatalogImportBundleRepositoryStub(bundle));

    private static CatalogImportBundle CreateBundleWithOwnershipCandidate(Guid targetUserId, Guid targetStoreId)
    {
        var bundle = CatalogImportBundle.Create(
            "2026-07-24",
            "tiki",
            "category",
            "1846",
            $"sha256:{Guid.NewGuid():N}",
            DateTimeOffset.UtcNow,
            Guid.NewGuid());

        bundle.AddSeller("seller-1", "Existing Seed Store", null, null, null, SellerLogoUrl)
            .MarkConflict("SimilarStoreNameConflict", targetStoreId, targetUserId);

        return bundle;
    }

    private static CatalogImportBundle CreateSearchableBundle()
    {
        var bundle = CatalogImportBundle.Create(
            "2026-07-24",
            "tiki",
            "category",
            "1846",
            $"sha256:{Guid.NewGuid():N}",
            DateTimeOffset.UtcNow,
            Guid.NewGuid());

        bundle.AddSeller("seller-1", "Store Match Seller", null, null, null, SellerLogoUrl)
            .MarkProvisioned(Guid.NewGuid(), Guid.NewGuid(), created: false);
        bundle.AddSeller("seller-2", "Store Conflict Seller", null, null, null, "https://cdn.example.com/sellers/store-conflict.png")
            .MarkConflict("SellerConflict");

        bundle.AddCategoryLink("cat-match-1", null, "Kitchen Bundle Match", 10);
        bundle.AddCategoryLink("cat-other-1", null, "Office Supplies", 11);
        bundle.AddCategoryLink("cat-bundle-2", null, "Bundle Match Secondary", 12);

        bundle.AddProduct("bundle-match-001", "seller-1", "Bundle Match Alpha", ["cat-match-1"], null, "Description", null);
        bundle.AddProduct("bundle-match-002", "seller-1", "Bundle Match Beta", ["cat-match-1"], null, "Description", null);
        bundle.AddProduct("other-001", "seller-1", "Other Product", ["bundle-match-category"], null, "Description", null);
        bundle.AddProduct("bundle-match-archived", "seller-2", "Bundle Match Gamma", ["cat-other-1"], null, "Description", null)
            .MarkImported(42);

        var primaryProduct = bundle.Products.First();
        var archivedProduct = bundle.Products.Last();
        bundle.AddDuplicateGroup("DuplicateBundleMatch", ["bundle-match-001", "bundle-match-002"], [primaryProduct.Id, Guid.NewGuid()]);
        bundle.AddDuplicateGroup("DuplicateResolved", ["other-001", "bundle-match-archived"], [archivedProduct.Id, Guid.NewGuid()])
            .Resolve(archivedProduct.Id);

        bundle.AddValidationIssue("Product", "bundle-match-001", "title", ImportValidationSeverity.Warning, "MissingField", "Bundle field-match warning");
        bundle.AddValidationIssue("Sku", "sku-404", "price", ImportValidationSeverity.Blocking, "MissingField", "Non matching issue");

        return bundle;
    }

    private sealed class CatalogImportBundleRepositoryStub(CatalogImportBundle bundle) : ICatalogImportBundleRepository
    {
        public List<ExternalCategoryLink> CategoryLinks => throw new NotSupportedException();
        public void Add(CatalogImportBundle bundle)
            => throw new NotSupportedException();

        public void AddJob(CatalogImportJob job)
            => throw new NotSupportedException();

        public void AddExternalCategoryLink(ExternalCategoryLink categoryLink)
            => throw new NotSupportedException();

        public void AddExternalCategoryAttributeLink(ExternalCategoryAttributeLink categoryAttributeLink)
            => throw new NotSupportedException();

        public Task<ExternalCategoryLink?> GetExternalCategoryLinkAsync(string sourceSystem, string externalCategoryId, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<IReadOnlyList<ExternalCategoryLink>> GetExternalCategoryLinksAsync(string sourceSystem, IEnumerable<string> externalCategoryIds, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<IReadOnlyList<ExternalCategoryAttributeLink>> GetExternalCategoryAttributeLinksAsync(string sourceSystem, IEnumerable<string> externalCategoryIds, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<int?> GetImportedProductIdBySourceIdentityAsync(string sourceSystem, string externalProductId, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<CatalogImportBundle?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult<CatalogImportBundle?>(id == bundle.Id ? bundle : null);

        public Task<CatalogImportBundle?> GetBySourceFingerprintAsync(string sourceFingerprint, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<CatalogImportJob?> GetJobByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<CatalogImportJob?> GetJobByOperationAndSourceFingerprintAsync(HiveSpace.CatalogService.Domain.CatalogImports.Enums.CatalogImportJobOperationType operationType, string sourceSystem, string sourceFingerprint, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<CatalogImportJob?> GetActiveJobByOperationAndSourceFingerprintAsync(HiveSpace.CatalogService.Domain.CatalogImports.Enums.CatalogImportJobOperationType operationType, string sourceSystem, string sourceFingerprint, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<IReadOnlyList<CatalogImportJob>> GetPendingJobsAsync(int limit, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<IReadOnlyList<CatalogImportJob>> GetRunningJobsInactiveSinceAsync(DateTimeOffset inactiveSince, int limit, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<(IReadOnlyList<CatalogImportJob> Jobs, int TotalCount)> ListJobsAsync(int pageNumber, int pageSize, HiveSpace.CatalogService.Domain.CatalogImports.Enums.CatalogImportJobStatus? status = null, HiveSpace.CatalogService.Domain.CatalogImports.Enums.CatalogImportJobOperationType? operationType = null, string? sourceSystem = null, Guid? bundleId = null, DateTimeOffset? requestedFrom = null, DateTimeOffset? requestedTo = null, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<(IReadOnlyList<CatalogImportBundle> Bundles, int TotalCount)> ListBundlesAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<IReadOnlyList<CatalogImportBundle>> ListRecentAsync(int limit, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
