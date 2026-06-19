using FluentAssertions;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.OrderService.Application.Cart.Commands.RemovePlatformCoupon;
using HiveSpace.OrderService.Infrastructure.Repositories;
using HiveSpace.OrderService.Tests.Domain;
using HiveSpace.OrderService.Tests.Fixtures;
using HiveSpace.Testing.Shared.Doubles;
using Microsoft.EntityFrameworkCore;
using Xunit;
using CartAggregate = HiveSpace.OrderService.Domain.Aggregates.Carts.Cart;

namespace HiveSpace.OrderService.Tests.Application.Cart;

public class RemovePlatformCouponCommandHandlerTests : IClassFixture<OrderServiceFixture>
{
    private readonly OrderServiceFixture _fixture;

    public RemovePlatformCouponCommandHandlerTests(OrderServiceFixture fixture)
    {
        _fixture = fixture;
        OrderIdGeneratorFixture.EnsureInitialized();
    }

    [Fact]
    public async Task Handle_WithAppliedCoupon_RemovesCoupon()
    {
        var userId = Guid.NewGuid();
        var cart = CartAggregate.Create(userId);
        cart.AddItem(1L, 10L, 1);
        cart.ApplyPlatformCoupon("REMOVE_ME");
        _fixture.DbContext.Carts.Add(cart);
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new RemovePlatformCouponCommandHandler(
            new SqlCartRepository(_fixture.DbContext),
            new FakeUserContext { UserId = userId });

        await handler.Handle(
            new RemovePlatformCouponCommand("REMOVE_ME"), CancellationToken.None);

        var stored = await _fixture.DbContext.Carts
            .Include(c => c.AppliedPlatformCoupons)
            .SingleAsync(c => c.UserId == userId);
        stored.AppliedPlatformCoupons.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithNoCart_ThrowsNotFoundException()
    {
        var handler = new RemovePlatformCouponCommandHandler(
            new SqlCartRepository(_fixture.DbContext),
            new FakeUserContext { UserId = Guid.NewGuid() });

        var act = () => handler.Handle(
            new RemovePlatformCouponCommand("NONEXISTENT"), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
