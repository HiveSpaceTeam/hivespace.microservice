using FluentAssertions;
using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;
using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate.Specifications;
using HiveSpace.CatalogService.Domain.Enums;
using HiveSpace.Domain.Shared.Enumerations;
using Xunit;

namespace HiveSpace.CatalogService.Tests.Domain;

public class ProductSpecificationTests
{
    private static Product NewProduct(ProductStatus status, Guid? storeId = null) =>
        Product.CreateProduct("Spec Test", "spec-slug", "Desc", "Short",
            status, storeId ?? Guid.NewGuid(), ProductCondition.New, false,
            [], [], [], [], [], DateTimeOffset.UtcNow, "creator");

    [Fact]
    public void ProductActiveSpecification_ActiveProduct_ReturnsTrue()
    {
        var product = NewProduct(ProductStatus.Available);
        var spec = new ProductActiveSpecification();
        var predicate = spec.ToExpression().Compile();
        predicate(product).Should().BeTrue();
    }

    [Fact]
    public void ProductActiveSpecification_InactiveProduct_ReturnsFalse()
    {
        var product = NewProduct(ProductStatus.Unpublish);
        var spec = new ProductActiveSpecification();
        var predicate = spec.ToExpression().Compile();
        predicate(product).Should().BeFalse();
    }

    [Fact]
    public void ProductOwnedByStoreSpecification_MatchingStoreId_ReturnsTrue()
    {
        var storeId = Guid.NewGuid();
        var product = NewProduct(ProductStatus.Available, storeId);
        var spec = new ProductOwnedByStoreSpecification(storeId);
        var predicate = spec.ToExpression().Compile();
        predicate(product).Should().BeTrue();
    }

    [Fact]
    public void ProductOwnedByStoreSpecification_DifferentStoreId_ReturnsFalse()
    {
        var product = NewProduct(ProductStatus.Available, Guid.NewGuid());
        var spec = new ProductOwnedByStoreSpecification(Guid.NewGuid());
        var predicate = spec.ToExpression().Compile();
        predicate(product).Should().BeFalse();
    }

    [Fact]
    public void ProductInCategorySpecification_MatchingCategory_ReturnsTrue()
    {
        var product = NewProduct(ProductStatus.Available);
        product.AddCategory(new ProductCategory(5));
        var spec = new ProductInCategorySpecification(5);
        var predicate = spec.ToExpression().Compile();
        predicate(product).Should().BeTrue();
    }

    [Fact]
    public void ProductInCategorySpecification_NonMatchingCategory_ReturnsFalse()
    {
        var product = NewProduct(ProductStatus.Available);
        product.AddCategory(new ProductCategory(5));
        var spec = new ProductInCategorySpecification(999);
        var predicate = spec.ToExpression().Compile();
        predicate(product).Should().BeFalse();
    }
}
