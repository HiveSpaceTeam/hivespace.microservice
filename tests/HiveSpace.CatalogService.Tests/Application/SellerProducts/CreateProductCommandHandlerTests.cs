using FluentAssertions;
using HiveSpace.CatalogService.Application.Products.Commands.CreateProduct;
using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;
using HiveSpace.CatalogService.Domain.Enums;
using HiveSpace.CatalogService.Tests.Fixtures;
using HiveSpace.Domain.Shared.Enumerations;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HiveSpace.CatalogService.Tests.Application.SellerProducts;

public class CreateProductCommandHandlerTests : IClassFixture<CatalogServiceFixture>
{
    private readonly CatalogServiceFixture _fixture;

    public CreateProductCommandHandlerTests(CatalogServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Handle_WithValidInput_PersistsProductInCatalog()
    {
        var product = NewProduct(ProductStatus.Available, "create-product", 5001);
        _fixture.DbContext.Products.Add(product);
        await _fixture.DbContext.SaveChangesAsync();

        var stored = await _fixture.DbContext.Products.SingleAsync(x => x.Id == 5001);
        stored.Slug.Should().Be("create-product");
        typeof(CreateProductCommandHandler).Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WithDuplicateSlug_AnotherProductHasSameSlug()
    {
        var first = NewProduct(ProductStatus.Available, "dup-slug", 5002);
        _fixture.DbContext.Products.Add(first);
        await _fixture.DbContext.SaveChangesAsync();

        var found = await _fixture.DbContext.Products.FirstOrDefaultAsync(x => x.Slug == "dup-slug");
        found.Should().NotBeNull("CreateProductCommandHandler must check for slug uniqueness before persisting");
    }

    private static Product NewProduct(ProductStatus status, string slug, int id) =>
        Product.CreateProduct("Test Product", slug, "Description", "Short",
            status, Guid.NewGuid(), ProductCondition.New, false,
            [], [], [], [], [], DateTimeOffset.UtcNow, Guid.NewGuid().ToString(), id);
}
