using FluentAssertions;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.OrderService.Domain.Aggregates.Orders;
using HiveSpace.OrderService.Domain.Enumerations;
using Xunit;

namespace HiveSpace.OrderService.Tests.Domain;

public class DiscountTests
{
    [Fact]
    public void CreatePlatformDiscount_WithPositiveAmount_StoresAmount()
    {
        var couponId = Guid.NewGuid();
        var discount = Discount.CreatePlatformDiscount(couponId, "SAVE10", Money.FromVND(10_000), CouponScope.ItemPrice);

        discount.DiscountAmount.Amount.Should().Be(10_000L);
        discount.CouponOwnerType.Should().Be(CouponOwnerType.Platform);
    }

    [Fact]
    public void CreatePlatformDiscount_WithZeroAmount_ThrowsDomainException()
    {
        // Money.FromVND(0) is valid (zero is not negative), but Discount guards against amount <= 0
        var act = () => Discount.CreatePlatformDiscount(Guid.NewGuid(), "ZERO", Money.FromVND(0), CouponScope.ItemPrice);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void CreatePlatformDiscount_WithEmptyCode_ThrowsDomainException()
    {
        var act = () => Discount.CreatePlatformDiscount(Guid.NewGuid(), "", Money.FromVND(10_000), CouponScope.ItemPrice);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Apply_FixedType_ReturnsExactAmount()
    {
        OrderIdGeneratorFixture.EnsureInitialized();
        var coupon = HiveSpace.OrderService.Domain.Aggregates.Coupons.Coupon.CreateByPlatform(
            Guid.NewGuid().ToString(), "FIX50", "Fixed50",
            DiscountType.FixedAmount, null, Money.FromVND(50_000),
            CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddDays(1),
            id: Guid.NewGuid());

        var discount = coupon.CalculateDiscount(Money.FromVND(200_000));

        discount.Amount.Should().Be(50_000L);
    }

    [Fact]
    public void Apply_PercentageType_CapsAtMaxDiscountAmount()
    {
        OrderIdGeneratorFixture.EnsureInitialized();
        var coupon = HiveSpace.OrderService.Domain.Aggregates.Coupons.Coupon.CreateByPlatform(
            Guid.NewGuid().ToString(), "PCT5", "5Percent",
            DiscountType.Percentage, 5m, null,
            CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddDays(1),
            maxDiscountAmount: Money.FromVND(3_000),
            id: Guid.NewGuid());

        var discount = coupon.CalculateDiscount(Money.FromVND(200_000));

        discount.Amount.Should().BeLessOrEqualTo(3_000L, "percentage discount must not exceed MaxDiscountAmount");
    }

    [Fact]
    public void CreateStoreDiscount_WithPositiveAmount_StoresAmount()
    {
        var couponId = Guid.NewGuid();
        var discount = Discount.CreateStoreDiscount(couponId, "STORE5", Money.FromVND(5_000), CouponScope.ShippingFee);

        discount.DiscountAmount.Amount.Should().Be(5_000L);
        discount.CouponOwnerType.Should().Be(CouponOwnerType.Store);
        discount.Scope.Should().Be(CouponScope.ShippingFee);
    }

    [Fact]
    public void CreateStoreDiscount_WithZeroAmount_ThrowsDomainException()
    {
        var act = () => Discount.CreateStoreDiscount(Guid.NewGuid(), "STORE5", Money.FromVND(0), CouponScope.ItemPrice);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void CreateStoreDiscount_WithEmptyCode_ThrowsDomainException()
    {
        var act = () => Discount.CreateStoreDiscount(Guid.NewGuid(), "", Money.FromVND(5_000), CouponScope.ItemPrice);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void CreateStoreDiscount_WithEmptyId_ThrowsDomainException()
    {
        var act = () => Discount.CreateStoreDiscount(Guid.Empty, "STORE5", Money.FromVND(5_000), CouponScope.ItemPrice);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void CreatePlatformDiscount_WithEmptyId_ThrowsDomainException()
    {
        var act = () => Discount.CreatePlatformDiscount(Guid.Empty, "SAVE10", Money.FromVND(10_000), CouponScope.ItemPrice);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void CreatePlatformDiscount_WithInvalidScope_ThrowsDomainException()
    {
        var act = () => Discount.CreatePlatformDiscount(Guid.NewGuid(), "SAVE10", Money.FromVND(10_000), (CouponScope)0);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void CreateStoreDiscount_WithInvalidScope_ThrowsDomainException()
    {
        var act = () => Discount.CreateStoreDiscount(Guid.NewGuid(), "STORE5", Money.FromVND(5_000), (CouponScope)0);

        act.Should().Throw<DomainException>();
    }
}
