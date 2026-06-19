using FluentAssertions;
using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;
using HiveSpace.CatalogService.Domain.Enums;
using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Domain.Shared.ValueObjects;
using Xunit;

namespace HiveSpace.CatalogService.Tests.Domain;

public class ProductTests
{
    private static Product NewProduct(ProductStatus status = ProductStatus.Available, Guid? sellerId = null, int id = 0) =>
        Product.CreateProduct("Test Product", $"test-slug-{id}", "Description", "Short",
            status, sellerId ?? Guid.NewGuid(), ProductCondition.New, false,
            [], [], [], [], [], DateTimeOffset.UtcNow, "creator", id == 0 ? null : id);

    [Fact]
    public void CreateProduct_WithValidFields_SetsNameStatusAndSellerId()
    {
        var sellerId = Guid.NewGuid();
        var product = Product.CreateProduct("Widget", "widget-slug", "Good product", "Short desc",
            ProductStatus.Available, sellerId, ProductCondition.New, false,
            [], [], [], [], [], DateTimeOffset.UtcNow, "creator");
        product.Name.Should().Be("Widget");
        product.Status.Should().Be(ProductStatus.Available);
        product.SellerId.Should().Be(sellerId);
    }

    [Fact]
    public void UpdateName_WithValidName_ChangesStoredName()
    {
        var product = NewProduct();
        product.UpdateName("Updated Widget");
        product.Name.Should().Be("Updated Widget");
    }

    [Fact]
    public void AddCategory_WithNewCategory_AppearsInCategoryCollection()
    {
        var product = NewProduct();
        product.AddCategory(new ProductCategory(10));
        product.Categories.Should().ContainSingle(c => c.CategoryId == 10);
    }

    [Fact]
    public void RemoveCategory_WithExistingCategory_RemovesFromCollection()
    {
        var product = NewProduct();
        product.AddCategory(new ProductCategory(11));
        product.RemoveCategory(new ProductCategory(11));
        product.Categories.Should().BeEmpty();
    }

    [Fact]
    public void UpdateCategories_ReplacesEntireCategorySet()
    {
        var product = NewProduct();
        product.AddCategory(new ProductCategory(1));
        product.UpdateCategories([new ProductCategory(2), new ProductCategory(3)]);
        product.Categories.Should().HaveCount(2);
        product.Categories.Should().NotContain(c => c.CategoryId == 1);
    }

    [Fact]
    public void CreateProduct_WithId_SetsId()
    {
        var product = NewProduct(id: 5);
        product.Id.Should().Be(5);
    }

    [Fact]
    public void CreateProduct_OptionalPropertiesDefaultToNull()
    {
        var product = NewProduct();
        product.BrandId.Should().BeNull();
        product.Weight.Should().BeNull();
        product.Dimensions.Should().BeNull();
    }

    [Fact]
    public void CreateProduct_WithNullCollections_CreatesEmptyCollections()
    {
        var product = Product.CreateProduct("name", "slug", "desc", null,
            ProductStatus.Available, Guid.NewGuid(), ProductCondition.New, false,
            null!, null!, null!, null!, null!, DateTimeOffset.UtcNow, "creator");
        product.Categories.Should().BeEmpty();
        product.Attributes.Should().BeEmpty();
        product.Images.Should().BeEmpty();
        product.Skus.Should().BeEmpty();
        product.Variants.Should().BeEmpty();
    }

    [Fact]
    public void UpdateDescription_ChangesStoredDescription()
    {
        var product = NewProduct();
        product.UpdateDescription("New description");
        product.Description.Should().Be("New description");
    }

    [Fact]
    public void UpdateAttributes_ReplacesAttributeSet()
    {
        var product = NewProduct();
        product.UpdateAttributes([new ProductAttribute(1, [10])]);
        product.Attributes.Should().HaveCount(1);
    }

    [Fact]
    public void UpdateAttributes_WithNull_ThrowsException()
    {
        var product = NewProduct();
        var act = () => product.UpdateAttributes(null!);
        act.Should().Throw<InvalidFieldException>();
    }

    [Fact]
    public void UpdateVariants_ReplacesVariantSet()
    {
        var product = NewProduct();
        product.UpdateVariants([new ProductVariant("Color")]);
        product.Variants.Should().HaveCount(1);
    }

    [Fact]
    public void UpdateVariants_WithNull_ThrowsException()
    {
        var product = NewProduct();
        var act = () => product.UpdateVariants(null!);
        act.Should().Throw<InvalidFieldException>();
    }

    [Fact]
    public void UpdateSkus_ReplacesSkuSet()
    {
        var sku = new Sku("SKU-001", [], [], 10, true, Money.Zero(Currency.VND));
        var product = NewProduct();
        product.UpdateSkus([sku]);
        product.Skus.Should().HaveCount(1);
    }

    [Fact]
    public void UpdateSkus_WithNull_ThrowsException()
    {
        var product = NewProduct();
        var act = () => product.UpdateSkus(null!);
        act.Should().Throw<InvalidFieldException>();
    }

    [Fact]
    public void UpdateAuditInfo_SetsUpdatedBy()
    {
        var product = NewProduct();
        product.UpdateAuditInfo("admin");
        product.UpdatedBy.Should().Be("admin");
        product.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdateThumbnail_SetsFileIdAndClearsUrl()
    {
        var product = NewProduct();
        product.SetThumbnailUrl("http://old.url");
        product.UpdateThumbnail("file-123");
        product.ThumbnailFileId.Should().Be("file-123");
        product.ThumbnailUrl.Should().BeNull();
    }

    [Fact]
    public void SetThumbnailUrl_SetsUrl()
    {
        var product = NewProduct();
        product.SetThumbnailUrl("http://thumb.url");
        product.ThumbnailUrl.Should().Be("http://thumb.url");
    }

    [Fact]
    public void UpdateProductImageUrl_WithMatchingFileId_SetsUrl()
    {
        var image = new ProductImage(1, "file-abc");
        var product = Product.CreateProduct("Test Product", "slug", "desc", null,
            ProductStatus.Available, Guid.NewGuid(), ProductCondition.New, false,
            [], [], [image], [], [], DateTimeOffset.UtcNow, "creator");
        product.UpdateProductImageUrl("file-abc", "http://image.url");
        product.Images.Should().ContainSingle(i => i.ImageUrl == "http://image.url");
    }

    [Fact]
    public void UpdateProductImageUrl_WithNoMatchingFileId_DoesNothing()
    {
        var product = NewProduct();
        var act = () => product.UpdateProductImageUrl("nonexistent", "http://url");
        act.Should().NotThrow();
    }

    [Fact]
    public void AddCategory_WithNull_ThrowsException()
    {
        var product = NewProduct();
        var act = () => product.AddCategory(null!);
        act.Should().Throw<InvalidFieldException>();
    }

    [Fact]
    public void RemoveCategory_WithNull_ThrowsException()
    {
        var product = NewProduct();
        var act = () => product.RemoveCategory(null!);
        act.Should().Throw<InvalidFieldException>();
    }

    [Fact]
    public void UpdateCategories_WithNull_ThrowsException()
    {
        var product = NewProduct();
        var act = () => product.UpdateCategories(null!);
        act.Should().Throw<InvalidFieldException>();
    }
}
