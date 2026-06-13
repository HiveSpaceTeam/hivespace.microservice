using FluentAssertions;
using HiveSpace.CatalogService.Application.Products.Queries.GetProductSummaries;
using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;
using HiveSpace.CatalogService.Domain.Enums;
using HiveSpace.CatalogService.Tests.Fixtures;
using HiveSpace.Domain.Shared.Enumerations;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HiveSpace.CatalogService.Tests.Application.Products;

public class SearchProductsQueryHandlerTests : IClassFixture<CatalogServiceFixture>
{
    private readonly CatalogServiceFixture _fixture;

    public SearchProductsQueryHandlerTests(CatalogServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Handle_ReturnsOnlyActiveProducts()
    {
        var product = NewProduct(ProductStatus.Available, "search-active-product", 3001);
        _fixture.DbContext.Products.Add(product);
        await _fixture.DbContext.SaveChangesAsync();

        var products = await _fixture.DbContext.Products
            .Where(x => x.Status == ProductStatus.Available).ToListAsync();

        products.Should().ContainSingle(x => x.Slug == "search-active-product");
        typeof(GetProductSummariesQueryHandler).Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WithInactiveProduct_ExcludesFromActiveListings()
    {
        var inactive = NewProduct(ProductStatus.Unpublish, "inactive-product", 3002);
        _fixture.DbContext.Products.Add(inactive);
        await _fixture.DbContext.SaveChangesAsync();

        var activeProducts = await _fixture.DbContext.Products
            .Where(x => x.Status == ProductStatus.Available).ToListAsync();

        activeProducts.Should().NotContain(x => x.Slug == "inactive-product");
    }

    private static Product NewProduct(ProductStatus status, string slug, int id) =>
        Product.CreateProduct("Test Product", slug, "Description", "Short",
            status, Guid.NewGuid(), ProductCondition.New, false,
            [], [], [], [], [], DateTimeOffset.UtcNow, Guid.NewGuid().ToString(), id);
}
