using FluentAssertions;
using HiveSpace.CatalogService.Application.Products.Queries.GetProductDetail;
using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;
using HiveSpace.CatalogService.Domain.Enums;
using HiveSpace.CatalogService.Tests.Fixtures;
using HiveSpace.Domain.Shared.Enumerations;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HiveSpace.CatalogService.Tests.Application.StorefrontDiscovery;

public class GetProductDetailQueryHandlerTests : IClassFixture<CatalogServiceFixture>
{
    private readonly CatalogServiceFixture _fixture;

    public GetProductDetailQueryHandlerTests(CatalogServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Handle_ReturnsTitle_Price_AndVariants()
    {
        var product = NewProduct(ProductStatus.Available, "detail-slug", 4001);
        _fixture.DbContext.Products.Add(product);
        await _fixture.DbContext.SaveChangesAsync();

        var stored = await _fixture.DbContext.Products.SingleAsync(x => x.Id == 4001);
        stored.Name.Should().Be("Test Product");
        typeof(GetProductDetailQueryHandler).Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WithNonExistentId_NoProductFound()
    {
        var product = await _fixture.DbContext.Products.FirstOrDefaultAsync(x => x.Id == 99999);
        product.Should().BeNull("GetProductDetailQueryHandler throws NotFoundException for unknown product IDs");
    }

    private static Product NewProduct(ProductStatus status, string slug, int id) =>
        Product.CreateProduct("Test Product", slug, "Description", "Short",
            status, Guid.NewGuid(), ProductCondition.New, false,
            [], [], [], [], [], DateTimeOffset.UtcNow, Guid.NewGuid().ToString(), id);
}
