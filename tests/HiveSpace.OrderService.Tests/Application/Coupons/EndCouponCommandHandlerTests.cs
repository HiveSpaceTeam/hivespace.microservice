using FluentAssertions;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.OrderService.Application.Coupons.Commands.EndCoupon;
using HiveSpace.OrderService.Domain.Aggregates.Coupons;
using HiveSpace.OrderService.Application.Coupons.Dtos;
using HiveSpace.OrderService.Domain.Enumerations;
using HiveSpace.OrderService.Infrastructure.Repositories;
using HiveSpace.OrderService.Tests.Domain;
using HiveSpace.OrderService.Tests.Fixtures;
using HiveSpace.Testing.Shared.Doubles;
using Xunit;

namespace HiveSpace.OrderService.Tests.Application.Coupons;

public class EndCouponCommandHandlerTests : IClassFixture<OrderServiceFixture>
{
    private readonly OrderServiceFixture _fixture;

    public EndCouponCommandHandlerTests(OrderServiceFixture fixture)
    {
        _fixture = fixture;
        OrderIdGeneratorFixture.EnsureInitialized();
    }

    [Fact]
    public async Task Handle_WithActiveCoupon_EndsCoupon()
    {
        var couponId = Guid.NewGuid();
        var coupon = Coupon.CreateByPlatform(
            Guid.NewGuid().ToString(),
            "END01",
            "End Me",
            DiscountType.FixedAmount,
            null,
            Money.FromVND(10_000),
            CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddMinutes(-1),
            DateTimeOffset.UtcNow.AddDays(7),
            id: couponId);

        _fixture.DbContext.Coupons.Add(coupon);
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new EndCouponCommandHandler(
            new SqlCouponRepository(_fixture.DbContext),
            new FakeUserContext { UserId = Guid.NewGuid(), Roles = ["Admin"] });

        var result = await handler.Handle(new EndCouponCommand(couponId), CancellationToken.None);

        result.Should().NotBeNull();
        result.Status.Should().Be(CouponStatus.Expired);
    }

    [Fact]
    public async Task Handle_WithExpiredCoupon_ThrowsInvalidFieldException()
    {
        var couponId = Guid.NewGuid();
        var coupon = Coupon.CreateByPlatform(
            Guid.NewGuid().ToString(),
            "END02",
            "Already Ended",
            DiscountType.FixedAmount,
            null,
            Money.FromVND(10_000),
            CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddMinutes(-2),
            DateTimeOffset.UtcNow.AddMinutes(-1),
            id: couponId);

        _fixture.DbContext.Coupons.Add(coupon);
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new EndCouponCommandHandler(
            new SqlCouponRepository(_fixture.DbContext),
            new FakeUserContext { UserId = Guid.NewGuid(), Roles = ["Admin"] });

        var act = () => handler.Handle(new EndCouponCommand(couponId), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidFieldException>();
    }

    [Fact]
    public async Task Handle_AsSellerForOtherStoresCoupon_ThrowsForbiddenException()
    {
        var storeA = Guid.NewGuid();
        var storeB = Guid.NewGuid();

        var coupon = Coupon.CreateByStore(
            storeA, Guid.NewGuid(),
            "END_FORB1", "Forbidden End",
            DiscountType.FixedAmount, null, Money.FromVND(5_000),
            CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(7));
        _fixture.DbContext.Coupons.Add(coupon);
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new EndCouponCommandHandler(
            new SqlCouponRepository(_fixture.DbContext),
            new FakeUserContext { UserId = Guid.NewGuid(), Roles = ["Seller"], StoreId = storeB });

        var act = () => handler.Handle(new EndCouponCommand(coupon.Id), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }
}
