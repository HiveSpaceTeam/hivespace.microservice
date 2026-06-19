using FluentAssertions;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.OrderService.Application.Cart.Commands.RemoveCartItem;
using HiveSpace.OrderService.Infrastructure.Repositories;
using HiveSpace.OrderService.Tests.Domain;
using HiveSpace.OrderService.Tests.Fixtures;
using HiveSpace.Testing.Shared.Doubles;
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
    public async Task Handle_WithExistingItem_RemovesItemFromCart()
    {
        var userId = Guid.NewGuid();
        var cart = CartAggregate.Create(userId);
        cart.AddItem(1L, 10L, 2);
        cart.AddItem(2L, 20L, 1);
        _fixture.DbContext.Carts.Add(cart);
        await _fixture.DbContext.SaveChangesAsync();

        var cartItemId = cart.Items.First(i => i.ProductId == 1L).Id;

        var handler = new RemoveCartItemCommandHandler(
            new SqlCartRepository(_fixture.DbContext),
            new FakeUserContext { UserId = userId });

        await handler.Handle(new RemoveCartItemCommand(cartItemId), CancellationToken.None);

        var stored = await _fixture.DbContext.Carts
            .Include(c => c.Items)
            .SingleAsync(c => c.UserId == userId);
        stored.Items.Should().NotContain(i => i.ProductId == 1L);
        stored.Items.Should().ContainSingle(i => i.ProductId == 2L);
    }

    [Fact]
    public async Task Handle_WithNoCart_ThrowsNotFoundException()
    {
        var handler = new RemoveCartItemCommandHandler(
            new SqlCartRepository(_fixture.DbContext),
            new FakeUserContext { UserId = Guid.NewGuid() });

        var act = () => handler.Handle(
            new RemoveCartItemCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
