using FluentAssertions;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.OrderService.Application.Coupons.Commands.DeleteCoupon;
using HiveSpace.OrderService.Domain.Aggregates.Coupons;
using HiveSpace.OrderService.Domain.Enumerations;
using HiveSpace.OrderService.Infrastructure.Repositories;
using HiveSpace.OrderService.Tests.Domain;
using HiveSpace.OrderService.Tests.Fixtures;
using HiveSpace.Testing.Shared.Doubles;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HiveSpace.OrderService.Tests.Application.Coupons;

public class DeleteCouponCommandHandlerTests : IClassFixture<OrderServiceFixture>
{
    private readonly OrderServiceFixture _fixture;

    public DeleteCouponCommandHandlerTests(OrderServiceFixture fixture)
    {
        _fixture = fixture;
        OrderIdGeneratorFixture.EnsureInitialized();
    }

    [Fact]
    public async Task Handle_WithUpcomingCoupon_RemovesCouponFromDb()
    {
        var couponId = Guid.NewGuid();
        var coupon = Coupon.CreateByPlatform(
            Guid.NewGuid().ToString(),
            "DEL01",
            "Delete Me",
            DiscountType.FixedAmount,
            null,
            Money.FromVND(5_000),
            CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddDays(1),
            DateTimeOffset.UtcNow.AddDays(7),
            id: couponId);

        _fixture.DbContext.Coupons.Add(coupon);
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new DeleteCouponCommandHandler(
            new SqlCouponRepository(_fixture.DbContext),
            new FakeUserContext { UserId = Guid.NewGuid(), Roles = ["Admin"] });

        await handler.Handle(new DeleteCouponCommand(couponId), CancellationToken.None);

        var stored = await _fixture.DbContext.Coupons.FirstOrDefaultAsync(c => c.Id == couponId);
        stored.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithNonExistentId_ThrowsNotFoundException()
    {
        var handler = new DeleteCouponCommandHandler(
            new SqlCouponRepository(_fixture.DbContext),
            new FakeUserContext { UserId = Guid.NewGuid(), Roles = ["Admin"] });

        var act = () => handler.Handle(new DeleteCouponCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WithOngoingCoupon_ThrowsInvalidFieldException()
    {
        var couponId = Guid.NewGuid();
        var coupon = Coupon.CreateByPlatform(
            Guid.NewGuid().ToString(), "DCH_ONGOING1", "Ongoing Coupon",
            DiscountType.FixedAmount, null, Money.FromVND(5_000), CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddDays(7),
            id: couponId);
        _fixture.DbContext.Coupons.Add(coupon);
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new DeleteCouponCommandHandler(
            new SqlCouponRepository(_fixture.DbContext),
            new FakeUserContext { UserId = Guid.NewGuid(), Roles = ["Admin"] });

        var act = () => handler.Handle(new DeleteCouponCommand(couponId), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidFieldException>();
    }

    [Fact]
    public async Task Handle_AsSellerDeletingAnotherStoresCoupon_ThrowsForbiddenException()
    {
        var storeId1 = Guid.NewGuid();
        var storeId2 = Guid.NewGuid();
        var couponId = Guid.NewGuid();
        var coupon = Coupon.CreateByStore(
            storeId1, Guid.NewGuid(), "DCH_STOLEN1", "Another Store Coupon",
            DiscountType.FixedAmount, null, Money.FromVND(5_000), CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddDays(1), DateTimeOffset.UtcNow.AddDays(7),
            id: couponId);
        _fixture.DbContext.Coupons.Add(coupon);
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new DeleteCouponCommandHandler(
            new SqlCouponRepository(_fixture.DbContext),
            new FakeUserContext { UserId = Guid.NewGuid(), Roles = ["Seller"], StoreId = storeId2 });

        var act = () => handler.Handle(new DeleteCouponCommand(couponId), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }
}
