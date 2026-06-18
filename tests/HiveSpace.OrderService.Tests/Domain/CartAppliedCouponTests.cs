using FluentAssertions;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.OrderService.Domain.Aggregates.Carts;
using Xunit;

namespace HiveSpace.OrderService.Tests.Domain;

public class CartAppliedCouponTests
{
    // ── CartAppliedPlatformCoupon ─────────────────────────────────────────────

    [Fact]
    public void PlatformCoupon_Create_WithValidCode_NormalizesToUppercase()
    {
        var coupon = CartAppliedPlatformCoupon.Create("save10");

        coupon.CouponCode.Should().Be("SAVE10");
    }

    [Fact]
    public void PlatformCoupon_Create_WithCodeWithSpaces_TrimsAndNormalizes()
    {
        var coupon = CartAppliedPlatformCoupon.Create("  save10  ");

        coupon.CouponCode.Should().Be("SAVE10");
    }

    [Fact]
    public void PlatformCoupon_Create_WithEmptyCode_ThrowsDomainException()
    {
        var act = () => CartAppliedPlatformCoupon.Create("");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void PlatformCoupon_Create_WithWhitespaceOnlyCode_ThrowsDomainException()
    {
        var act = () => CartAppliedPlatformCoupon.Create("   ");

        act.Should().Throw<DomainException>();
    }

    // ── CartAppliedStoreCoupon ────────────────────────────────────────────────

    [Fact]
    public void StoreCoupon_Create_WithValidFields_StoresNormalizedCode()
    {
        var storeId = Guid.NewGuid();

        var coupon = CartAppliedStoreCoupon.Create(storeId, "store5");

        coupon.StoreId.Should().Be(storeId);
        coupon.CouponCode.Should().Be("STORE5");
    }

    [Fact]
    public void StoreCoupon_Create_WithEmptyStoreId_ThrowsDomainException()
    {
        var act = () => CartAppliedStoreCoupon.Create(Guid.Empty, "STORE5");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void StoreCoupon_Create_WithEmptyCode_ThrowsDomainException()
    {
        var act = () => CartAppliedStoreCoupon.Create(Guid.NewGuid(), "");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void StoreCoupon_Create_WithWhitespaceCode_ThrowsDomainException()
    {
        var act = () => CartAppliedStoreCoupon.Create(Guid.NewGuid(), "   ");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void StoreCoupon_UpdateCouponCode_WithValidCode_UpdatesCode()
    {
        var coupon = CartAppliedStoreCoupon.Create(Guid.NewGuid(), "OLD");

        coupon.UpdateCouponCode("new_code");

        coupon.CouponCode.Should().Be("NEW_CODE");
    }

    [Fact]
    public void StoreCoupon_UpdateCouponCode_WithEmptyCode_ThrowsDomainException()
    {
        var coupon = CartAppliedStoreCoupon.Create(Guid.NewGuid(), "OLD");

        var act = () => coupon.UpdateCouponCode("");

        act.Should().Throw<DomainException>();
    }
}
