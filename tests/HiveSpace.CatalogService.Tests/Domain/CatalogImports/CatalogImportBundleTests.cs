using FluentAssertions;
using HiveSpace.CatalogService.Domain.CatalogImports;
using HiveSpace.CatalogService.Domain.CatalogImports.Enums;
using Xunit;

namespace HiveSpace.CatalogService.Tests.Domain.CatalogImports;

public class CatalogImportBundleTests
{
    private const string SellerLogoUrl = "https://cdn.example.com/sellers/tiki-trading.png";

    [Fact]
    public void Create_WithRequiredSourceMetadata_InitializesSubmittedBundle()
    {
        var submittedBy = Guid.NewGuid();

        var bundle = CatalogImportBundle.Create(
            "2026-07-24",
            "tiki",
            "category",
            "1846",
            "sha256:test",
            DateTimeOffset.UtcNow,
            submittedBy);

        bundle.Id.Should().NotBeEmpty();
        bundle.Status.Should().Be(CatalogImportBundleStatus.Submitted);
        bundle.SourceSystem.Should().Be("tiki");
        bundle.SourceFingerprint.Should().Be("sha256:test");
        bundle.SubmittedByUserId.Should().Be(submittedBy);
    }

    [Fact]
    public void ApplyImportedRecords_WithBlockingHints_SetsNeedsAttentionSummary()
    {
        var bundle = CreateBundle();

        bundle.AddSeller("seller-1", "Tiki Trading", "tiki-trading", null, null, SellerLogoUrl);
        bundle.AddCategory("1846", null, "Nha sach Tiki", """["Nha sach Tiki"]""");
        var product = bundle.AddProduct("product-1", "seller-1", "Book", ["1846"], null, null, null);
        product.AddSku("sku-1", "TIKI-SKU-1", "{}", null, null, "VND", null, 10, true);
        bundle.AddValidationIssue("Sku", "sku-1", "price", ImportValidationSeverity.Blocking, "MissingVndPrice", "Price is required.");
        bundle.RefreshSummary();

        bundle.Status.Should().Be(CatalogImportBundleStatus.NeedsAttention);
        bundle.TotalProducts.Should().Be(1);
        bundle.BlockedProducts.Should().Be(1);
        bundle.ReadyProducts.Should().Be(0);
        bundle.WarningCount.Should().Be(0);
    }

    [Fact]
    public void RefreshSummary_WithoutBlockingIssues_KeepsBundleReviewable()
    {
        var bundle = CreateBundle();

        bundle.AddSeller("seller-1", "Tiki Trading", null, null, null, SellerLogoUrl);
        bundle.AddCategory("1846", null, "Nha sach Tiki", null);
        var product = bundle.AddProduct("product-1", "seller-1", "Book", ["1846"], null, null, null);
        product.AddSku("sku-1", "TIKI-SKU-1", "{}", 125000, "125000", "VND", null, 10, true);
        bundle.AddValidationIssue("Product", "product-1", "description", ImportValidationSeverity.Warning, "MissingOptionalDescription", "Description was not available.");
        bundle.RefreshSummary();

        bundle.Status.Should().Be(CatalogImportBundleStatus.Submitted);
        bundle.TotalProducts.Should().Be(1);
        bundle.ReadyProducts.Should().Be(1);
        bundle.BlockedProducts.Should().Be(0);
        bundle.WarningCount.Should().Be(1);
    }

    private static CatalogImportBundle CreateBundle()
        => CatalogImportBundle.Create(
            "2026-07-24",
            "tiki",
            "category",
            "1846",
            $"sha256:{Guid.NewGuid():N}",
            DateTimeOffset.UtcNow,
            Guid.NewGuid());
}
