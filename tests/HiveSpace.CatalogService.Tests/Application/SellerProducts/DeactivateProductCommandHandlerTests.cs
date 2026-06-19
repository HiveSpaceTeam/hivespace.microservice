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

public class DeactivateProductCommandHandlerTests : IClassFixture<CatalogServiceFixture>
{
    private readonly CatalogServiceFixture _fixture;

    public DeactivateProductCommandHandlerTests(CatalogServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Handle_AfterDeletion_ProductAbsentFromCatalog()
    {
        var product = NewProduct(ProductStatus.Available, "to-deactivate-1", 40001);
        _fixture.DbContext.Products.Add(product);
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new DeleteProductCommandHandler(
            new SqlProductRepository(_fixture.DbContext),
            new FakeProductEventPublisher());

        await handler.Handle(new DeleteProductCommand(40001), CancellationToken.None);

        var found = await _fixture.DbContext.Products.FirstOrDefaultAsync(p => p.Id == 40001);
        found.Should().BeNull("deleted products must not remain in the catalog");
    }

    [Fact]
    public async Task Handle_WithAnotherProduct_DoesNotDeleteOtherProducts()
    {
        var keep = NewProduct(ProductStatus.Available, "keep-me", 40002);
        var remove = NewProduct(ProductStatus.Available, "remove-me", 40003);
        _fixture.DbContext.Products.AddRange(keep, remove);
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new DeleteProductCommandHandler(
            new SqlProductRepository(_fixture.DbContext),
            new FakeProductEventPublisher());

        await handler.Handle(new DeleteProductCommand(40003), CancellationToken.None);

        var kept = await _fixture.DbContext.Products.FirstOrDefaultAsync(p => p.Id == 40002);
        kept.Should().NotBeNull("deleting one product must not affect other products");
    }

    private static Product NewProduct(ProductStatus status, string slug, int id) =>
        Product.CreateProduct("Test Product", slug, "Description", "Short",
            status, Guid.NewGuid(), ProductCondition.New, false,
            [], [], [], [], [], DateTimeOffset.UtcNow, Guid.NewGuid().ToString(), id);
}
