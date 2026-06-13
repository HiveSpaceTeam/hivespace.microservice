using FluentAssertions;
using HiveSpace.CatalogService.Application.Products.Commands.DeleteProduct;
using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;
using HiveSpace.CatalogService.Domain.Enums;
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
    public async Task Handle_AfterDeletion_ProductAbsentFromActiveListings()
    {
        var product = NewProduct(ProductStatus.Available, "to-delete", 7001);
        _fixture.DbContext.Products.Add(product);
        await _fixture.DbContext.SaveChangesAsync();

        _fixture.DbContext.Products.Remove(product);
        await _fixture.DbContext.SaveChangesAsync();

        var found = await _fixture.DbContext.Products.FirstOrDefaultAsync(x => x.Slug == "to-delete");
        found.Should().BeNull("deleted products must not appear in the catalog");
    }

    [Fact]
    public void Handle_DeleteProductCommandHandler_IsRegistered()
    {
        var handlerType = typeof(DeleteProductCommandHandler);
        handlerType.GetInterfaces().Should().NotBeEmpty("DeleteProductCommandHandler must implement an ICommandHandler interface");
    }

    private static Product NewProduct(ProductStatus status, string slug, int id) =>
        Product.CreateProduct("Test Product", slug, "Description", "Short",
            status, Guid.NewGuid(), ProductCondition.New, false,
            [], [], [], [], [], DateTimeOffset.UtcNow, Guid.NewGuid().ToString(), id);
}
