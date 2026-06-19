using FluentAssertions;
using HiveSpace.Core.Models.Pagination;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.OrderService.Application.Cart;
using HiveSpace.OrderService.Application.Cart.Dtos;
using HiveSpace.OrderService.Application.Cart.Queries.GetCartSummary;
using HiveSpace.OrderService.Domain.Aggregates.Coupons;
using HiveSpace.OrderService.Domain.Enumerations;
using HiveSpace.OrderService.Infrastructure.Repositories;
using HiveSpace.OrderService.Tests.Domain;
using HiveSpace.OrderService.Tests.Fixtures;
using HiveSpace.Testing.Shared.Doubles;
using Xunit;
using CartAggregate = HiveSpace.OrderService.Domain.Aggregates.Carts.Cart;

namespace HiveSpace.OrderService.Tests.Application.Cart;

public class GetCartSummaryQueryHandlerTests : IClassFixture<OrderServiceFixture>
{
    private readonly OrderServiceFixture _fixture;

    public GetCartSummaryQueryHandlerTests(OrderServiceFixture fixture)
    {
        _fixture = fixture;
        OrderIdGeneratorFixture.EnsureInitialized();
    }

    [Fact]
    public async Task Handle_WithExistingCart_ReturnsResponse()
    {
        var userId = Guid.NewGuid();
        var cart = CartAggregate.Create(userId);
        cart.AddItem(1L, 10L, 2);
        _fixture.DbContext.Carts.Add(cart);
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new GetCartSummaryQueryHandler(
            new FakeCartDataQuery(),
            new FakeCheckoutQuery(),
            new SqlCouponRepository(_fixture.DbContext),
            new SqlCartRepository(_fixture.DbContext),
            new FakeUserContext { UserId = userId });

        var result = await handler.Handle(
            new GetCartSummaryQuery(1, 20), CancellationToken.None);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WithNoCart_ReturnsEmptyResponse()
    {
        var handler = new GetCartSummaryQueryHandler(
            new FakeCartDataQuery(),
            new FakeCheckoutQuery(),
            new SqlCouponRepository(_fixture.DbContext),
            new SqlCartRepository(_fixture.DbContext),
            new FakeUserContext { UserId = Guid.NewGuid() });

        var result = await handler.Handle(
            new GetCartSummaryQuery(1, 20), CancellationToken.None);

        result.Should().NotBeNull();
        result.Stores.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithCartItemsAndCheckoutRows_ReturnsPopulatedStores()
    {
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var cart = CartAggregate.Create(userId);
        cart.AddItem(1L, 10L, 1);
        _fixture.DbContext.Carts.Add(cart);
        await _fixture.DbContext.SaveChangesAsync();

        var cartItemId = cart.Items.First().Id;
        var row = FakeCheckoutQuery.MakeRow(storeId, productId: 1L, skuId: 10L, quantity: 1, price: 50_000);

        var handler = new GetCartSummaryQueryHandler(
            new FakeCartDataQueryWithItems(
                new CartItemDto(cartItemId, 1L, 10L, 1, true,
                    "Product", null, null,
                    50_000, 50_000, "VND", null, "SKU", null, null,
                    storeId, "Store", null,
                    DateTimeOffset.UtcNow, null)),
            new FakeCheckoutQuery(row),
            new SqlCouponRepository(_fixture.DbContext),
            new SqlCartRepository(_fixture.DbContext),
            new FakeUserContext { UserId = userId });

        var result = await handler.Handle(new GetCartSummaryQuery(1, 20), CancellationToken.None);

        result.Should().NotBeNull();
        result.Stores.Should().ContainSingle(s => s.StoreId == storeId);
    }

    [Fact]
    public async Task Handle_WithCartItemsButNoSelectedItems_ReturnsOriginalPrices()
    {
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var cartItemId = Guid.NewGuid();

        var cart = CartAggregate.Create(userId);
        _fixture.DbContext.Carts.Add(cart);
        await _fixture.DbContext.SaveChangesAsync();

        // FakeCheckoutQuery with no rows → CartExists=false → snapshots=[] → snapshotsByStore empty
        // Item storeId has no snapshot → enters the "no snapshot" path in ApplyItemDiscounts
        var handler = new GetCartSummaryQueryHandler(
            new FakeCartDataQueryWithItems(
                new CartItemDto(cartItemId, 1L, 10L, 1, false,
                    "P", null, null, 50_000, 50_000, "VND", null, "S", null, null,
                    storeId, "Store", null, DateTimeOffset.UtcNow, null)),
            new FakeCheckoutQuery(),
            new SqlCouponRepository(_fixture.DbContext),
            new SqlCartRepository(_fixture.DbContext),
            new FakeUserContext { UserId = userId });

        var result = await handler.Handle(new GetCartSummaryQuery(1, 20), CancellationToken.None);

        result.Should().NotBeNull();
        result.Stores.Should().ContainSingle(s => s.StoreId == storeId);
    }

    [Fact]
    public async Task Handle_WithExpiredCouponApplied_InvalidatesCouponInResult()
    {
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();

        // minOrderAmount > subtotal → Validate() fails → coupon invalidated → SaveChangesAsync called
        var coupon = Coupon.CreateByPlatform(
            adminId: "admin",
            code: "GCSUM_EXP1",
            name: "High Min Order",
            discountType: DiscountType.FixedAmount,
            percentage: null,
            discountAmount: Money.FromVND(5_000),
            scope: CouponScope.ItemPrice,
            startDateTime: DateTimeOffset.UtcNow.AddDays(-1),
            endDateTime: DateTimeOffset.UtcNow.AddDays(1),
            minOrderAmount: Money.FromVND(999_999));
        _fixture.DbContext.Coupons.Add(coupon);

        var cart = CartAggregate.Create(userId);
        cart.ApplyPlatformCoupon("GCSUM_EXP1");
        _fixture.DbContext.Carts.Add(cart);
        await _fixture.DbContext.SaveChangesAsync();

        var row = FakeCheckoutQuery.MakeRow(storeId, productId: 1L, skuId: 10L, price: 50_000);
        var handler = new GetCartSummaryQueryHandler(
            new FakeCartDataQuery(),
            new FakeCheckoutQuery(row),
            new SqlCouponRepository(_fixture.DbContext),
            new SqlCartRepository(_fixture.DbContext),
            new FakeUserContext { UserId = userId });

        var result = await handler.Handle(new GetCartSummaryQuery(1, 20), CancellationToken.None);

        result.InvalidatedCoupons.Should().ContainSingle(c => c.CouponCode == "GCSUM_EXP1");
    }

    [Fact]
    public async Task Handle_WithValidStoreCouponApplied_AppliesStoreItemDiscount()
    {
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();

        var coupon = Coupon.CreateByStore(
            storeId: storeId,
            storeOwnerId: Guid.NewGuid(),
            code: "GCSUM_STORE1",
            name: "Store Discount",
            discountType: DiscountType.FixedAmount,
            percentage: null,
            discountAmount: Money.FromVND(5_000),
            scope: CouponScope.ItemPrice,
            startDateTime: DateTimeOffset.UtcNow.AddDays(-1),
            endDateTime: DateTimeOffset.UtcNow.AddDays(1));
        _fixture.DbContext.Coupons.Add(coupon);

        var cart = CartAggregate.Create(userId);
        cart.ApplyStoreCoupon(storeId, "GCSUM_STORE1");
        _fixture.DbContext.Carts.Add(cart);
        await _fixture.DbContext.SaveChangesAsync();

        // Use row.CartItemId as the CartItemDto.CartItemId so selectedItemIds.Contains matches
        var row = FakeCheckoutQuery.MakeRow(storeId, productId: 1L, skuId: 10L, quantity: 1, price: 50_000);
        var handler = new GetCartSummaryQueryHandler(
            new FakeCartDataQueryWithItems(
                new CartItemDto(row.CartItemId, 1L, 10L, 1, true,
                    "P", null, null, 50_000, 50_000, "VND", null, "S", null, null,
                    storeId, "Store", null, DateTimeOffset.UtcNow, null)),
            new FakeCheckoutQuery(row),
            new SqlCouponRepository(_fixture.DbContext),
            new SqlCartRepository(_fixture.DbContext),
            new FakeUserContext { UserId = userId });

        var result = await handler.Handle(new GetCartSummaryQuery(1, 20), CancellationToken.None);

        result.Summary.DiscountAmount.Should().Be(5_000);
        result.Stores.Should().ContainSingle()
            .Which.AppliedStoreCoupon.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WithValidPlatformCouponApplied_CalculatesPlatformDiscount()
    {
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var cartItemId = Guid.NewGuid();

        var coupon = Coupon.CreateByPlatform(
            adminId: "admin",
            code: "GCSUM_PLAT1",
            name: "Platform Discount",
            discountType: DiscountType.FixedAmount,
            percentage: null,
            discountAmount: Money.FromVND(5_000),
            scope: CouponScope.ItemPrice,
            startDateTime: DateTimeOffset.UtcNow.AddDays(-1),
            endDateTime: DateTimeOffset.UtcNow.AddDays(1));
        _fixture.DbContext.Coupons.Add(coupon);

        var cart = CartAggregate.Create(userId);
        cart.ApplyPlatformCoupon("GCSUM_PLAT1");
        _fixture.DbContext.Carts.Add(cart);
        await _fixture.DbContext.SaveChangesAsync();

        var row = new CheckoutPreviewRawRow(
            CartItemId: cartItemId, ProductId: 1L, SkuId: 10L, Quantity: 1,
            ProductName: "P", ThumbnailUrl: null, Price: 50_000, Currency: "VND",
            SkuName: "S", SkuImageUrl: null, SkuAttributes: null,
            StoreId: storeId, StoreName: "Store");

        var handler = new GetCartSummaryQueryHandler(
            new FakeCartDataQueryWithItems(
                new CartItemDto(cartItemId, 1L, 10L, 1, true,
                    "Product", null, null,
                    50_000, 50_000, "VND", null, "SKU", null, null,
                    storeId, "Store", null,
                    DateTimeOffset.UtcNow, null)),
            new FakeCheckoutQuery(row),
            new SqlCouponRepository(_fixture.DbContext),
            new SqlCartRepository(_fixture.DbContext),
            new FakeUserContext { UserId = userId });

        var result = await handler.Handle(new GetCartSummaryQuery(1, 20), CancellationToken.None);

        result.Should().NotBeNull();
        result.Summary.DiscountAmount.Should().Be(5_000);
    }

    private sealed class FakeCartDataQueryWithItems(params CartItemDto[] items) : ICartDataQuery
    {
        public Task<PagedResult<CartItemDto>> GetPagedCartItemsAsync(
            Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
            => Task.FromResult(new PagedResult<CartItemDto>([.. items], page, pageSize, items.Length));
    }
}
