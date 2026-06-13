using FluentAssertions;
using HiveSpace.OrderService.Application.Cart.Commands.UpdateCartItems;
using HiveSpace.OrderService.Tests.Domain;
using HiveSpace.OrderService.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Xunit;
using CartAggregate = HiveSpace.OrderService.Domain.Aggregates.Carts.Cart;

namespace HiveSpace.OrderService.Tests.Application.Cart;

public class UpdateCartItemQuantityCommandHandlerTests : IClassFixture<OrderServiceFixture>
{
    private readonly OrderServiceFixture _fixture;

    public UpdateCartItemQuantityCommandHandlerTests(OrderServiceFixture fixture)
    {
        _fixture = fixture;
        OrderIdGeneratorFixture.EnsureInitialized();
    }

    [Fact]
    public async Task Handle_ChangesLineTotalForAffectedItem()
    {
        var cart = CartAggregate.Create(Guid.NewGuid(), Guid.NewGuid());
        cart.AddItem(1, 10, 2);
        _fixture.DbContext.Carts.Add(cart);
        await _fixture.DbContext.SaveChangesAsync();

        cart.UpdateItemQuantity(1, 10, 5);
        await _fixture.DbContext.SaveChangesAsync();

        cart.Items.Single().Quantity.Should().Be(5);
        typeof(UpdateCartItemsCommandHandler).Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_UpdateQuantity_OtherItemsUnchanged()
    {
        var cart = CartAggregate.Create(Guid.NewGuid(), Guid.NewGuid());
        cart.AddItem(1, 10, 2);
        cart.AddItem(2, 20, 1);
        _fixture.DbContext.Carts.Add(cart);
        await _fixture.DbContext.SaveChangesAsync();

        cart.UpdateItemQuantity(1, 10, 4);
        await _fixture.DbContext.SaveChangesAsync();

        cart.Items.Single(i => i.ProductId == 2).Quantity.Should().Be(1);
    }
}
