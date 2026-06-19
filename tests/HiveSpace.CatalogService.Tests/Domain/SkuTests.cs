using FluentAssertions;
using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;
using HiveSpace.CatalogService.Domain.Exceptions;
using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.Domain.Shared.ValueObjects;
using Xunit;

namespace HiveSpace.CatalogService.Tests.Domain;

public class SkuTests
{
    private static Sku NewSku(int quantity = 10) =>
        new("SKU-001", [], [], quantity, true, Money.Zero(Currency.VND));

    [Fact]
    public void Create_WithId_SetsId()
    {
        var sku = new Sku(1, "SKU-001", [], [], 10, true, Money.Zero(Currency.VND));
        sku.Id.Should().Be(1);
        sku.SkuNo.Should().Be("SKU-001");
        sku.Quantity.Should().Be(10);
    }

    [Fact]
    public void UpdateQuantity_WithValidValue_ChangesQuantity()
    {
        var sku = NewSku(10);
        sku.UpdateQuantity(50);
        sku.Quantity.Should().Be(50);
    }

    [Fact]
    public void UpdateQuantity_WithZero_ChangesQuantity()
    {
        var sku = NewSku(10);
        sku.UpdateQuantity(0);
        sku.Quantity.Should().Be(0);
    }

    [Fact]
    public void UpdateQuantity_WithNegativeValue_ThrowsInvalidQuantityException()
    {
        var sku = NewSku();
        var act = () => sku.UpdateQuantity(-1);
        act.Should().Throw<InvalidQuantityException>();
    }

    [Fact]
    public void UpdateSkuImageUrl_WithMatchingFileId_SetsImageUrl()
    {
        var image = new SkuImage("file-001");
        var sku = new Sku("SKU-001", [], [image], 10, true, Money.Zero(Currency.VND));
        sku.UpdateSkuImageUrl("file-001", "http://sku-image.url");
        sku.Images.Should().ContainSingle(i => i.ImageUrl == "http://sku-image.url");
    }

    [Fact]
    public void UpdateSkuImageUrl_WithNoMatchingFileId_DoesNothing()
    {
        var sku = NewSku();
        var act = () => sku.UpdateSkuImageUrl("nonexistent", "http://url");
        act.Should().NotThrow();
    }
}
