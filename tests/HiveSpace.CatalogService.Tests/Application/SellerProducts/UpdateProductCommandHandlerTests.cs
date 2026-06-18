using FluentAssertions;
using HiveSpace.CatalogService.Application.Contracts;
using HiveSpace.CatalogService.Application.Products.Commands.UpdateProduct;
using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;
using HiveSpace.CatalogService.Domain.Enums;
using HiveSpace.CatalogService.Infrastructure.Repositories;
using HiveSpace.CatalogService.Tests.Fakes;
using HiveSpace.CatalogService.Tests.Fixtures;
using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.Testing.Shared.Doubles;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HiveSpace.CatalogService.Tests.Application.SellerProducts;

public class UpdateProductCommandHandlerTests : IClassFixture<CatalogServiceFixture>
{
    private readonly CatalogServiceFixture _fixture;

    public UpdateProductCommandHandlerTests(CatalogServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Handle_WithExistingProduct_UpdatesNameAndReturnsTrue()
    {
        var product = NewProduct("update-slug-1", 20001);
        _fixture.DbContext.Products.Add(product);
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new UpdateProductCommandHandler(
            new SqlProductRepository(_fixture.DbContext),
            new FakeCatalogTransactionService(_fixture.DbContext),
            new FakeUserContext { UserId = Guid.NewGuid() },
            new FakeProductEventPublisher());

        var result = await handler.Handle(
            new UpdateProductCommand(20001, new ProductUpsertRequestDto("Updated Name", 0, "")),
            CancellationToken.None);

        result.Should().BeTrue();
        var stored = await _fixture.DbContext.Products.FirstOrDefaultAsync(p => p.Id == 20001);
        stored!.Name.Should().Be("Updated Name");
    }

    [Fact]
    public async Task Handle_WithNonExistentProduct_ReturnsFalse()
    {
        var handler = new UpdateProductCommandHandler(
            new SqlProductRepository(_fixture.DbContext),
            new FakeCatalogTransactionService(_fixture.DbContext),
            new FakeUserContext { UserId = Guid.NewGuid() },
            new FakeProductEventPublisher());

        var result = await handler.Handle(
            new UpdateProductCommand(99999, new ProductUpsertRequestDto("Name", 0, "")),
            CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithVariantsSkusAndAttributes_ReplacesNestedCollections()
    {
        var product = NewProduct("update-slug-2", 20002);
        _fixture.DbContext.Products.Add(product);
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new UpdateProductCommandHandler(
            new SqlProductRepository(_fixture.DbContext),
            new FakeCatalogTransactionService(_fixture.DbContext),
            new FakeUserContext { UserId = Guid.NewGuid() },
            new FakeProductEventPublisher());

        var result = await handler.Handle(
            new UpdateProductCommand(
                20002,
                new ProductUpsertRequestDto(
                    "Updated Configurable Product",
                    7,
                    "Updated description",
                    Variants:
                    [
                        new ProductVariantRequestDto(
                            0,
                            "Size",
                            [new ProductVariantOptionRequestDto("S"), new ProductVariantOptionRequestDto("M")])
                    ],
                    Skus:
                    [
                        new ProductSkuRequestDto(
                            0,
                            [new ProductSkuVariantRequestDto("Size", "M")],
                            Money.FromVND(299_000),
                            8,
                            "SKU-M")
                    ],
                    Attributes:
                    [
                        new ProductAttributeRequestDto(20, [301], null),
                        new ProductAttributeRequestDto(21, null, "Slim fit")
                    ])),
            CancellationToken.None);

        result.Should().BeTrue();

        var stored = await _fixture.DbContext.Products
            .Include(p => p.Categories)
            .Include(p => p.Variants)
                .ThenInclude(v => v.Options)
            .Include(p => p.Skus)
                .ThenInclude(s => s.SkuVariants)
            .Include(p => p.Attributes)
            .SingleAsync(p => p.Id == 20002);

        stored.Name.Should().Be("Updated Configurable Product");
        stored.Description.Should().Be("Updated description");
        stored.Categories.Should().ContainSingle(c => c.CategoryId == 7);
        stored.Variants.Should().ContainSingle(v => v.Name == "Size");
        stored.Variants.Single().Options.Should().HaveCount(2);
        stored.Skus.Should().ContainSingle(s => s.SkuNo == "SKU-M");
        stored.Skus.Single().SkuVariants.Should().ContainSingle(v => v.VariantName == "Size" && v.Value == "M");
        stored.Attributes.Should().HaveCount(2);
    }

    private static Product NewProduct(string slug, int id) =>
        Product.CreateProduct("Original Name", slug, "Description", "Short",
            ProductStatus.Available, Guid.NewGuid(), ProductCondition.New, false,
            [], [], [], [], [], DateTimeOffset.UtcNow, Guid.NewGuid().ToString(), id);
}
