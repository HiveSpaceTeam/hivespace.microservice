using FluentAssertions;
using HiveSpace.CatalogService.Application.Contracts;
using HiveSpace.CatalogService.Application.Products.Queries.GetProductSummaries;
using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;
using HiveSpace.CatalogService.Domain.Enums;
using HiveSpace.CatalogService.Infrastructure.Repositories;
using HiveSpace.CatalogService.Tests.Fixtures;
using HiveSpace.Domain.Shared.Enumerations;
using Xunit;

namespace HiveSpace.CatalogService.Tests.Application.StorefrontDiscovery;

public class SearchProductsQueryHandlerTests : IClassFixture<CatalogServiceFixture>
{
    private readonly CatalogServiceFixture _fixture;

    public SearchProductsQueryHandlerTests(CatalogServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Handle_WithMatchingKeyword_ReturnsFilteredProducts()
    {
        _fixture.DbContext.Products.AddRange(
            NewProduct("Apple iPhone 15", "iphone-15", 80001),
            NewProduct("Samsung Galaxy", "samsung-galaxy", 80002));
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new GetProductSummariesQueryHandler(new SqlProductRepository(_fixture.DbContext));

        var result = await handler.Handle(
            new GetProductSummariesQuery(new ProductSearchRequestDto(Keyword: "iPhone", Page: 1, PageSize: 10)),
            CancellationToken.None);

        result.Items.Should().ContainSingle(p => p.Name == "Apple iPhone 15");
    }

    [Fact]
    public async Task Handle_WithNonMatchingKeyword_ReturnsEmptyResults()
    {
        var handler = new GetProductSummariesQueryHandler(new SqlProductRepository(_fixture.DbContext));

        var result = await handler.Handle(
            new GetProductSummariesQuery(new ProductSearchRequestDto(Keyword: "xyzzy-nonexistent", Page: 1, PageSize: 10)),
            CancellationToken.None);

        result.Items.Should().BeEmpty();
    }

    private static Product NewProduct(string name, string slug, int id) =>
        Product.CreateProduct(name, slug, "Description", "Short",
            ProductStatus.Available, Guid.NewGuid(), ProductCondition.New, false,
            [], [], [], [], [], DateTimeOffset.UtcNow, Guid.NewGuid().ToString(), id);
}
