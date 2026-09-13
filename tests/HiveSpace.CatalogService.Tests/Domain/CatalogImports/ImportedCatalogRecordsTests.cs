using FluentAssertions;
using HiveSpace.CatalogService.Domain.CatalogImports;
using HiveSpace.CatalogService.Domain.CatalogImports.Enums;
using HiveSpace.Domain.Shared.Exceptions;
using System.Text.Json;
using Xunit;

namespace HiveSpace.CatalogService.Tests.Domain.CatalogImports;

public class ImportedCatalogRecordsTests
{
    private const string SellerLogoUrl = "https://cdn.example.com/sellers/tiki-trading.png";

    [Fact]
    public void ImportedSeller_WithoutDisplayName_ThrowsDomainException()
    {
        var act = () => ImportedSeller.Create(Guid.NewGuid(), "seller-1", "", null, null, null, SellerLogoUrl);

        act.Should().Throw<InvalidFieldException>();
    }

    [Fact]
    public void ImportedSeller_WithoutLogoUrl_ThrowsDomainException()
    {
        var act = () => ImportedSeller.Create(Guid.NewGuid(), "seller-1", "Tiki Trading", null, null, null, "");

        act.Should().Throw<InvalidFieldException>();
    }

    [Fact]
    public void ExternalCategoryLink_MissingProvisionedCategory_BlocksProducts()
    {
        var bundle = CreateBundle();
        bundle.AddSeller("seller-1", "Tiki Trading", null, null, null, SellerLogoUrl)
            .MarkProvisioned(Guid.NewGuid(), Guid.NewGuid(), created: false);
        bundle.AddCategory("1846", null, "Nha sach Tiki", null);
        var product = bundle.AddProduct("product-1", "seller-1", "Book", ["1846"], null, null, null);
        product.AddSku("sku-1", "TIKI-SKU-1", "{}", 125000, "125000", "VND", null, 10, true);

        var issues = CatalogImportValidator.Validate(bundle, vndEnabled: true);

        issues.Should().Contain(i =>
            i.EntityType == "Category"
            && i.EntitySourceId == "1846"
            && i.Severity == ImportValidationSeverity.Blocking
            && i.ReasonCode == "UnprovisionedCategory");

        var productIssue = issues.Should().ContainSingle(i =>
            i.EntityType == "Product"
            && i.EntitySourceId == "product-1"
            && i.Field == "category"
            && i.ReasonCode == "UnprovisionedCategory").Subject;
        using var metadata = JsonDocument.Parse(productIssue.MetadataJson!);
        metadata.RootElement
            .GetProperty("missingExternalCategoryIds")
            .EnumerateArray()
            .Select(x => x.GetString())
            .Should()
            .ContainSingle()
            .Which
            .Should()
            .Be("1846");
    }

    [Fact]
    public void ImportedSku_WithNegativeStock_ThrowsDomainException()
    {
        var product = ImportedProduct.Create(Guid.NewGuid(), "product-1", "seller-1", "Book", ["1846"], null, null, null);

        var act = () => product.AddSku("sku-1", "TIKI-SKU-1", "{}", 125000, "125000", "VND", null, -1, true);

        act.Should().Throw<InvalidFieldException>();
    }

    [Fact]
    public void ImportedSku_WithMissingVndPrice_RecordsBlockingIssue()
    {
        var bundle = CreateBundle();
        bundle.AddSeller("seller-1", "Tiki Trading", null, null, null, SellerLogoUrl)
            .MarkProvisioned(Guid.NewGuid(), Guid.NewGuid(), created: false);
        bundle.AddCategory("1846", null, "Nha sach Tiki", null).MapToCategory(1, Guid.NewGuid());
        var product = bundle.AddProduct("product-1", "seller-1", "Book", ["1846"], null, null, null);
        product.AddSku("sku-1", "TIKI-SKU-1", "{}", null, null, "VND", null, 10, true);

        var issues = CatalogImportValidator.Validate(bundle, vndEnabled: true);

        issues.Should().Contain(i =>
            i.EntityType == "Sku"
            && i.EntitySourceId == "sku-1"
            && i.Field == "price"
            && i.Severity == ImportValidationSeverity.Blocking);
    }

    [Fact]
    public void ExternalCategoryAttributeLink_Create_TrimsFieldsAndPreservesSelectableValues()
    {
        var provisionedByUserId = Guid.NewGuid();

        var link = ExternalCategoryAttributeLink.Create(
            " tiki ",
            " 1846 ",
            " brand ",
            " Brand ",
            " Dropdown ",
            isRequired: true,
            hiveSpaceCategoryId: 12,
            hiveSpaceAttributeDefinitionId: 34,
            [
                new ExternalCategoryAttributeValueLink("apple", "apple", "Apple", 56)
            ],
            " sha256:attrs ",
            provisionedByUserId);

        link.SourceSystem.Should().Be("tiki");
        link.ExternalCategoryId.Should().Be("1846");
        link.SourceAttributeId.Should().Be("brand");
        link.ExternalAttributeName.Should().Be("Brand");
        link.InputType.Should().Be("Dropdown");
        link.IsRequired.Should().BeTrue();
        link.HiveSpaceCategoryId.Should().Be(12);
        link.HiveSpaceAttributeDefinitionId.Should().Be(34);
        link.SourceFingerprint.Should().Be("sha256:attrs");
        link.ProvisionedByUserId.Should().Be(provisionedByUserId);
        link.SelectableValues.Should().ContainSingle(value =>
            value.SourceValueId == "apple"
            && value.DisplayName == "Apple"
            && value.HiveSpaceAttributeValueId == 56);
    }

    [Fact]
    public void ExternalCategoryAttributeLink_Update_ReplacesProvisioningMetadata()
    {
        var link = ExternalCategoryAttributeLink.Create(
            "tiki",
            "1846",
            "brand",
            "Brand",
            "Dropdown",
            isRequired: true,
            hiveSpaceCategoryId: 12,
            hiveSpaceAttributeDefinitionId: 34,
            [],
            "sha256:attrs",
            Guid.NewGuid());
        var nextProvisionedByUserId = Guid.NewGuid();

        link.Update(
            "Manufacturer",
            "Textbox",
            isRequired: false,
            hiveSpaceCategoryId: 21,
            hiveSpaceAttributeDefinitionId: 43,
            [
                new ExternalCategoryAttributeValueLink("sony", "sony", "Sony", 65)
            ],
            "sha256:attrs-next",
            nextProvisionedByUserId);

        link.ExternalAttributeName.Should().Be("Manufacturer");
        link.InputType.Should().Be("Textbox");
        link.IsRequired.Should().BeFalse();
        link.HiveSpaceCategoryId.Should().Be(21);
        link.HiveSpaceAttributeDefinitionId.Should().Be(43);
        link.SourceFingerprint.Should().Be("sha256:attrs-next");
        link.ProvisionedByUserId.Should().Be(nextProvisionedByUserId);
        link.SelectableValues.Should().ContainSingle(value =>
            value.SourceValueId == "sony"
            && value.HiveSpaceAttributeValueId == 65);
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
