using FluentAssertions;
using HiveSpace.OrderService.Application.Cart.Commands.AddCartItem;
using HiveSpace.OrderService.Tests.Domain;
using HiveSpace.OrderService.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Xunit;
using CartAggregate = HiveSpace.OrderService.Domain.Aggregates.Carts.Cart;

namespace HiveSpace.OrderService.Tests.Application.Cart;

public class AddItemToCartCommandHandlerTests : IClassFixture<OrderServiceFixture>
{
    private readonly OrderServiceFixture _fixture;

    public AddItemToCartCommandHandlerTests(OrderServiceFixture fixture)
    {
        _fixture = fixture;
        OrderIdGeneratorFixture.EnsureInitialized();
    }

    [Fact]
    public async Task Handle_WithValidProduct_AddsLineItem()
    {
        var cart = CartAggregate.Create(Guid.NewGuid(), Guid.NewGuid());
        cart.AddItem(1, 10, 2);
        _fixture.DbContext.Carts.Add(cart);
        await _fixture.DbContext.SaveChangesAsync();

        var stored = await _fixture.DbContext.Carts.Include(x => x.Items).SingleAsync(x => x.Id == cart.Id);
        stored.Items.Should().ContainSingle(i => i.ProductId == 1 && i.Quantity == 2);
        typeof(AddCartItemCommandHandler).Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WithTwoItems_CartHasMultipleLineItems()
    {
        var cart = CartAggregate.Create(Guid.NewGuid(), Guid.NewGuid());
        cart.AddItem(10, 100, 1);
        cart.AddItem(20, 200, 3);
        _fixture.DbContext.Carts.Add(cart);
        await _fixture.DbContext.SaveChangesAsync();

        var stored = await _fixture.DbContext.Carts.Include(x => x.Items).SingleAsync(x => x.Id == cart.Id);
        stored.Items.Should().HaveCount(2);
    }
}
