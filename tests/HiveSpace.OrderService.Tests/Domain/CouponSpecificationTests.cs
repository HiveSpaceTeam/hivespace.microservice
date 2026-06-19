using FluentAssertions;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.OrderService.Domain.Aggregates.Coupons;
using HiveSpace.OrderService.Domain.Aggregates.Coupons.Specifications;
using HiveSpace.OrderService.Domain.Enumerations;
using Xunit;

namespace HiveSpace.OrderService.Tests.Domain;

public class CouponSpecificationTests
{
    public CouponSpecificationTests()
    {
        OrderIdGeneratorFixture.EnsureInitialized();
    }

    private static Coupon OngoingPlatformCoupon(Guid? id = null) =>
        Coupon.CreateByPlatform(
            "admin", "CODE", "Name", DiscountType.FixedAmount,
            null, Money.FromVND(10_000), CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddDays(1),
            id: id ?? Guid.NewGuid());

    private static Coupon UpcomingPlatformCoupon() =>
        Coupon.CreateByPlatform(
            "admin", "FUTURE", "Future", DiscountType.FixedAmount,
            null, Money.FromVND(10_000), CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddDays(1), DateTimeOffset.UtcNow.AddDays(2),
            id: Guid.NewGuid());

    private static Coupon ExpiredPlatformCoupon() =>
        Coupon.CreateByPlatform(
            "admin", "OLD", "Old", DiscountType.FixedAmount,
            null, Money.FromVND(10_000), CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddDays(-2), DateTimeOffset.UtcNow.AddDays(-1),
            id: Guid.NewGuid());

    private static Coupon StoreCoupon(Guid storeId) =>
        Coupon.CreateByStore(
            storeId, Guid.NewGuid(), "STORE", "Store Coupon",
            DiscountType.FixedAmount, null, Money.FromVND(5_000), CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddDays(1),
            id: Guid.NewGuid());

    // ── CouponExpiredSpecification ─────────────────────────────────────────────

    [Fact]
    public void ExpiredSpec_OnExpiredCoupon_ReturnsTrue()
    {
        var spec = new CouponExpiredSpecification();

        spec.IsSatisfiedBy(ExpiredPlatformCoupon()).Should().BeTrue();
    }

    [Fact]
    public void ExpiredSpec_OnDeactivatedCoupon_ReturnsTrue()
    {
        var coupon = OngoingPlatformCoupon();
        coupon.Deactivate();

        new CouponExpiredSpecification().IsSatisfiedBy(coupon).Should().BeTrue();
    }

    [Fact]
    public void ExpiredSpec_OnActiveCoupon_ReturnsFalse()
    {
        new CouponExpiredSpecification().IsSatisfiedBy(OngoingPlatformCoupon()).Should().BeFalse();
    }

    // ── CouponOngoingSpecification ─────────────────────────────────────────────

    [Fact]
    public void OngoingSpec_OnOngoingCoupon_ReturnsTrue()
    {
        new CouponOngoingSpecification().IsSatisfiedBy(OngoingPlatformCoupon()).Should().BeTrue();
    }

    [Fact]
    public void OngoingSpec_OnExpiredCoupon_ReturnsFalse()
    {
        new CouponOngoingSpecification().IsSatisfiedBy(ExpiredPlatformCoupon()).Should().BeFalse();
    }

    [Fact]
    public void OngoingSpec_OnUpcomingCoupon_ReturnsFalse()
    {
        new CouponOngoingSpecification().IsSatisfiedBy(UpcomingPlatformCoupon()).Should().BeFalse();
    }

    // ── CouponUpcomingSpecification ───────────────────────────────────────────

    [Fact]
    public void UpcomingSpec_OnUpcomingCoupon_ReturnsTrue()
    {
        new CouponUpcomingSpecification().IsSatisfiedBy(UpcomingPlatformCoupon()).Should().BeTrue();
    }

    [Fact]
    public void UpcomingSpec_OnOngoingCoupon_ReturnsFalse()
    {
        new CouponUpcomingSpecification().IsSatisfiedBy(OngoingPlatformCoupon()).Should().BeFalse();
    }

    [Fact]
    public void UpcomingSpec_OnInactiveCoupon_ReturnsFalse()
    {
        var coupon = UpcomingPlatformCoupon();
        coupon.Deactivate();

        new CouponUpcomingSpecification().IsSatisfiedBy(coupon).Should().BeFalse();
    }

    // ── CouponOwnedByStoreSpecification ──────────────────────────────────────

    [Fact]
    public void OwnedByStoreSpec_WithMatchingStoreId_ReturnsTrue()
    {
        var storeId = Guid.NewGuid();
        var coupon = StoreCoupon(storeId);

        new CouponOwnedByStoreSpecification(storeId).IsSatisfiedBy(coupon).Should().BeTrue();
    }

    [Fact]
    public void OwnedByStoreSpec_WithDifferentStoreId_ReturnsFalse()
    {
        var coupon = StoreCoupon(Guid.NewGuid());

        new CouponOwnedByStoreSpecification(Guid.NewGuid()).IsSatisfiedBy(coupon).Should().BeFalse();
    }
}
