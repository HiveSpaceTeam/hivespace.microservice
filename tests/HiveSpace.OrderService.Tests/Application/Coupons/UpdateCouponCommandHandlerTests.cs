using FluentAssertions;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.OrderService.Application.Coupons.Commands.UpdateCoupon;
using HiveSpace.OrderService.Domain.Aggregates.Coupons;
using HiveSpace.OrderService.Domain.Enumerations;
using HiveSpace.OrderService.Infrastructure.Repositories;
using HiveSpace.OrderService.Tests.Domain;
using HiveSpace.OrderService.Tests.Fixtures;
using HiveSpace.Testing.Shared.Doubles;
using Xunit;

namespace HiveSpace.OrderService.Tests.Application.Coupons;

public class UpdateCouponCommandHandlerTests : IClassFixture<OrderServiceFixture>
{
    private readonly OrderServiceFixture _fixture;

    public UpdateCouponCommandHandlerTests(OrderServiceFixture fixture)
    {
        _fixture = fixture;
        OrderIdGeneratorFixture.EnsureInitialized();
    }

    [Fact]
    public async Task Handle_WithUpcomingCoupon_UpdatesStoredCoupon()
    {
        var couponId = Guid.NewGuid();
        var futureStart = DateTimeOffset.UtcNow.AddDays(1);
        var coupon = Coupon.CreateByPlatform(
            Guid.NewGuid().ToString(),
            "UPDATE01",
            "Original Name",
            DiscountType.FixedAmount,
            null,
            Money.FromVND(15_000),
            CouponScope.ItemPrice,
            futureStart,
            futureStart.AddDays(7),
            id: couponId);

        _fixture.DbContext.Coupons.Add(coupon);
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new UpdateCouponCommandHandler(
            new SqlCouponRepository(_fixture.DbContext),
            new FakeUserContext { UserId = Guid.NewGuid(), Roles = ["Admin"] });

        var result = await handler.Handle(new UpdateCouponCommand
        {
            Id              = couponId,
            Name            = "Updated Name",
            Code            = "UPDATE01",
            StartDateTime   = futureStart,
            EndDateTime     = futureStart.AddDays(14),
            DiscountCurrency= "VND",
            DiscountAmount  = 15_000,
            MaxUsageCount   = 0,
            ApplicableProductIds = []
        }, CancellationToken.None);

        result.Name.Should().Be("Updated Name");
    }

    [Fact]
    public async Task Handle_AsSellerForOtherStoresCoupon_ThrowsForbiddenException()
    {
        var storeA = Guid.NewGuid();
        var storeB = Guid.NewGuid();
        var futureStart = DateTimeOffset.UtcNow.AddDays(1);

        var coupon = Coupon.CreateByStore(
            storeA, Guid.NewGuid(),
            "UPDATE_FORB1", "Forbidden Update",
            DiscountType.FixedAmount, null, Money.FromVND(5_000),
            CouponScope.ItemPrice,
            futureStart, futureStart.AddDays(7));
        _fixture.DbContext.Coupons.Add(coupon);
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new UpdateCouponCommandHandler(
            new SqlCouponRepository(_fixture.DbContext),
            new FakeUserContext { UserId = Guid.NewGuid(), Roles = ["Seller"], StoreId = storeB });

        var act = () => handler.Handle(new UpdateCouponCommand
        {
            Id              = coupon.Id,
            Name            = "Hacked",
            Code            = "UPDATE_FORB1",
            StartDateTime   = futureStart,
            EndDateTime     = futureStart.AddDays(14),
            DiscountCurrency= "VND",
            DiscountAmount  = 5_000,
            MaxUsageCount   = 0,
            ApplicableProductIds = []
        }, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_WithExpiredCoupon_ThrowsInvalidFieldException()
    {
        var couponId = Guid.NewGuid();
        var coupon = Coupon.CreateByPlatform(
            Guid.NewGuid().ToString(),
            "EXPIRED01",
            "Expired Coupon",
            DiscountType.FixedAmount,
            null,
            Money.FromVND(10_000),
            CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddMinutes(-10),
            DateTimeOffset.UtcNow.AddMinutes(-1),
            id: couponId);

        _fixture.DbContext.Coupons.Add(coupon);
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new UpdateCouponCommandHandler(
            new SqlCouponRepository(_fixture.DbContext),
            new FakeUserContext { UserId = Guid.NewGuid(), Roles = ["Admin"] });

        var act = () => handler.Handle(new UpdateCouponCommand
        {
            Id              = couponId,
            Name            = "New Name",
            Code            = "EXPIRED01",
            StartDateTime   = DateTimeOffset.UtcNow.AddDays(1),
            EndDateTime     = DateTimeOffset.UtcNow.AddDays(2),
            DiscountCurrency= "VND",
            DiscountAmount  = 10_000,
            MaxUsageCount   = 0,
            ApplicableProductIds = []
        }, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidFieldException>();
    }
}
