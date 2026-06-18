using FluentAssertions;
using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.OrderService.Domain.Aggregates.Orders;
using HiveSpace.OrderService.Domain.Enumerations;
using Xunit;

namespace HiveSpace.OrderService.Tests.Domain;

public class CheckoutTests
{
    [Fact]
    public void Create_WithValidPaymentMethodAndAmount_StoresCorrectly()
    {
        var amount = Money.FromVND(150_000);
        var checkout = Checkout.Create(PaymentMethod.VNPAY, amount);

        checkout.Amount.Amount.Should().Be(150_000L);
        checkout.PaymentMethod.Should().Be(PaymentMethod.VNPAY);
    }

    [Fact]
    public void CreatePlatformDiscount_StoresAmountAndOwnerType()
    {
        var discountId = Guid.NewGuid();
        var discount = Discount.CreatePlatformDiscount(discountId, "SAVE10", Money.FromVND(10_000), CouponScope.ItemPrice);

        discount.DiscountAmount.Amount.Should().Be(10_000L);
        discount.CouponOwnerType.Should().Be(CouponOwnerType.Platform);
    }

    [Fact]
    public void CreateStoreDiscount_StoresAmountScopeAndOwnerType()
    {
        var discountId = Guid.NewGuid();
        var discount = Discount.CreateStoreDiscount(discountId, "STORE5", Money.FromVND(5_000), CouponScope.ShippingFee);

        discount.DiscountAmount.Amount.Should().Be(5_000L);
        discount.CouponOwnerType.Should().Be(CouponOwnerType.Store);
        discount.Scope.Should().Be(CouponScope.ShippingFee);
    }

    [Fact]
    public void Checkout_WithExpiredCoupon_ValidationFails()
    {
        OrderIdGeneratorFixture.EnsureInitialized();
        var coupon = HiveSpace.OrderService.Domain.Aggregates.Coupons.Coupon.CreateByPlatform(
            Guid.NewGuid().ToString(), "OLD", "Expired",
            HiveSpace.OrderService.Domain.Enumerations.DiscountType.FixedAmount, null,
            Money.FromVND(10_000), CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddDays(-2), DateTimeOffset.UtcNow.AddDays(-1),
            id: Guid.NewGuid());

        var result = coupon.Validate(Guid.NewGuid(), Money.FromVND(50_000));

        result.IsValid.Should().BeFalse("checkout with an expired coupon must be rejected");
    }
}
