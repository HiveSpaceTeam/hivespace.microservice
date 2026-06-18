using FluentAssertions;
using HiveSpace.CatalogService.Application.Products.Commands.DeleteProduct;
using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;
using HiveSpace.CatalogService.Domain.Enums;
using HiveSpace.CatalogService.Infrastructure.Repositories;
using HiveSpace.CatalogService.Tests.Fakes;
using HiveSpace.CatalogService.Tests.Fixtures;
using HiveSpace.Domain.Shared.Enumerations;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HiveSpace.CatalogService.Tests.Application.SellerProducts;

public class DeleteProductCommandHandlerTests : IClassFixture<CatalogServiceFixture>
{
    private readonly CatalogServiceFixture _fixture;

    public DeleteProductCommandHandlerTests(CatalogServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Handle_WithExistingProduct_RemovesProductAndReturnsTrue()
    {
        var product = NewProduct(Guid.NewGuid(), "delete-me-1", 30001);
        _fixture.DbContext.Products.Add(product);
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new DeleteProductCommandHandler(
            new SqlProductRepository(_fixture.DbContext),
            new FakeProductEventPublisher());

        var result = await handler.Handle(new DeleteProductCommand(30001), CancellationToken.None);

        result.Should().BeTrue();
        var found = await _fixture.DbContext.Products.FirstOrDefaultAsync(p => p.Id == 30001);
        found.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithNonExistentProductId_ReturnsFalse()
    {
        var handler = new DeleteProductCommandHandler(
            new SqlProductRepository(_fixture.DbContext),
            new FakeProductEventPublisher());

        var result = await handler.Handle(new DeleteProductCommand(99999), CancellationToken.None);

        result.Should().BeFalse();
    }

    private static Product NewProduct(Guid sellerId, string slug, int id) =>
        Product.CreateProduct("Test Product", slug, "Description", "Short",
            ProductStatus.Available, sellerId, ProductCondition.New, false,
            [], [], [], [], [], DateTimeOffset.UtcNow, sellerId.ToString(), id);
}
