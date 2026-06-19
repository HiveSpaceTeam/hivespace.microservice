using FluentAssertions;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.OrderService.Application.Cart;
using HiveSpace.OrderService.Domain.Aggregates.Coupons;
using HiveSpace.OrderService.Domain.Enumerations;
using HiveSpace.OrderService.Tests.Domain;
using Xunit;

namespace HiveSpace.OrderService.Tests.Application.Checkout;

public class CheckoutCalculatorTests
{
    public CheckoutCalculatorTests()
    {
        OrderIdGeneratorFixture.EnsureInitialized();
    }

    [Fact]
    public void CalculateShippingFee_WithFewItems_ReturnsBaseFee()
    {
        var fee = CheckoutCalculator.CalculateShippingFee(3);

        fee.Should().Be(30_000L);
    }

    [Fact]
    public void CalculateShippingFee_ReturnsExpectedFeeForWeight()
    {
        var fee = CheckoutCalculator.CalculateShippingFee(6);

        fee.Should().Be(50_000L, "orders over 5 items use the higher shipping tier");
    }

    [Fact]
    public void CalculatePlatformDiscount_WithFixedCoupon_DeductsFixed()
    {
        var coupon = Coupon.CreateByPlatform(
            Guid.NewGuid().ToString(),
            "FIXED10K",
            "Fixed 10k",
            DiscountType.FixedAmount,
            null,
            Money.FromVND(10_000),
            CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddMinutes(-1),
            DateTimeOffset.UtcNow.AddDays(1),
            id: Guid.NewGuid());

        var (itemDiscount, shippingDiscount) = CheckoutCalculator.ApplyCoupon(
            coupon,
            Guid.NewGuid(),
            subtotal: 100_000L,
            shippingFee: 30_000L);

        itemDiscount.Should().Be(10_000L);
        shippingDiscount.Should().Be(0L);
    }

    [Fact]
    public void DistributeShippingFee_AcrossStores_EvenSplit()
    {
        var fees = CheckoutCalculator.DistributeShippingFee(30_000L, 2);

        fees.Should().HaveCount(2);
        fees.Sum().Should().Be(30_000L, "distributed fees must sum to the original total");
    }

    [Fact]
    public void CalculateTotal_SubtotalPlusShippingMinusDiscounts()
    {
        var shippingFee = CheckoutCalculator.CalculateShippingFee(3);
        var coupon = Coupon.CreateByPlatform(
            Guid.NewGuid().ToString(),
            "TOTAL10K",
            "Total 10k",
            DiscountType.FixedAmount,
            null,
            Money.FromVND(10_000),
            CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddMinutes(-1),
            DateTimeOffset.UtcNow.AddDays(1),
            id: Guid.NewGuid());

        var (itemDiscount, _) = CheckoutCalculator.ApplyCoupon(
            coupon,
            Guid.NewGuid(),
            subtotal: 100_000L,
            shippingFee: shippingFee);

        var grandTotal = 100_000L + shippingFee - itemDiscount;
        grandTotal.Should().Be(120_000L);
    }
}
