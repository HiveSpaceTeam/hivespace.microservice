using FluentAssertions;
using HiveSpace.Core.Exceptions;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.OrderService.Application.Cart;
using HiveSpace.OrderService.Application.Cart.Dtos;
using HiveSpace.OrderService.Domain.Aggregates.Coupons;
using HiveSpace.OrderService.Domain.Enumerations;
using HiveSpace.OrderService.Domain.Exceptions;
using HiveSpace.OrderService.Infrastructure.Repositories;
using HiveSpace.OrderService.Tests.Domain;
using HiveSpace.OrderService.Tests.Fixtures;
using Xunit;
using CartAggregate = HiveSpace.OrderService.Domain.Aggregates.Carts.Cart;

namespace HiveSpace.OrderService.Tests.Application.Cart;

public class PersistedCartCouponStateTests : IClassFixture<OrderServiceFixture>
{
    private readonly OrderServiceFixture _fixture;

    public PersistedCartCouponStateTests(OrderServiceFixture fixture)
    {
        _fixture = fixture;
        OrderIdGeneratorFixture.EnsureInitialized();
    }

    [Fact]
    public async Task RemoveInvalidStoreCoupons_WithNoAppliedCoupons_DoesNotModifyCart()
    {
        var cart = CartAggregate.Create(Guid.NewGuid());

        await PersistedCartCouponState.RemoveInvalidStoreCouponsAsync(
            cart, [], new SqlCouponRepository(_fixture.DbContext),
            Guid.NewGuid(), CancellationToken.None);

        cart.AppliedStoreCoupons.Should().BeEmpty();
    }

    [Fact]
    public async Task RemoveInvalidStoreCoupons_WithUnknownCoupon_RemovesCouponFromCart()
    {
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var cart = CartAggregate.Create(userId);
        cart.ApplyStoreCoupon(storeId, "PCS_UNKNOWN1");

        // No matching snapshot → invalid → removed
        await PersistedCartCouponState.RemoveInvalidStoreCouponsAsync(
            cart, [], new SqlCouponRepository(_fixture.DbContext),
            userId, CancellationToken.None);

        cart.AppliedStoreCoupons.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateAsync_WithNoCoupons_ReturnsEmptyResult()
    {
        var cart = CartAggregate.Create(Guid.NewGuid());

        var result = await PersistedCartCouponState.ValidateAsync(
            cart, [], new SqlCouponRepository(_fixture.DbContext),
            Guid.NewGuid(), CancellationToken.None,
            removeInvalidSelections: false);

        result.AppliedPlatformCoupons.Should().BeEmpty();
        result.AppliedStoreCoupons.Should().BeEmpty();
        result.InvalidatedCoupons.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateAsync_WithUnknownPlatformCoupon_MovesToInvalidated()
    {
        var userId = Guid.NewGuid();
        var cart = CartAggregate.Create(userId);
        cart.ApplyPlatformCoupon("PCS_PLATUNK1");

        var result = await PersistedCartCouponState.ValidateAsync(
            cart, [], new SqlCouponRepository(_fixture.DbContext),
            userId, CancellationToken.None,
            removeInvalidSelections: false);

        result.AppliedPlatformCoupons.Should().BeEmpty();
        result.InvalidatedCoupons.Should().ContainSingle(c => c.CouponCode == "PCS_PLATUNK1");
    }

    [Fact]
    public async Task ValidateAsync_WithRemoveInvalidSelectionsTrue_RemovesBadCouponsFromCart()
    {
        var userId = Guid.NewGuid();
        var cart = CartAggregate.Create(userId);
        cart.ApplyPlatformCoupon("PCS_PLATREM1");

        await PersistedCartCouponState.ValidateAsync(
            cart, [], new SqlCouponRepository(_fixture.DbContext),
            userId, CancellationToken.None,
            removeInvalidSelections: true);

        cart.AppliedPlatformCoupons.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateAsync_WithUnknownStoreCoupon_MovesToInvalidated()
    {
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var cart = CartAggregate.Create(userId);
        cart.ApplyStoreCoupon(storeId, "PCS_STOREUNK1");

        var result = await PersistedCartCouponState.ValidateAsync(
            cart, [], new SqlCouponRepository(_fixture.DbContext),
            userId, CancellationToken.None,
            removeInvalidSelections: false);

        result.AppliedStoreCoupons.Should().BeEmpty();
        result.InvalidatedCoupons.Should().ContainSingle(c => c.CouponCode == "PCS_STOREUNK1");
    }

    [Fact]
    public async Task ValidateAsync_WithStoreCouponCodeInPlatformSlot_MovesToInvalidated()
    {
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();

        var storeCoupon = Coupon.CreateByStore(
            storeId,
            userId,
            "PCS_WRONGOWNER1",
            "Wrong Owner",
            DiscountType.FixedAmount,
            null,
            Money.FromVND(5_000),
            CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow.AddDays(1));
        _fixture.DbContext.Coupons.Add(storeCoupon);
        await _fixture.DbContext.SaveChangesAsync();

        var cart = CartAggregate.Create(userId);
        cart.ApplyPlatformCoupon("PCS_WRONGOWNER1");

        var result = await PersistedCartCouponState.ValidateAsync(
            cart, [], new SqlCouponRepository(_fixture.DbContext),
            userId, CancellationToken.None,
            removeInvalidSelections: false);

        result.AppliedPlatformCoupons.Should().BeEmpty();
        result.InvalidatedCoupons.Should().ContainSingle(c => c.CouponCode == "PCS_WRONGOWNER1");
    }

    [Fact]
    public async Task ValidateAsync_WithStoreCouponBelongingToDifferentStore_MovesToInvalidated()
    {
        var userId = Guid.NewGuid();
        var storeA = Guid.NewGuid();
        var storeB = Guid.NewGuid();

        var coupon = Coupon.CreateByStore(
            storeA, Guid.NewGuid(),
            "PCS_WRONGSTORE1", "Wrong Store",
            DiscountType.FixedAmount, null, Money.FromVND(5_000),
            CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1));
        _fixture.DbContext.Coupons.Add(coupon);
        await _fixture.DbContext.SaveChangesAsync();

        var cart = CartAggregate.Create(userId);
        cart.ApplyStoreCoupon(storeB, "PCS_WRONGSTORE1");

        var snapshot = new SelectedCartStoreSnapshot(
            storeB, "Store B", "VND", 50_000, 30_000,
            [1L], [new SelectedCartStoreLineSnapshot(1L, 50_000)]);

        var result = await PersistedCartCouponState.ValidateAsync(
            cart, [snapshot], new SqlCouponRepository(_fixture.DbContext),
            userId, CancellationToken.None,
            removeInvalidSelections: false);

        result.AppliedStoreCoupons.Should().BeEmpty();
        result.InvalidatedCoupons.Should().ContainSingle(c => c.CouponCode == "PCS_WRONGSTORE1");
    }

    [Fact]
    public async Task ValidateAsync_WithExpiredPlatformCoupon_MovesToInvalidated()
    {
        var userId = Guid.NewGuid();

        var coupon = Coupon.CreateByPlatform(
            adminId: "admin",
            code: "PCS_EXPIRED1",
            name: "High Min Order",
            discountType: DiscountType.FixedAmount,
            percentage: null,
            discountAmount: Money.FromVND(5_000),
            scope: CouponScope.ItemPrice,
            startDateTime: DateTimeOffset.UtcNow.AddDays(-1),
            endDateTime: DateTimeOffset.UtcNow.AddDays(1),
            minOrderAmount: Money.FromVND(999_999));
        _fixture.DbContext.Coupons.Add(coupon);
        await _fixture.DbContext.SaveChangesAsync();

        var cart = CartAggregate.Create(userId);
        cart.ApplyPlatformCoupon("PCS_EXPIRED1");

        // No snapshots → grandSubtotal = 0 < 999_999 → Validate() fails
        var result = await PersistedCartCouponState.ValidateAsync(
            cart, [], new SqlCouponRepository(_fixture.DbContext),
            userId, CancellationToken.None,
            removeInvalidSelections: false);

        result.AppliedPlatformCoupons.Should().BeEmpty();
        result.InvalidatedCoupons.Should().ContainSingle(c => c.CouponCode == "PCS_EXPIRED1");
    }

    [Fact]
    public async Task ValidateAsync_WithValidStoreCoupon_ReturnsAppliedStoreCoupon()
    {
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();

        var coupon = Coupon.CreateByStore(
            storeId: storeId,
            storeOwnerId: Guid.NewGuid(),
            code: "PCS_STOREOK1",
            name: "Store OK",
            discountType: DiscountType.FixedAmount,
            percentage: null,
            discountAmount: Money.FromVND(5_000),
            scope: CouponScope.ItemPrice,
            startDateTime: DateTimeOffset.UtcNow.AddDays(-1),
            endDateTime: DateTimeOffset.UtcNow.AddDays(1));
        _fixture.DbContext.Coupons.Add(coupon);
        await _fixture.DbContext.SaveChangesAsync();

        var cart = CartAggregate.Create(userId);
        cart.ApplyStoreCoupon(storeId, "PCS_STOREOK1");

        var snapshot = new SelectedCartStoreSnapshot(
            storeId, "Store", "VND", 50_000, 30_000,
            [1L], [new SelectedCartStoreLineSnapshot(1L, 50_000)]);

        var result = await PersistedCartCouponState.ValidateAsync(
            cart, [snapshot], new SqlCouponRepository(_fixture.DbContext),
            userId, CancellationToken.None,
            removeInvalidSelections: false);

        result.AppliedStoreCoupons.Should().ContainKey(storeId);
        result.InvalidatedCoupons.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateAsync_WithIneligibleStoreCoupon_MovesToInvalidated()
    {
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();

        var coupon = Coupon.CreateByStore(
            storeId: storeId,
            storeOwnerId: Guid.NewGuid(),
            code: "PCS_MINORDER1",
            name: "Min Order Too High",
            discountType: DiscountType.FixedAmount,
            percentage: null,
            discountAmount: Money.FromVND(5_000),
            scope: CouponScope.ItemPrice,
            startDateTime: DateTimeOffset.UtcNow.AddDays(-1),
            endDateTime: DateTimeOffset.UtcNow.AddDays(1),
            minOrderAmount: Money.FromVND(999_999));
        _fixture.DbContext.Coupons.Add(coupon);
        await _fixture.DbContext.SaveChangesAsync();

        var cart = CartAggregate.Create(userId);
        cart.ApplyStoreCoupon(storeId, "PCS_MINORDER1");

        // Snapshot subtotal 50_000 < minOrderAmount 999_999 → EvaluateCoupon returns IsApplicable=false
        var snapshot = new SelectedCartStoreSnapshot(
            storeId, "Store", "VND", 50_000, 30_000,
            [1L], [new SelectedCartStoreLineSnapshot(1L, 50_000)]);

        var result = await PersistedCartCouponState.ValidateAsync(
            cart, [snapshot], new SqlCouponRepository(_fixture.DbContext),
            userId, CancellationToken.None,
            removeInvalidSelections: false);

        result.AppliedStoreCoupons.Should().BeEmpty();
        result.InvalidatedCoupons.Should().ContainSingle(c => c.CouponCode == "PCS_MINORDER1");
    }

    [Fact]
    public async Task ValidateAsync_WithInvalidStoreCoupon_AndRemoveTrue_RemovesFromCart()
    {
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var cart = CartAggregate.Create(userId);
        cart.ApplyStoreCoupon(storeId, "PCS_REMOVE_STORE1");

        // No snapshot → store coupon invalid → removeInvalidSelections removes it from cart
        await PersistedCartCouponState.ValidateAsync(
            cart, [], new SqlCouponRepository(_fixture.DbContext),
            userId, CancellationToken.None,
            removeInvalidSelections: true);

        cart.AppliedStoreCoupons.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateAsync_WithSnapshotButUnknownStoreCouponCode_MovesToInvalidated()
    {
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var cart = CartAggregate.Create(userId);
        cart.ApplyStoreCoupon(storeId, "PCS_SNAP_UNKNOWN1");  // code not in DB

        var snapshot = new SelectedCartStoreSnapshot(
            storeId, "Store", "VND", 50_000, 30_000,
            [1L], [new SelectedCartStoreLineSnapshot(1L, 50_000)]);

        // Snapshot present → TryValidateStoreCoupon skips null-snapshot branch
        // couponsByCode.TryGetValue("PCS_SNAP_UNKNOWN1") → false → lines 182-184 covered
        var result = await PersistedCartCouponState.ValidateAsync(
            cart, [snapshot], new SqlCouponRepository(_fixture.DbContext),
            userId, CancellationToken.None,
            removeInvalidSelections: false);

        result.AppliedStoreCoupons.Should().BeEmpty();
        result.InvalidatedCoupons.Should().ContainSingle(c => c.CouponCode == "PCS_SNAP_UNKNOWN1");
    }

    [Fact]
    public void BuildCheckoutCouponException_WithInvalidCoupons_ReturnsBadRequestException()
    {
        var invalid = PersistedCartCouponState.BuildInvalidCoupon(
            "PCS_BAD01", CouponOwnerType.Platform, null, OrderDomainErrorCode.CouponNotFound);

        var ex = PersistedCartCouponState.BuildCheckoutCouponException([invalid]);

        ex.Should().BeOfType<BadRequestException>();
    }
}
