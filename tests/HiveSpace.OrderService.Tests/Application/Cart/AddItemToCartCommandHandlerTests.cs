using FluentAssertions;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.OrderService.Application.Cart.Commands.AddCartItem;
using HiveSpace.OrderService.Infrastructure.Repositories;
using HiveSpace.OrderService.Tests.Domain;
using HiveSpace.OrderService.Tests.Fixtures;
using HiveSpace.Testing.Shared.Doubles;
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
    public async Task Handle_WithValidSku_CreatesCartAndReturnsItemId()
    {
        var sku = new HiveSpace.OrderService.Domain.External.SkuRef(
            10L, 1L, "SKU-01", 50_000, "VND", null, null);
        _fixture.DbContext.SkuRefs.Add(sku);
        await _fixture.DbContext.SaveChangesAsync();

        var userId = Guid.NewGuid();
        var handler = new AddCartItemCommandHandler(
            new SqlCartRepository(_fixture.DbContext),
            new SqlSkuRefRepository(_fixture.DbContext),
            new FakeUserContext { UserId = userId });

        var cartItemId = await handler.Handle(
            new AddCartItemCommand(1L, 10L, 2), CancellationToken.None);

        cartItemId.Should().NotBeEmpty();
        var cart = await _fixture.DbContext.Carts
            .Include(c => c.Items)
            .SingleAsync(c => c.UserId == userId);
        cart.Items.Should().ContainSingle(i => i.ProductId == 1L && i.Quantity == 2);
    }

    [Fact]
    public async Task Handle_WithNonExistentSku_ThrowsNotFoundException()
    {
        var userId = Guid.NewGuid();
        var handler = new AddCartItemCommandHandler(
            new SqlCartRepository(_fixture.DbContext),
            new SqlSkuRefRepository(_fixture.DbContext),
            new FakeUserContext { UserId = userId });

        var act = () => handler.Handle(
            new AddCartItemCommand(999L, 9999L, 1), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
