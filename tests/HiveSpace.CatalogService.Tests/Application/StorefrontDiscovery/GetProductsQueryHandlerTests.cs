using FluentAssertions;
using HiveSpace.CatalogService.Application.Contracts;
using HiveSpace.CatalogService.Application.Products.Queries.GetProducts;
using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;
using HiveSpace.CatalogService.Domain.Enums;
using HiveSpace.CatalogService.Infrastructure.Repositories;
using HiveSpace.CatalogService.Tests.Fixtures;
using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.Testing.Shared.Doubles;
using Xunit;

namespace HiveSpace.CatalogService.Tests.Application.StorefrontDiscovery;

public class GetProductsQueryHandlerTests : IClassFixture<CatalogServiceFixture>
{
    private readonly CatalogServiceFixture _fixture;

    public GetProductsQueryHandlerTests(CatalogServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Handle_WithMatchingSellerProducts_ReturnsPaginatedResults()
    {
        var sellerId = Guid.NewGuid();
        _fixture.DbContext.Products.AddRange(
            NewProduct(sellerId, "gp-seller-a", 60001),
            NewProduct(sellerId, "gp-seller-b", 60002));
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new GetProductsQueryHandler(
            new SqlProductRepository(_fixture.DbContext),
            new FakeUserContext { UserId = sellerId });

        var result = await handler.Handle(
            new GetProductsQuery(new ProductSearchRequestDto(Page: 1, PageSize: 10)),
            CancellationToken.None);

        result.Items.Should().HaveCountGreaterThanOrEqualTo(2);
        result.Items.Should().OnlyContain(p => p.SellerId == sellerId);
    }

    [Fact]
    public async Task Handle_WithDifferentSellerId_ReturnsOnlyOwnProducts()
    {
        var ownSellerId = Guid.NewGuid();
        var otherSellerId = Guid.NewGuid();
        _fixture.DbContext.Products.AddRange(
            NewProduct(ownSellerId, "gp-own-1", 60003),
            NewProduct(otherSellerId, "gp-other-1", 60004));
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new GetProductsQueryHandler(
            new SqlProductRepository(_fixture.DbContext),
            new FakeUserContext { UserId = ownSellerId });

        var result = await handler.Handle(
            new GetProductsQuery(new ProductSearchRequestDto(Page: 1, PageSize: 10)),
            CancellationToken.None);

        result.Items.Should().NotContain(p => p.SellerId == otherSellerId);
    }

    [Fact]
    public async Task Handle_WithNullKeyword_FallsBackToEmptySearch()
    {
        var sellerId = Guid.NewGuid();
        _fixture.DbContext.Products.Add(NewProduct(sellerId, "gp-null-kw", 60005));
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new GetProductsQueryHandler(
            new SqlProductRepository(_fixture.DbContext),
            new FakeUserContext { UserId = sellerId });

        var result = await handler.Handle(
            new GetProductsQuery(new ProductSearchRequestDto(Keyword: null!, Page: 1, PageSize: 10)),
            CancellationToken.None);

        result.Items.Should().HaveCountGreaterThanOrEqualTo(1);
    }

    private static Product NewProduct(Guid sellerId, string slug, int id) =>
        Product.CreateProduct("Test Product", slug, "Description", "Short",
            ProductStatus.Available, sellerId, ProductCondition.New, false,
            [], [], [], [], [], DateTimeOffset.UtcNow, sellerId.ToString(), id);
}
