using FluentAssertions;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.OrderService.Application.Cart.Queries.GetCheckoutPreview;
using HiveSpace.OrderService.Domain.Aggregates.Coupons;
using HiveSpace.OrderService.Domain.Enumerations;
using HiveSpace.OrderService.Infrastructure.Repositories;
using HiveSpace.OrderService.Tests.Domain;
using HiveSpace.OrderService.Tests.Fixtures;
using HiveSpace.Testing.Shared.Doubles;
using Xunit;
using CartAggregate = HiveSpace.OrderService.Domain.Aggregates.Carts.Cart;

namespace HiveSpace.OrderService.Tests.Application.Cart;

public class GetCheckoutPreviewQueryHandlerTests : IClassFixture<OrderServiceFixture>
{
    private readonly OrderServiceFixture _fixture;

    public GetCheckoutPreviewQueryHandlerTests(OrderServiceFixture fixture)
    {
        _fixture = fixture;
        OrderIdGeneratorFixture.EnsureInitialized();
    }

    [Fact]
    public async Task Handle_WithNoCart_ThrowsNotFoundException()
    {
        var storeId = Guid.NewGuid();
        var handler = new GetCheckoutPreviewQueryHandler(
            new FakeCheckoutQuery(FakeCheckoutQuery.MakeRow(storeId)),
            new SqlCouponRepository(_fixture.DbContext),
            new SqlCartRepository(_fixture.DbContext),
            new FakeUserContext { UserId = Guid.NewGuid() });

        var act = () => handler.Handle(
            new GetCheckoutPreviewQuery(), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WithCartAndSelectedItems_ReturnsPreviewWithPackage()
    {
        var userId  = Guid.NewGuid();
        var storeId = Guid.NewGuid();

        var cart = CartAggregate.Create(userId);
        _fixture.DbContext.Carts.Add(cart);
        await _fixture.DbContext.SaveChangesAsync();

        var row = FakeCheckoutQuery.MakeRow(storeId, quantity: 1, price: 50_000);
        var handler = new GetCheckoutPreviewQueryHandler(
            new FakeCheckoutQuery(row),
            new SqlCouponRepository(_fixture.DbContext),
            new SqlCartRepository(_fixture.DbContext),
            new FakeUserContext { UserId = userId });

        var result = await handler.Handle(
            new GetCheckoutPreviewQuery(), CancellationToken.None);

        result.Should().NotBeNull();
        result.Packages.Should().ContainSingle();
        result.GrandTotal.Should().BePositive();
    }

    [Fact]
    public async Task Handle_WithValidPlatformCouponApplied_ReducesGrandTotal()
    {
        var userId  = Guid.NewGuid();
        var storeId = Guid.NewGuid();

        var coupon = Coupon.CreateByPlatform(
            adminId: "admin",
            code: "PREVIEW_PLAT1",
            name: "Preview Platform",
            discountType: DiscountType.FixedAmount,
            percentage: null,
            discountAmount: Money.FromVND(5_000),
            scope: CouponScope.ItemPrice,
            startDateTime: DateTimeOffset.UtcNow.AddDays(-1),
            endDateTime: DateTimeOffset.UtcNow.AddDays(1));
        _fixture.DbContext.Coupons.Add(coupon);

        var cart = CartAggregate.Create(userId);
        cart.ApplyPlatformCoupon("PREVIEW_PLAT1");
        _fixture.DbContext.Carts.Add(cart);
        await _fixture.DbContext.SaveChangesAsync();

        var row = FakeCheckoutQuery.MakeRow(storeId, quantity: 1, price: 50_000);
        var handler = new GetCheckoutPreviewQueryHandler(
            new FakeCheckoutQuery(row),
            new SqlCouponRepository(_fixture.DbContext),
            new SqlCartRepository(_fixture.DbContext),
            new FakeUserContext { UserId = userId });

        var result = await handler.Handle(
            new GetCheckoutPreviewQuery(), CancellationToken.None);

        result.Should().NotBeNull();
        result.GrandTotal.Should().Be(75_000);
    }

    [Fact]
    public async Task Handle_WithExpiredCouponApplied_InvalidatesCouponAndSavesCart()
    {
        var userId  = Guid.NewGuid();
        var storeId = Guid.NewGuid();

        // minOrderAmount > snapshot subtotal → Validate() fails → coupon invalidated
        var coupon = Coupon.CreateByPlatform(
            adminId: "admin",
            code: "PREVIEW_EXP1",
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
        cart.ApplyPlatformCoupon("PREVIEW_EXP1");
        _fixture.DbContext.Carts.Add(cart);
        await _fixture.DbContext.SaveChangesAsync();

        var row = FakeCheckoutQuery.MakeRow(storeId, quantity: 1, price: 50_000);
        var handler = new GetCheckoutPreviewQueryHandler(
            new FakeCheckoutQuery(row),
            new SqlCouponRepository(_fixture.DbContext),
            new SqlCartRepository(_fixture.DbContext),
            new FakeUserContext { UserId = userId });

        var result = await handler.Handle(new GetCheckoutPreviewQuery(), CancellationToken.None);

        result.InvalidatedCoupons.Should().ContainSingle(c => c.CouponCode == "PREVIEW_EXP1");
    }

    [Fact]
    public async Task Handle_WithValidStoreCouponApplied_ReducesPackageSubtotal()
    {
        var userId  = Guid.NewGuid();
        var storeId = Guid.NewGuid();

        var coupon = Coupon.CreateByStore(
            storeId: storeId,
            storeOwnerId: Guid.NewGuid(),
            code: "PREVIEW_STORE1",
            name: "Store Discount",
            discountType: DiscountType.FixedAmount,
            percentage: null,
            discountAmount: Money.FromVND(5_000),
            scope: CouponScope.ItemPrice,
            startDateTime: DateTimeOffset.UtcNow.AddDays(-1),
            endDateTime: DateTimeOffset.UtcNow.AddDays(1));
        _fixture.DbContext.Coupons.Add(coupon);

        var cart = CartAggregate.Create(userId);
        cart.ApplyStoreCoupon(storeId, "PREVIEW_STORE1");
        _fixture.DbContext.Carts.Add(cart);
        await _fixture.DbContext.SaveChangesAsync();

        var row = FakeCheckoutQuery.MakeRow(storeId, quantity: 1, price: 50_000);
        var handler = new GetCheckoutPreviewQueryHandler(
            new FakeCheckoutQuery(row),
            new SqlCouponRepository(_fixture.DbContext),
            new SqlCartRepository(_fixture.DbContext),
            new FakeUserContext { UserId = userId });

        var result = await handler.Handle(new GetCheckoutPreviewQuery(), CancellationToken.None);

        result.InvalidatedCoupons.Should().BeEmpty();
        result.Packages.Should().ContainSingle()
            .Which.AppliedStoreCoupon.Should().NotBeNull();
        result.Packages[0].Subtotal.Should().BeLessThan(result.Packages[0].OriginalSubtotal);
    }
}
