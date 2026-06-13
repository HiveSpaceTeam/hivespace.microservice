using FluentAssertions;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.OrderService.Domain.Aggregates.Coupons;
using HiveSpace.OrderService.Domain.Enumerations;
using Xunit;

namespace HiveSpace.OrderService.Tests.Domain;

public class CouponTests
{
    public CouponTests()
    {
        OrderIdGeneratorFixture.EnsureInitialized();
    }

    [Fact]
    public void ValidCoupon_ReducesOrderTotal()
    {
        var coupon = Coupon.CreateByPlatform(
            Guid.NewGuid().ToString(),
            "SAVE10",
            "Save 10",
            DiscountType.FixedAmount,
            null,
            Money.FromVND(10_000),
            CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddMinutes(-1),
            DateTimeOffset.UtcNow.AddDays(1),
            id: Guid.NewGuid());

        var discount = coupon.CalculateDiscount(Money.FromVND(50_000));

        discount.Amount.Should().Be(10_000);
    }

    [Fact]
    public void ExpiredCoupon_IsRejected()
    {
        var coupon = Coupon.CreateByPlatform(
            Guid.NewGuid().ToString(),
            "OLD",
            "Expired",
            DiscountType.FixedAmount,
            null,
            Money.FromVND(10_000),
            CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddDays(-2),
            DateTimeOffset.UtcNow.AddDays(-1),
            id: Guid.NewGuid());

        var result = coupon.Validate(Guid.NewGuid(), Money.FromVND(50_000));

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void AlreadyUsedCoupon_IsRejected()
    {
        var userId = Guid.NewGuid();
        var coupon = Coupon.CreateByPlatform(
            Guid.NewGuid().ToString(),
            "ONCE",
            "Once",
            DiscountType.FixedAmount,
            null,
            Money.FromVND(10_000),
            CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddMinutes(-1),
            DateTimeOffset.UtcNow.AddDays(1),
            id: Guid.NewGuid());
        coupon.SetMaxUsagePerUser(1);
        coupon.MarkAsUsed(userId, Guid.NewGuid(), Money.FromVND(10_000));

        var act = () => coupon.MarkAsUsed(userId, Guid.NewGuid(), Money.FromVND(10_000));

        act.Should().Throw<HiveSpace.Domain.Shared.Exceptions.DomainException>();
    }
}
