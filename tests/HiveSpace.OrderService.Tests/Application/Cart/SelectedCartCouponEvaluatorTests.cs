using FluentAssertions;
using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.OrderService.Application.Cart;
using HiveSpace.OrderService.Application.Cart.Dtos;
using HiveSpace.OrderService.Domain.Aggregates.Carts;
using HiveSpace.OrderService.Domain.Aggregates.Coupons;
using HiveSpace.OrderService.Domain.Enumerations;
using HiveSpace.OrderService.Domain.External;
using HiveSpace.OrderService.Tests.Domain;
using Xunit;

namespace HiveSpace.OrderService.Tests.Application.Cart;

public class SelectedCartCouponEvaluatorTests
{
    public SelectedCartCouponEvaluatorTests()
    {
        OrderIdGeneratorFixture.EnsureInitialized();
    }

    // ── BuildStoreSnapshots (CheckoutPreviewRawResult overload) ───────────

    [Fact]
    public void BuildStoreSnapshots_WithRows_ReturnsMappedSnapshots()
    {
        var storeId = Guid.NewGuid();
        var row = MakeRow(storeId, price: 50_000, quantity: 2);
        var result = new CheckoutPreviewRawResult([row], CartExists: true);

        var snapshots = SelectedCartCouponEvaluator.BuildStoreSnapshots(result);

        snapshots.Should().ContainSingle(s => s.StoreId == storeId);
        snapshots[0].Subtotal.Should().Be(100_000);
    }

    [Fact]
    public void BuildStoreSnapshots_WithNullCurrency_DefaultsToVND()
    {
        var row = MakeRow(Guid.NewGuid()) with { Currency = null };
        var result = new CheckoutPreviewRawResult([row], CartExists: true);

        var snapshots = SelectedCartCouponEvaluator.BuildStoreSnapshots(result);

        snapshots[0].Currency.Should().Be("VND");
    }

    [Fact]
    public void BuildStoreSnapshots_WithNullPrice_TreatsAsZero()
    {
        var row = MakeRow(Guid.NewGuid()) with { Price = null };
        var result = new CheckoutPreviewRawResult([row], CartExists: true);

        var snapshots = SelectedCartCouponEvaluator.BuildStoreSnapshots(result);

        snapshots[0].Subtotal.Should().Be(0);
    }

    [Fact]
    public void BuildStoreSnapshots_WithMultipleStores_CreatesOneSnapshotPerStore()
    {
        var storeA = Guid.NewGuid();
        var storeB = Guid.NewGuid();
        var result = new CheckoutPreviewRawResult(
            [MakeRow(storeA, price: 30_000), MakeRow(storeB, price: 70_000)],
            CartExists: true);

        var snapshots = SelectedCartCouponEvaluator.BuildStoreSnapshots(result);

        snapshots.Should().HaveCount(2);
        snapshots.Select(s => s.StoreId).Should().BeEquivalentTo(new[] { storeA, storeB });
    }

    // ── BuildStoreSnapshots (CartItem overload) ────────────────────────────

    [Fact]
    public void BuildStoreSnapshots_CartItemOverload_WithNoSelectedItems_ReturnsEmpty()
    {
        var item = CartItem.Create(1L, 10L, 2);
        item.UpdateSelection(false);

        var result = SelectedCartCouponEvaluator.BuildStoreSnapshots(
            [item],
            new Dictionary<long, ProductRef>(),
            new Dictionary<long, SkuRef>());

        result.Should().BeEmpty();
    }

    [Fact]
    public void BuildStoreSnapshots_CartItemOverload_WithMissingRefs_ReturnsEmpty()
    {
        var item = CartItem.Create(1L, 10L, 2); // IsSelected = true by default

        var result = SelectedCartCouponEvaluator.BuildStoreSnapshots(
            [item],
            new Dictionary<long, ProductRef>(),
            new Dictionary<long, SkuRef>());

        result.Should().BeEmpty();
    }

    [Fact]
    public void BuildStoreSnapshots_CartItemOverload_WithSelectedItems_ReturnsMappedSnapshot()
    {
        var storeId = Guid.NewGuid();
        var item = CartItem.Create(1L, 10L, 2);
        var product = new ProductRef(1L, storeId, "P", null, ProductStatus.Available);
        var sku = new SkuRef(10L, 1L, "SKU-1", 50_000L, "VND", null, null);

        var snapshots = SelectedCartCouponEvaluator.BuildStoreSnapshots(
            [item],
            new Dictionary<long, ProductRef> { [1L] = product },
            new Dictionary<long, SkuRef> { [10L] = sku });

        snapshots.Should().ContainSingle(s => s.StoreId == storeId);
        snapshots[0].Subtotal.Should().Be(100_000); // 50_000 × 2
    }

    [Fact]
    public void BuildStoreSnapshots_CartItemOverload_WithMultipleStores_DistributesShippingFee()
    {
        var storeA = Guid.NewGuid();
        var storeB = Guid.NewGuid();
        var itemA = CartItem.Create(1L, 10L, 1);
        var itemB = CartItem.Create(2L, 20L, 1);
        var products = new Dictionary<long, ProductRef>
        {
            [1L] = new ProductRef(1L, storeA, "PA", null, ProductStatus.Available),
            [2L] = new ProductRef(2L, storeB, "PB", null, ProductStatus.Available),
        };
        var skus = new Dictionary<long, SkuRef>
        {
            [10L] = new SkuRef(10L, 1L, "SKU-A", 40_000L, "VND", null, null),
            [20L] = new SkuRef(20L, 2L, "SKU-B", 60_000L, "VND", null, null),
        };

        var snapshots = SelectedCartCouponEvaluator.BuildStoreSnapshots([itemA, itemB], products, skus);

        snapshots.Should().HaveCount(2);
        snapshots.Sum(s => s.ShippingFee).Should().Be(30_000);
    }

    // ── EnsureSelectedCartExists ───────────────────────────────────────────

    [Fact]
    public void EnsureSelectedCartExists_WhenCartNotExists_ThrowsNotFoundException()
    {
        var result = new CheckoutPreviewRawResult([], CartExists: false);

        var act = () => SelectedCartCouponEvaluator.EnsureSelectedCartExists(result, "test");

        act.Should().Throw<NotFoundException>();
    }

    [Fact]
    public void EnsureSelectedCartExists_WhenCartExistsButNoRows_ThrowsInvalidFieldException()
    {
        var result = new CheckoutPreviewRawResult([], CartExists: true);

        var act = () => SelectedCartCouponEvaluator.EnsureSelectedCartExists(result, "test");

        act.Should().Throw<InvalidFieldException>();
    }

    // ── GetStoreSnapshot ───────────────────────────────────────────────────

    [Fact]
    public void GetStoreSnapshot_WhenStoreNotInResult_ThrowsInvalidFieldException()
    {
        var row = MakeRow(Guid.NewGuid());
        var result = new CheckoutPreviewRawResult([row], CartExists: true);

        var act = () => SelectedCartCouponEvaluator.GetStoreSnapshot(result, Guid.NewGuid(), "test");

        act.Should().Throw<InvalidFieldException>();
    }

    [Fact]
    public void GetStoreSnapshot_WhenStoreExists_ReturnsSnapshot()
    {
        var storeId = Guid.NewGuid();
        var row = MakeRow(storeId, price: 50_000);
        var result = new CheckoutPreviewRawResult([row], CartExists: true);

        var snapshot = SelectedCartCouponEvaluator.GetStoreSnapshot(result, storeId, "test");

        snapshot.StoreId.Should().Be(storeId);
        snapshot.Subtotal.Should().Be(50_000);
    }

    // ── FilterSnapshotByProductIds ─────────────────────────────────────────

    [Fact]
    public void FilterSnapshotByProductIds_WithNullProductIds_ReturnsOriginal()
    {
        var snapshot = MakeSnapshot([1L, 2L]);

        var filtered = SelectedCartCouponEvaluator.FilterSnapshotByProductIds(snapshot, null);

        filtered.ProductIds.Should().BeEquivalentTo(new[] { 1L, 2L });
    }

    [Fact]
    public void FilterSnapshotByProductIds_WithProductIds_FiltersLines()
    {
        var snapshot = MakeSnapshot([1L, 2L]);

        var filtered = SelectedCartCouponEvaluator.FilterSnapshotByProductIds(snapshot, new List<long> { 1L });

        filtered.ProductIds.Should().ContainSingle().Which.Should().Be(1L);
        filtered.Subtotal.Should().Be(50_000);
    }

    // ── EvaluateCoupon ─────────────────────────────────────────────────────

    [Fact]
    public void EvaluateCoupon_WithFixedAmountItemPriceCoupon_ReturnsItemDiscount()
    {
        var coupon = Coupon.CreateByPlatform(
            "admin", "SCEVAL_FIXED1", "Fixed",
            DiscountType.FixedAmount, null, Money.FromVND(10_000),
            CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1));
        var snapshot = MakeSnapshot([1L]);

        var result = SelectedCartCouponEvaluator.EvaluateCoupon(coupon, Guid.NewGuid(), snapshot);

        result.IsApplicable.Should().BeTrue();
        result.ItemDiscount.Should().Be(10_000);
        result.ShippingDiscount.Should().Be(0);
    }

    [Fact]
    public void EvaluateCoupon_WithFixedAmountShippingFeeCoupon_ReturnsShippingDiscount()
    {
        var coupon = Coupon.CreateByPlatform(
            "admin", "SCEVAL_SHIP1", "Shipping",
            DiscountType.FixedAmount, null, Money.FromVND(15_000),
            CouponScope.ShippingFee,
            DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1));
        var snapshot = MakeSnapshot([1L]);

        var result = SelectedCartCouponEvaluator.EvaluateCoupon(coupon, Guid.NewGuid(), snapshot);

        result.IsApplicable.Should().BeTrue();
        result.ItemDiscount.Should().Be(0);
        result.ShippingDiscount.Should().Be(15_000);
    }

    [Fact]
    public void EvaluateCoupon_WithPercentageCoupon_ReturnsProportionalDiscount()
    {
        var coupon = Coupon.CreateByPlatform(
            "admin", "SCEVAL_PCT1", "Percent",
            DiscountType.Percentage, 10m, null,
            CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1));
        var snapshot = MakeSnapshot([1L]);

        var result = SelectedCartCouponEvaluator.EvaluateCoupon(coupon, Guid.NewGuid(), snapshot);

        result.IsApplicable.Should().BeTrue();
        result.ItemDiscount.Should().Be(5_000);
    }

    [Fact]
    public void EvaluateCoupon_WithMinOrderAmountNotMet_ReturnsNotApplicable()
    {
        var coupon = Coupon.CreateByPlatform(
            "admin", "SCEVAL_MIN1", "MinOrder",
            DiscountType.FixedAmount, null, Money.FromVND(5_000),
            CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1),
            minOrderAmount: Money.FromVND(200_000));
        var snapshot = MakeSnapshot([1L]);

        var result = SelectedCartCouponEvaluator.EvaluateCoupon(coupon, Guid.NewGuid(), snapshot);

        result.IsApplicable.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public void EvaluateCoupon_WithApplicableProductIdsNotMatchingSnapshot_ReturnsNotApplicable()
    {
        var coupon = Coupon.CreateByPlatform(
            "admin", "SCEVAL_PROD1", "Prod",
            DiscountType.FixedAmount, null, Money.FromVND(5_000),
            CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1));
        coupon.LimitToProducts([999L]);
        var snapshot = MakeSnapshot([1L]);

        var result = SelectedCartCouponEvaluator.EvaluateCoupon(coupon, Guid.NewGuid(), snapshot);

        result.IsApplicable.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public void EvaluateCoupon_WithApplicableProductIdsMatchingSnapshot_ReturnsApplicable()
    {
        var coupon = Coupon.CreateByPlatform(
            "admin", "SCEVAL_MATCH1", "Match",
            DiscountType.FixedAmount, null, Money.FromVND(5_000),
            CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1));
        coupon.LimitToProducts([1L]);
        var snapshot = MakeSnapshot([1L]);

        var result = SelectedCartCouponEvaluator.EvaluateCoupon(coupon, Guid.NewGuid(), snapshot);

        result.IsApplicable.Should().BeTrue();
    }

    private static CheckoutPreviewRawRow MakeRow(Guid storeId, long price = 50_000, int quantity = 1)
        => new(
            CartItemId: Guid.NewGuid(), ProductId: 1L, SkuId: 10L, Quantity: quantity,
            ProductName: "P", ThumbnailUrl: null,
            Price: price, Currency: "VND",
            SkuName: "S", SkuImageUrl: null, SkuAttributes: null,
            StoreId: storeId, StoreName: "Store");

    private static SelectedCartStoreSnapshot MakeSnapshot(List<long> productIds)
        => new(
            Guid.NewGuid(),
            "Test Store",
            "VND",
            50_000L * productIds.Count,
            30_000L,
            productIds,
            productIds.Select(id => new SelectedCartStoreLineSnapshot(id, 50_000L)).ToList());
}
