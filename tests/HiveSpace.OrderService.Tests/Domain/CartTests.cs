using FluentAssertions;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.OrderService.Domain.Aggregates.Carts;
using Xunit;

namespace HiveSpace.OrderService.Tests.Domain;

public class CartTests
{
    public CartTests()
    {
        OrderIdGeneratorFixture.EnsureInitialized();
    }

    [Fact]
    public void AddItem_UpdatesCartCount()
    {
        var cart = Cart.Create(Guid.NewGuid(), Guid.NewGuid());

        cart.AddItem(1, 10, 2);

        cart.GetTotalItemCount().Should().Be(2);
    }

    [Fact]
    public void AddSameProductTwice_MergesIntoOneLine()
    {
        var cart = Cart.Create(Guid.NewGuid(), Guid.NewGuid());

        cart.AddItem(1, 10, 2);
        cart.AddItem(1, 10, 3);

        cart.Items.Should().ContainSingle();
        cart.GetTotalItemCount().Should().Be(5);
    }

    [Fact]
    public void UpdateQuantity_ChangesLineTotal()
    {
        var cart = Cart.Create(Guid.NewGuid(), Guid.NewGuid());
        cart.AddItem(1, 10, 2);

        cart.UpdateItemQuantity(1, 10, 4);

        cart.Items.Single().Quantity.Should().Be(4);
        cart.GetTotalItemCount().Should().Be(4);
    }

    [Fact]
    public void RemoveItem_LeavesRemainingItems()
    {
        var cart = Cart.Create(Guid.NewGuid(), Guid.NewGuid());
        cart.AddItem(1, 10, 2);
        cart.AddItem(2, 20, 1);

        cart.RemoveItem(1, 10);

        cart.Items.Should().ContainSingle(i => i.ProductId == 2);
    }

    [Fact]
    public void InitiateCheckoutOnEmptyCart_ThrowsDomainException()
    {
        var cart = Cart.Create(Guid.NewGuid(), Guid.NewGuid());

        var act = cart.ValidateForCheckout;

        act.Should().Throw<DomainException>();
    }
}
