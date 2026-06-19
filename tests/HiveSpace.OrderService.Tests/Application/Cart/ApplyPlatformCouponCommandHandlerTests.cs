using FluentAssertions;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.OrderService.Application.Cart.Commands.ApplyPlatformCoupon;
using HiveSpace.OrderService.Domain.Aggregates.Coupons;
using HiveSpace.OrderService.Domain.Enumerations;
using HiveSpace.OrderService.Infrastructure.Repositories;
using HiveSpace.OrderService.Tests.Domain;
using HiveSpace.OrderService.Tests.Fixtures;
using HiveSpace.Testing.Shared.Doubles;
using Xunit;
using CartAggregate = HiveSpace.OrderService.Domain.Aggregates.Carts.Cart;

namespace HiveSpace.OrderService.Tests.Application.Cart;

public class ApplyPlatformCouponCommandHandlerTests : IClassFixture<OrderServiceFixture>
{
    private readonly OrderServiceFixture _fixture;

    public ApplyPlatformCouponCommandHandlerTests(OrderServiceFixture fixture)
    {
        _fixture = fixture;
        OrderIdGeneratorFixture.EnsureInitialized();
    }

    [Fact]
    public async Task Handle_WithValidCoupon_AppliesCouponToCart()
    {
        var userId  = Guid.NewGuid();
        var storeId = Guid.NewGuid();

        var cart = CartAggregate.Create(userId);
        _fixture.DbContext.Carts.Add(cart);

        var coupon = Coupon.CreateByPlatform(
            Guid.NewGuid().ToString(),
            "PLAT10",
            "Platform 10k",
            DiscountType.FixedAmount,
            null,
            Money.FromVND(10_000),
            CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddMinutes(-1),
            DateTimeOffset.UtcNow.AddDays(1),
            id: Guid.NewGuid());
        _fixture.DbContext.Coupons.Add(coupon);
        await _fixture.DbContext.SaveChangesAsync();

        var row = FakeCheckoutQuery.MakeRow(storeId, quantity: 2, price: 50_000);
        var handler = new ApplyPlatformCouponCommandHandler(
            new SqlCartRepository(_fixture.DbContext),
            new FakeCheckoutQuery(row),
            new SqlCouponRepository(_fixture.DbContext),
            new FakeUserContext { UserId = userId });

        var result = await handler.Handle(
            new ApplyPlatformCouponCommand("PLAT10"), CancellationToken.None);

        result.CouponCode.Should().Be("PLAT10");
    }

    [Fact]
    public async Task Handle_WithNoCart_ThrowsNotFoundException()
    {
        var storeId = Guid.NewGuid();
        var handler = new ApplyPlatformCouponCommandHandler(
            new SqlCartRepository(_fixture.DbContext),
            new FakeCheckoutQuery(FakeCheckoutQuery.MakeRow(storeId)),
            new SqlCouponRepository(_fixture.DbContext),
            new FakeUserContext { UserId = Guid.NewGuid() });

        var act = () => handler.Handle(
            new ApplyPlatformCouponCommand("ANYCODE"), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WithStoreCouponCode_ThrowsInvalidFieldException()
    {
        var userId  = Guid.NewGuid();
        var storeId = Guid.NewGuid();

        var cart = CartAggregate.Create(userId);
        _fixture.DbContext.Carts.Add(cart);

        var coupon = Coupon.CreateByStore(
            storeId, Guid.NewGuid(),
            "PLAT_STORE1", "Store Coupon",
            DiscountType.FixedAmount, null, Money.FromVND(5_000),
            CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1));
        _fixture.DbContext.Coupons.Add(coupon);
        await _fixture.DbContext.SaveChangesAsync();

        var row = FakeCheckoutQuery.MakeRow(storeId, quantity: 2, price: 50_000);
        var handler = new ApplyPlatformCouponCommandHandler(
            new SqlCartRepository(_fixture.DbContext),
            new FakeCheckoutQuery(row),
            new SqlCouponRepository(_fixture.DbContext),
            new FakeUserContext { UserId = userId });

        var act = () => handler.Handle(
            new ApplyPlatformCouponCommand("PLAT_STORE1"), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidFieldException>();
    }

    [Fact]
    public async Task Handle_WithCouponValidationFailure_ThrowsInvalidFieldException()
    {
        var userId  = Guid.NewGuid();
        var storeId = Guid.NewGuid();

        var cart = CartAggregate.Create(userId);
        _fixture.DbContext.Carts.Add(cart);

        var coupon = Coupon.CreateByPlatform(
            Guid.NewGuid().ToString(),
            "PLAT_MINFAIL1", "Min Fail",
            DiscountType.FixedAmount, null, Money.FromVND(5_000),
            CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1),
            minOrderAmount: Money.FromVND(999_999));
        _fixture.DbContext.Coupons.Add(coupon);
        await _fixture.DbContext.SaveChangesAsync();

        var row = FakeCheckoutQuery.MakeRow(storeId, quantity: 1, price: 50_000);
        var handler = new ApplyPlatformCouponCommandHandler(
            new SqlCartRepository(_fixture.DbContext),
            new FakeCheckoutQuery(row),
            new SqlCouponRepository(_fixture.DbContext),
            new FakeUserContext { UserId = userId });

        var act = () => handler.Handle(
            new ApplyPlatformCouponCommand("PLAT_MINFAIL1"), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidFieldException>();
    }
}
