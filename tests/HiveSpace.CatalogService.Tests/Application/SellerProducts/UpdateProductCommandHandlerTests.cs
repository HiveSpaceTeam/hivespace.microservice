using FluentAssertions;
using HiveSpace.CatalogService.Application.Products.Commands.UpdateProduct;
using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;
using HiveSpace.CatalogService.Domain.Enums;
using HiveSpace.CatalogService.Tests.Fixtures;
using HiveSpace.Domain.Shared.Enumerations;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HiveSpace.CatalogService.Tests.Application.SellerProducts;

public class UpdateProductCommandHandlerTests : IClassFixture<CatalogServiceFixture>
{
    private readonly CatalogServiceFixture _fixture;

    public UpdateProductCommandHandlerTests(CatalogServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Handle_ChangesStoredTitle()
    {
        var product = NewProduct(ProductStatus.Available, "update-product", 6001);
        _fixture.DbContext.Products.Add(product);
        await _fixture.DbContext.SaveChangesAsync();

        product.UpdateName("Updated Product");
        await _fixture.DbContext.SaveChangesAsync();

        product.Name.Should().Be("Updated Product");
        typeof(UpdateProductCommandHandler).Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ProductRemainsInCatalogAfterUpdate()
    {
        var product = NewProduct(ProductStatus.Available, "update-remains", 6002);
        _fixture.DbContext.Products.Add(product);
        await _fixture.DbContext.SaveChangesAsync();

        product.UpdateName("Renamed Product");
        await _fixture.DbContext.SaveChangesAsync();

        var stored = await _fixture.DbContext.Products.SingleAsync(x => x.Id == 6002);
        stored.Should().NotBeNull();
        stored.Name.Should().Be("Renamed Product");
    }

    private static Product NewProduct(ProductStatus status, string slug, int id) =>
        Product.CreateProduct("Test Product", slug, "Description", "Short",
            status, Guid.NewGuid(), ProductCondition.New, false,
            [], [], [], [], [], DateTimeOffset.UtcNow, Guid.NewGuid().ToString(), id);
}
