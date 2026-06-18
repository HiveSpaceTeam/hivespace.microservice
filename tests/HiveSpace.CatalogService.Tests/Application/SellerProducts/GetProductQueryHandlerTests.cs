using FluentAssertions;
using HiveSpace.CatalogService.Application.Products.Queries.GetProduct;
using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;
using HiveSpace.CatalogService.Domain.Enums;
using HiveSpace.CatalogService.Infrastructure.Repositories;
using HiveSpace.CatalogService.Tests.Fixtures;
using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.Domain.Shared.Exceptions;
using Xunit;

namespace HiveSpace.CatalogService.Tests.Application.SellerProducts;

public class GetProductQueryHandlerTests : IClassFixture<CatalogServiceFixture>
{
    private readonly CatalogServiceFixture _fixture;

    public GetProductQueryHandlerTests(CatalogServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Handle_WithExistingProductId_ReturnsProduct()
    {
        var product = NewProduct("get-product-detail-1", 50001);
        _fixture.DbContext.Products.Add(product);
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new GetProductQueryHandler(new SqlProductRepository(_fixture.DbContext));

        var result = await handler.Handle(new GetProductQuery(50001), CancellationToken.None);

        result.Should().NotBeNull();
        result.Name.Should().Be("Test Product");
        result.Slug.Should().Be("get-product-detail-1");
    }

    [Fact]
    public async Task Handle_WithNonExistentId_ThrowsNotFoundException()
    {
        var handler = new GetProductQueryHandler(new SqlProductRepository(_fixture.DbContext));

        var act = () => handler.Handle(new GetProductQuery(99999), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    private static Product NewProduct(string slug, int id) =>
        Product.CreateProduct("Test Product", slug, "Description", "Short",
            ProductStatus.Available, Guid.NewGuid(), ProductCondition.New, false,
            [], [], [], [], [], DateTimeOffset.UtcNow, Guid.NewGuid().ToString(), id);
}
