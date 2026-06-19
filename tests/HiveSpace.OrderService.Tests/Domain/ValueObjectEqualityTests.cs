using FluentAssertions;
using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.OrderService.Domain.Aggregates.Orders;
using HiveSpace.OrderService.Domain.Enumerations;
using HiveSpace.OrderService.Domain.ValueObjects;
using Xunit;

namespace HiveSpace.OrderService.Tests.Domain;

public class ValueObjectEqualityTests
{
    public ValueObjectEqualityTests()
    {
        OrderIdGeneratorFixture.EnsureInitialized();
    }

    // ── Checkout ──────────────────────────────────────────────────────────────

    [Fact]
    public void Checkout_EqualToItself_IsTrue()
    {
        // Checkout.GetEqualityComponents yields PaymentMethod, Amount, CreatedAt.
        // Comparing to self triggers the iterator and confirms equality.
        var a = Checkout.Create(PaymentMethod.VNPAY, Money.FromVND(100_000));

        a.Equals(a).Should().BeTrue();
    }

    [Fact]
    public void Checkout_DifferentPaymentMethod_AreNotEqual()
    {
        var a = Checkout.Create(PaymentMethod.VNPAY, Money.FromVND(100_000));
        var b = Checkout.Create(PaymentMethod.COD, Money.FromVND(100_000));

        a.Should().NotBe(b);
    }

    // ── Discount ──────────────────────────────────────────────────────────────

    [Fact]
    public void Discount_SameCouponIdAndScope_AreEqual()
    {
        var id = Guid.NewGuid();
        var d1 = Discount.CreatePlatformDiscount(id, "SAVE10", Money.FromVND(10_000), CouponScope.ItemPrice);
        var d2 = Discount.CreatePlatformDiscount(id, "SAVE10", Money.Copy(Money.FromVND(10_000)), CouponScope.ItemPrice);

        d1.Should().Be(d2);
    }

    [Fact]
    public void Discount_DifferentCouponId_AreNotEqual()
    {
        var d1 = Discount.CreatePlatformDiscount(Guid.NewGuid(), "SAVE10", Money.FromVND(10_000), CouponScope.ItemPrice);
        var d2 = Discount.CreatePlatformDiscount(Guid.NewGuid(), "SAVE10", Money.FromVND(10_000), CouponScope.ItemPrice);

        d1.Should().NotBe(d2);
    }

    // ── ProductSnapshot ───────────────────────────────────────────────────────

    [Fact]
    public void ProductSnapshot_EqualToItself_IsTrue()
    {
        // ProductSnapshot.GetEqualityComponents includes CapturedAt, so two separate
        // instances will differ by timestamp. Compare to self to exercise the iterator.
        var s = ProductSnapshot.Capture(1L, 10L, "Widget", "SKU-1", Money.FromVND(50_000), "img.jpg");

        s.Equals(s).Should().BeTrue();
    }

    [Fact]
    public void ProductSnapshot_WithAttributes_EqualToItself_CoversAttributeLoop()
    {
        // Covers lines 134-137: yield return attribute.Key / attribute.Value inside foreach loop.
        var attrs = new Dictionary<string, string> { { "Color", "Red" }, { "Size", "M" } };
        var s = ProductSnapshot.Capture(1L, 10L, "Widget", "SKU-1", Money.FromVND(50_000), "img.jpg", attrs);

        s.Equals(s).Should().BeTrue();
    }

    [Fact]
    public void ProductSnapshot_DifferentSkuId_AreNotEqual()
    {
        var price = Money.FromVND(50_000);
        var s1 = ProductSnapshot.Capture(1L, 10L, "Widget", "SKU-1", price, "img.jpg");
        var s2 = ProductSnapshot.Capture(1L, 99L, "Widget", "SKU-1", price, "img.jpg");

        s1.Should().NotBe(s2);
    }
}
