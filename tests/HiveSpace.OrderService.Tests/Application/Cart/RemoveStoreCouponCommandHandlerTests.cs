using FluentAssertions;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.OrderService.Application.Cart.Commands.RemoveStoreCoupon;
using HiveSpace.OrderService.Infrastructure.Repositories;
using HiveSpace.OrderService.Tests.Domain;
using HiveSpace.OrderService.Tests.Fixtures;
using HiveSpace.Testing.Shared.Doubles;
using Microsoft.EntityFrameworkCore;
using Xunit;
using CartAggregate = HiveSpace.OrderService.Domain.Aggregates.Carts.Cart;

namespace HiveSpace.OrderService.Tests.Application.Cart;

public class RemoveStoreCouponCommandHandlerTests : IClassFixture<OrderServiceFixture>
{
    private readonly OrderServiceFixture _fixture;

    public RemoveStoreCouponCommandHandlerTests(OrderServiceFixture fixture)
    {
        _fixture = fixture;
        OrderIdGeneratorFixture.EnsureInitialized();
    }

    [Fact]
    public async Task Handle_WithAppliedStoreCoupon_RemovesCoupon()
    {
        var userId  = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var cart = CartAggregate.Create(userId);
        cart.AddItem(1L, 10L, 1);
        cart.ApplyStoreCoupon(storeId, "STORE_REMOVE");
        _fixture.DbContext.Carts.Add(cart);
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new RemoveStoreCouponCommandHandler(
            new SqlCartRepository(_fixture.DbContext),
            new FakeUserContext { UserId = userId });

        await handler.Handle(
            new RemoveStoreCouponCommand(storeId), CancellationToken.None);

        var stored = await _fixture.DbContext.Carts
            .Include(c => c.AppliedStoreCoupons)
            .SingleAsync(c => c.UserId == userId);
        stored.AppliedStoreCoupons.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithNoCart_ThrowsNotFoundException()
    {
        var handler = new RemoveStoreCouponCommandHandler(
            new SqlCartRepository(_fixture.DbContext),
            new FakeUserContext { UserId = Guid.NewGuid() });

        var act = () => handler.Handle(
            new RemoveStoreCouponCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
