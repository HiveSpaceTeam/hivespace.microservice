using FluentAssertions;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.OrderService.Application.Cart.Commands.ApplyStoreCoupon;
using HiveSpace.OrderService.Domain.Aggregates.Coupons;
using HiveSpace.OrderService.Domain.Enumerations;
using HiveSpace.OrderService.Infrastructure.Repositories;
using HiveSpace.OrderService.Tests.Domain;
using HiveSpace.OrderService.Tests.Fixtures;
using HiveSpace.Testing.Shared.Doubles;
using Xunit;
using CartAggregate = HiveSpace.OrderService.Domain.Aggregates.Carts.Cart;

namespace HiveSpace.OrderService.Tests.Application.Cart;

public class ApplyStoreCouponCommandHandlerTests : IClassFixture<OrderServiceFixture>
{
    private readonly OrderServiceFixture _fixture;

    public ApplyStoreCouponCommandHandlerTests(OrderServiceFixture fixture)
    {
        _fixture = fixture;
        OrderIdGeneratorFixture.EnsureInitialized();
    }

    [Fact]
    public async Task Handle_WithValidStoreCoupon_AppliesCoupon()
    {
        var userId    = Guid.NewGuid();
        var storeId   = Guid.NewGuid();
        var ownerId   = Guid.NewGuid();

        var cart = CartAggregate.Create(userId);
        _fixture.DbContext.Carts.Add(cart);

        var coupon = Coupon.CreateByStore(
            storeId, ownerId,
            "STORE20", "Store 20k",
            DiscountType.FixedAmount, null, Money.FromVND(20_000),
            CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddMinutes(-1),
            DateTimeOffset.UtcNow.AddDays(1),
            id: Guid.NewGuid());
        _fixture.DbContext.Coupons.Add(coupon);
        await _fixture.DbContext.SaveChangesAsync();

        var row = FakeCheckoutQuery.MakeRow(storeId, quantity: 2, price: 50_000);
        var handler = new ApplyStoreCouponCommandHandler(
            new SqlCartRepository(_fixture.DbContext),
            new FakeCheckoutQuery(row),
            new SqlCouponRepository(_fixture.DbContext),
            new FakeUserContext { UserId = userId });

        var result = await handler.Handle(
            new ApplyStoreCouponCommand(storeId, "STORE20"), CancellationToken.None);

        result.CouponCode.Should().Be("STORE20");
        result.StoreId.Should().Be(storeId);
    }

    [Fact]
    public async Task Handle_WithNoCart_ThrowsNotFoundException()
    {
        var storeId = Guid.NewGuid();
        var handler = new ApplyStoreCouponCommandHandler(
            new SqlCartRepository(_fixture.DbContext),
            new FakeCheckoutQuery(FakeCheckoutQuery.MakeRow(storeId)),
            new SqlCouponRepository(_fixture.DbContext),
            new FakeUserContext { UserId = Guid.NewGuid() });

        var act = () => handler.Handle(
            new ApplyStoreCouponCommand(storeId, "ANYCODE"), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WithPlatformCouponCode_ThrowsInvalidFieldException()
    {
        var userId  = Guid.NewGuid();
        var storeId = Guid.NewGuid();

        var cart = CartAggregate.Create(userId);
        _fixture.DbContext.Carts.Add(cart);

        var coupon = Coupon.CreateByPlatform(
            Guid.NewGuid().ToString(),
            "STORE_PLAT1", "Platform Coupon",
            DiscountType.FixedAmount, null, Money.FromVND(5_000),
            CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1));
        _fixture.DbContext.Coupons.Add(coupon);
        await _fixture.DbContext.SaveChangesAsync();

        var row = FakeCheckoutQuery.MakeRow(storeId, quantity: 2, price: 50_000);
        var handler = new ApplyStoreCouponCommandHandler(
            new SqlCartRepository(_fixture.DbContext),
            new FakeCheckoutQuery(row),
            new SqlCouponRepository(_fixture.DbContext),
            new FakeUserContext { UserId = userId });

        var act = () => handler.Handle(
            new ApplyStoreCouponCommand(storeId, "STORE_PLAT1"), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidFieldException>();
    }

    [Fact]
    public async Task Handle_WithNotApplicableCoupon_ThrowsInvalidFieldException()
    {
        var userId  = Guid.NewGuid();
        var storeId = Guid.NewGuid();

        var cart = CartAggregate.Create(userId);
        _fixture.DbContext.Carts.Add(cart);

        var coupon = Coupon.CreateByStore(
            storeId, Guid.NewGuid(),
            "STORE_MINFAIL1", "Min Fail",
            DiscountType.FixedAmount, null, Money.FromVND(5_000),
            CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1),
            minOrderAmount: Money.FromVND(999_999));
        _fixture.DbContext.Coupons.Add(coupon);
        await _fixture.DbContext.SaveChangesAsync();

        var row = FakeCheckoutQuery.MakeRow(storeId, quantity: 1, price: 50_000);
        var handler = new ApplyStoreCouponCommandHandler(
            new SqlCartRepository(_fixture.DbContext),
            new FakeCheckoutQuery(row),
            new SqlCouponRepository(_fixture.DbContext),
            new FakeUserContext { UserId = userId });

        var act = () => handler.Handle(
            new ApplyStoreCouponCommand(storeId, "STORE_MINFAIL1"), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidFieldException>();
    }
}
