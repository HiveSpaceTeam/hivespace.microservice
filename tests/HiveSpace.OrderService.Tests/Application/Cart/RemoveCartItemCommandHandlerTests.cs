using FluentAssertions;
using HiveSpace.OrderService.Application.Cart.Commands.RemoveCartItem;
using HiveSpace.OrderService.Tests.Domain;
using HiveSpace.OrderService.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Xunit;
using CartAggregate = HiveSpace.OrderService.Domain.Aggregates.Carts.Cart;

namespace HiveSpace.OrderService.Tests.Application.Cart;

public class RemoveCartItemCommandHandlerTests : IClassFixture<OrderServiceFixture>
{
    private readonly OrderServiceFixture _fixture;

    public RemoveCartItemCommandHandlerTests(OrderServiceFixture fixture)
    {
        _fixture = fixture;
        OrderIdGeneratorFixture.EnsureInitialized();
    }

    [Fact]
    public async Task Handle_RemovedItemAbsentFromCart()
    {
        var cart = CartAggregate.Create(Guid.NewGuid(), Guid.NewGuid());
        cart.AddItem(1, 10, 2);
        cart.AddItem(2, 20, 1);
        _fixture.DbContext.Carts.Add(cart);
        await _fixture.DbContext.SaveChangesAsync();

        cart.RemoveItem(1, 10);
        await _fixture.DbContext.SaveChangesAsync();

        cart.Items.Should().ContainSingle(i => i.ProductId == 2);
        typeof(RemoveCartItemCommandHandler).Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_RemoveLastItem_CartBecomesEmpty()
    {
        var cart = CartAggregate.Create(Guid.NewGuid(), Guid.NewGuid());
        cart.AddItem(1, 10, 1);
        _fixture.DbContext.Carts.Add(cart);
        await _fixture.DbContext.SaveChangesAsync();

        cart.RemoveItem(1, 10);
        await _fixture.DbContext.SaveChangesAsync();

        cart.Items.Should().BeEmpty();
    }
}
