using System.Reflection;
using FluentAssertions;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.OrderService.Domain.Aggregates.Carts;
using HiveSpace.OrderService.Domain.Aggregates.Coupons;
using HiveSpace.OrderService.Domain.Aggregates.Orders;
using HiveSpace.OrderService.Domain.External;
using HiveSpace.OrderService.Domain.ValueObjects;
using Xunit;

namespace HiveSpace.OrderService.Tests.Domain;

/// <summary>
/// Covers private EF Core constructors (required by ORM) and dead-code defensive branches
/// that cannot be reached through the public API. These are exercised via reflection so that
/// the coverage tool can confirm every line is instrumentable and understood.
/// </summary>
public class PrivateConstructorCoverageTests
{
    private static T CreateViaReflection<T>() =>
        (T)Activator.CreateInstance(typeof(T), nonPublic: true)!;

    private static void SetPrivate<T>(object obj, string propertyName, object? value)
    {
        typeof(T)
            .GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
            .GetSetMethod(nonPublic: true)!
            .Invoke(obj, new[] { value });
    }

    // ── Entity / aggregate EF Core constructors ───────────────────────────────

    [Fact]
    public void Cart_PrivateConstructor_CreatesInstance()
    {
        var cart = CreateViaReflection<Cart>();
        cart.Should().NotBeNull();
    }

    [Fact]
    public void Order_PrivateConstructor_CreatesInstance()
    {
        var order = CreateViaReflection<Order>();
        order.Should().NotBeNull();
    }

    [Fact]
    public void CartItem_PrivateConstructor_CreatesInstance()
    {
        var item = CreateViaReflection<CartItem>();
        item.Should().NotBeNull();
    }

    [Fact]
    public void CartAppliedPlatformCoupon_PrivateConstructor_CreatesInstance()
    {
        var coupon = CreateViaReflection<CartAppliedPlatformCoupon>();
        coupon.Should().NotBeNull();
    }

    [Fact]
    public void CartAppliedStoreCoupon_PrivateConstructor_CreatesInstance()
    {
        var coupon = CreateViaReflection<CartAppliedStoreCoupon>();
        coupon.Should().NotBeNull();
    }

    [Fact]
    public void CouponRule_PrivateConstructor_CreatesInstance()
    {
        var rule = CreateViaReflection<CouponRule>();
        rule.Should().NotBeNull();
    }

    [Fact]
    public void CouponUsage_PrivateConstructor_CreatesInstance()
    {
        var usage = CreateViaReflection<CouponUsage>();
        usage.Should().NotBeNull();
    }

    [Fact]
    public void Coupon_PrivateConstructor_CreatesInstance()
    {
        OrderIdGeneratorFixture.EnsureInitialized();
        var coupon = CreateViaReflection<Coupon>();
        coupon.Should().NotBeNull();
    }

    [Fact]
    public void ProductRef_PrivateConstructor_CreatesInstance()
    {
        var ref_ = CreateViaReflection<ProductRef>();
        ref_.Should().NotBeNull();
    }

    [Fact]
    public void SkuRef_PrivateConstructor_CreatesInstance()
    {
        var ref_ = CreateViaReflection<SkuRef>();
        ref_.Should().NotBeNull();
    }

    [Fact]
    public void StoreRef_PrivateConstructor_CreatesInstance()
    {
        var ref_ = CreateViaReflection<StoreRef>();
        ref_.Should().NotBeNull();
    }

    // ── Value object EF Core constructors ─────────────────────────────────────

    [Fact]
    public void PackageDimensions_PrivateConstructor_CreatesInstance()
    {
        var dims = CreateViaReflection<PackageDimensions>();
        dims.Should().NotBeNull();
    }

    [Fact]
    public void DeliveryAddress_PrivateConstructor_CreatesInstance()
    {
        var addr = CreateViaReflection<DeliveryAddress>();
        addr.Should().NotBeNull();
    }

    [Fact]
    public void PhoneNumber_PrivateConstructor_CreatesInstance()
    {
        var phone = CreateViaReflection<PhoneNumber>();
        phone.Should().NotBeNull();
    }

    [Fact]
    public void ProductSnapshot_PrivateConstructor_CreatesInstance()
    {
        var snap = CreateViaReflection<ProductSnapshot>();
        snap.Should().NotBeNull();
    }

    // ── Dead-code defensive branches ──────────────────────────────────────────

    [Fact]
    public void PhoneNumber_GetInternationalFormat_WithNonStandardPrefix_ReturnsPlus84Prefix()
    {
        // Line 78 in PhoneNumber: `return "+84" + Value;`
        // Unreachable through public API (regex enforces +84/84/0 prefix) — exercised via reflection.
        var phone = CreateViaReflection<PhoneNumber>();
        SetPrivate<PhoneNumber>(phone, "Value", "9901234567");

        var result = phone.GetInternationalFormat();

        result.Should().Be("+849901234567");
    }

    [Fact]
    public void Coupon_CalculateDiscount_WhenDiscountExceedsOrderTotal_CapsAtOrderTotal()
    {
        // Line 379 in Coupon.CalculateDiscount: `discountAmount = orderTotal;`
        // Unreachable via normal API (percentage ≤ 100 cannot exceed orderTotal) — exercised via reflection.
        OrderIdGeneratorFixture.EnsureInitialized();
        var coupon = Coupon.CreateByPlatform(
            "admin", "BIG", "Big",
            HiveSpace.OrderService.Domain.Enumerations.DiscountType.Percentage, 10m, null,
            HiveSpace.OrderService.Domain.Enumerations.CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddDays(1),
            id: Guid.NewGuid());
        // Force DiscountPercentage to 200 to make calculated discount (20_000) exceed orderTotal (10_000)
        SetPrivate<Coupon>(coupon, "DiscountPercentage", (decimal?)200m);

        var discount = coupon.CalculateDiscount(Money.FromVND(10_000));

        discount.Amount.Should().Be(10_000);
    }
}
