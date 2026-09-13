using FluentAssertions;
using HiveSpace.CatalogService.Domain.CatalogImports;
using HiveSpace.CatalogService.Domain.CatalogImports.Enums;
using Xunit;

namespace HiveSpace.CatalogService.Tests.Domain.CatalogImports;

public class ImportDuplicateGroupTests
{
    private const string SellerLogoUrl = "https://cdn.example.com/sellers/tiki-trading.png";

    [Fact]
    public void Create_WithSameExternalProductId_GroupsDuplicateMembers()
    {
        var memberIds = new[] { Guid.NewGuid(), Guid.NewGuid() };

        var group = ImportDuplicateGroup.Create(Guid.NewGuid(), "tiki:product-1", ["product-1", "product-1-copy"], memberIds);

        group.Id.Should().NotBeEmpty();
        group.DuplicateKey.Should().Be("tiki:product-1");
        group.ResolutionStatus.Should().Be(ImportDuplicateGroupResolutionStatus.Unresolved);
        group.ExternalProductIdsJson.Should().Contain("product-1");
        group.MemberImportedProductIdsJson.Should().Contain(memberIds[0].ToString());
    }

    [Fact]
    public void Resolve_WithRepresentativeProduct_SetsResolvedStatus()
    {
        var representativeId = Guid.NewGuid();
        var group = ImportDuplicateGroup.Create(Guid.NewGuid(), "tiki:product-1", ["product-1", "product-1-copy"], [representativeId, Guid.NewGuid()]);

        group.Resolve(representativeId);

        group.RepresentativeImportedProductId.Should().Be(representativeId);
        group.ResolutionStatus.Should().Be(ImportDuplicateGroupResolutionStatus.Resolved);
    }

    [Fact]
    public void ReadyValidation_WithUnresolvedDuplicate_BlocksMembers()
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
        bundle.AddCategory("1846", null, "Nha sach Tiki", null).MapToCategory(1, Guid.NewGuid());
        var product = bundle.AddProduct("product-1", "seller-1", "Book", ["1846"], null, null, null);
        product.AddSku("sku-1", "TIKI-SKU-1", "{}", 125000, "125000", "VND", null, 10, true);
        bundle.AddDuplicateGroup("tiki:product-1", ["product-1", "product-1-copy"], [product.Id, Guid.NewGuid()]);

        var issues = CatalogImportValidator.Validate(bundle, vndEnabled: true);

        issues.Should().Contain(i =>
            i.EntityType == "Product"
            && i.EntitySourceId == "product-1"
            && i.Severity == ImportValidationSeverity.Blocking
            && i.ReasonCode == "UnresolvedDuplicate");
    }
}
