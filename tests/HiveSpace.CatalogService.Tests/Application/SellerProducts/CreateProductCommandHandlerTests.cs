using FluentAssertions;
using HiveSpace.CatalogService.Application.Contracts;
using HiveSpace.CatalogService.Application.Products.Commands.CreateProduct;
using HiveSpace.CatalogService.Infrastructure.Repositories;
using HiveSpace.CatalogService.Tests.Fakes;
using HiveSpace.CatalogService.Tests.Fixtures;
using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.Testing.Shared.Doubles;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HiveSpace.CatalogService.Tests.Application.SellerProducts;

public class CreateProductCommandHandlerTests : IClassFixture<CatalogServiceFixture>
{
    private readonly CatalogServiceFixture _fixture;

    public CreateProductCommandHandlerTests(CatalogServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Handle_WithValidPayload_PersistsProductAndReturnsId()
    {
        var sellerId = Guid.NewGuid();
        var handler = new CreateProductCommandHandler(
            new SqlProductRepository(_fixture.DbContext),
            new FakeCatalogTransactionService(_fixture.DbContext),
            new FakeUserContext { UserId = Guid.NewGuid(), StoreId = sellerId },
            new FakeProductEventPublisher());

        var command = new CreateProductCommand(new ProductUpsertRequestDto("New Phone", 1, "A great phone"));

        var productId = await handler.Handle(command, CancellationToken.None);

        productId.Should().BeGreaterThan(0);
        var stored = await _fixture.DbContext.Products.FirstOrDefaultAsync(p => p.Id == productId);
        stored.Should().NotBeNull();
        stored!.Name.Should().Be("New Phone");
        stored.SellerId.Should().Be(sellerId);
    }

    [Fact]
    public async Task Handle_WithSellerContext_ProductHasCorrectSellerId()
    {
        var sellerA = Guid.NewGuid();
        var sellerB = Guid.NewGuid();
        var handlerA = new CreateProductCommandHandler(
            new SqlProductRepository(_fixture.DbContext),
            new FakeCatalogTransactionService(_fixture.DbContext),
            new FakeUserContext { UserId = Guid.NewGuid(), StoreId = sellerA },
            new FakeProductEventPublisher());
        var handlerB = new CreateProductCommandHandler(
            new SqlProductRepository(_fixture.DbContext),
            new FakeCatalogTransactionService(_fixture.DbContext),
            new FakeUserContext { UserId = Guid.NewGuid(), StoreId = sellerB },
            new FakeProductEventPublisher());

        var idA = await handlerA.Handle(new CreateProductCommand(new ProductUpsertRequestDto("Laptop A", 2, "Desc")), CancellationToken.None);
        var idB = await handlerB.Handle(new CreateProductCommand(new ProductUpsertRequestDto("Laptop B", 2, "Desc")), CancellationToken.None);

        var productA = await _fixture.DbContext.Products.FindAsync(idA);
        var productB = await _fixture.DbContext.Products.FindAsync(idB);
        productA!.SellerId.Should().Be(sellerA);
        productB!.SellerId.Should().Be(sellerB);
    }

    [Fact]
    public async Task Handle_WithVariantsSkusAndAttributes_PersistsNestedProductData()
    {
        var sellerId = Guid.NewGuid();
        var handler = new CreateProductCommandHandler(
            new SqlProductRepository(_fixture.DbContext),
            new FakeCatalogTransactionService(_fixture.DbContext),
            new FakeUserContext { UserId = Guid.NewGuid(), StoreId = sellerId },
            new FakeProductEventPublisher());

        var command = new CreateProductCommand(new ProductUpsertRequestDto(
            "Configurable Tee",
            3,
            "Has variants and attributes",
            Variants:
            [
                new ProductVariantRequestDto(
                    0,
                    "Color",
                    [new ProductVariantOptionRequestDto("Red"), new ProductVariantOptionRequestDto("Blue")])
            ],
            Skus:
            [
                new ProductSkuRequestDto(
                    0,
                    [new ProductSkuVariantRequestDto("Color", "Red")],
                    Money.FromVND(199_000),
                    12,
                    "TEE-RED")
            ],
            Attributes:
            [
                new ProductAttributeRequestDto(10, [101, 102], null),
                new ProductAttributeRequestDto(11, null, "Cotton")
            ]));

        var productId = await handler.Handle(command, CancellationToken.None);

        var stored = await _fixture.DbContext.Products
            .Include(p => p.Categories)
            .Include(p => p.Variants)
                .ThenInclude(v => v.Options)
            .Include(p => p.Skus)
                .ThenInclude(s => s.SkuVariants)
            .Include(p => p.Attributes)
            .SingleAsync(p => p.Id == productId);

        stored.Categories.Should().ContainSingle(c => c.CategoryId == 3);
        stored.Variants.Should().ContainSingle(v => v.Name == "Color");
        stored.Variants.Single().Options.Should().HaveCount(2);
        stored.Skus.Should().ContainSingle(s => s.SkuNo == "TEE-RED");
        stored.Skus.Single().Quantity.Should().Be(12);
        stored.Attributes.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WhenStoreIdIsNull_UsesFallbackGuid()
    {
        var handler = new CreateProductCommandHandler(
            new SqlProductRepository(_fixture.DbContext),
            new FakeCatalogTransactionService(_fixture.DbContext),
            new FakeUserContext { UserId = Guid.NewGuid(), StoreId = null },
            new FakeProductEventPublisher());

        var productId = await handler.Handle(
            new CreateProductCommand(new ProductUpsertRequestDto("Budget Phone", 1, "Entry level")),
            CancellationToken.None);

        var stored = await _fixture.DbContext.Products.FindAsync(productId);
        stored.Should().NotBeNull();
        stored!.SellerId.Should().Be(Guid.Empty);
    }
}
