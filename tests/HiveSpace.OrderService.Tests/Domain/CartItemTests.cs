using FluentAssertions;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.OrderService.Domain.Aggregates.Carts;
using Xunit;

namespace HiveSpace.OrderService.Tests.Domain;

public class CartItemTests
{
    public CartItemTests()
    {
        OrderIdGeneratorFixture.EnsureInitialized();
    }

    [Fact]
    public void Create_WithValidFields_CreatesItem()
    {
        var item = CartItem.Create(1L, 10L, 3);

        item.ProductId.Should().Be(1L);
        item.SkuId.Should().Be(10L);
        item.Quantity.Should().Be(3);
        item.IsSelected.Should().BeTrue();
        item.CreatedAt.Should().Be(default);
        item.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void Create_WithZeroProductId_ThrowsDomainException()
    {
        var act = () => CartItem.Create(0L, 10L, 1);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_WithNegativeProductId_ThrowsDomainException()
    {
        var act = () => CartItem.Create(-1L, 10L, 1);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_WithZeroSkuId_ThrowsDomainException()
    {
        var act = () => CartItem.Create(1L, 0L, 1);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_WithZeroQuantity_ThrowsDomainException()
    {
        var act = () => CartItem.Create(1L, 10L, 0);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void UpdateSku_WithValidSkuId_UpdatesSkuId()
    {
        var item = CartItem.Create(1L, 10L, 1);

        item.UpdateSku(20L);

        item.SkuId.Should().Be(20L);
    }

    [Fact]
    public void UpdateSku_WithZeroSkuId_ThrowsDomainException()
    {
        var item = CartItem.Create(1L, 10L, 1);

        var act = () => item.UpdateSku(0L);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void UpdateQuantity_WithValidQuantity_UpdatesQuantity()
    {
        var item = CartItem.Create(1L, 10L, 1);

        item.UpdateQuantity(5);

        item.Quantity.Should().Be(5);
    }

    [Fact]
    public void UpdateQuantity_WithZeroQuantity_ThrowsDomainException()
    {
        var item = CartItem.Create(1L, 10L, 1);

        var act = () => item.UpdateQuantity(0);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void UpdateSelection_ToFalse_SetsIsSelectedFalse()
    {
        var item = CartItem.Create(1L, 10L, 1);

        item.UpdateSelection(false);

        item.IsSelected.Should().BeFalse();
    }

    [Fact]
    public void UpdateSelection_ToTrue_SetsIsSelectedTrue()
    {
        var item = CartItem.Create(1L, 10L, 1);
        item.UpdateSelection(false);

        item.UpdateSelection(true);

        item.IsSelected.Should().BeTrue();
    }
}
