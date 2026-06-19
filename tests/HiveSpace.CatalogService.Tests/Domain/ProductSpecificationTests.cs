using FluentAssertions;
using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;
using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate.Specifications;
using HiveSpace.CatalogService.Domain.Enums;
using HiveSpace.Domain.Shared.Enumerations;
using Xunit;

namespace HiveSpace.CatalogService.Tests.Domain;

public class ProductSpecificationTests
{
    private static Product NewProduct(ProductStatus status, Guid? sellerId = null) =>
        Product.CreateProduct("Spec Test", "spec-slug", "Desc", "Short",
            status, sellerId ?? Guid.NewGuid(), ProductCondition.New, false,
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
    public void ProductOwnedBySellerSpecification_MatchingSellerId_ReturnsTrue()
    {
        var sellerId = Guid.NewGuid();
        var product = NewProduct(ProductStatus.Available, sellerId);
        var spec = new ProductOwnedBySellerSpecification(sellerId);
        var predicate = spec.ToExpression().Compile();
        predicate(product).Should().BeTrue();
    }

    [Fact]
    public void ProductOwnedBySellerSpecification_DifferentSellerId_ReturnsFalse()
    {
        var product = NewProduct(ProductStatus.Available, Guid.NewGuid());
        var spec = new ProductOwnedBySellerSpecification(Guid.NewGuid());
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
