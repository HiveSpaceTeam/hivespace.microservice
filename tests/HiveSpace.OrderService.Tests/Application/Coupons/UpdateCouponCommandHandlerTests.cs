using FluentAssertions;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.OrderService.Application.Coupons.Commands.UpdateCoupon;
using HiveSpace.OrderService.Domain.Aggregates.Coupons;
using HiveSpace.OrderService.Domain.Enumerations;
using HiveSpace.OrderService.Domain.External;
using HiveSpace.OrderService.Infrastructure.Repositories;
using HiveSpace.OrderService.Tests.Domain;
using HiveSpace.OrderService.Tests.Fixtures;
using HiveSpace.Testing.Shared.Doubles;
using Microsoft.EntityFrameworkCore;
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
        await SeedCurrencyPolicyAsync();
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
            new FakeUserContext { UserId = Guid.NewGuid(), Roles = ["Admin"] },
            new SqlPlatformCurrencyPolicyRefRepository(_fixture.DbContext));

        var result = await handler.Handle(new UpdateCouponCommand
        {
            Id              = couponId,
            Name            = "Updated Name",
            Code            = "UPDATE01",
            StartDateTime   = futureStart,
            EndDateTime     = futureStart.AddDays(14),
            CurrencyCode    = "VND",
            DiscountAmount  = 15_000,
            MaxUsageCount   = 0,
            ApplicableProductIds = []
        }, CancellationToken.None);

        result.Name.Should().Be("Updated Name");
    }

    [Fact]
    public async Task Handle_AsSellerForOtherStoresCoupon_ThrowsForbiddenException()
    {
        await SeedCurrencyPolicyAsync();
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
            new FakeUserContext { UserId = Guid.NewGuid(), Roles = ["Seller"], StoreId = storeB },
            new SqlPlatformCurrencyPolicyRefRepository(_fixture.DbContext));

        var act = () => handler.Handle(new UpdateCouponCommand
        {
            Id              = coupon.Id,
            Name            = "Hacked",
            Code            = "UPDATE_FORB1",
            StartDateTime   = futureStart,
            EndDateTime     = futureStart.AddDays(14),
            CurrencyCode    = "VND",
            DiscountAmount  = 5_000,
            MaxUsageCount   = 0,
            ApplicableProductIds = []
        }, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_WithExpiredCoupon_ThrowsInvalidFieldException()
    {
        await SeedCurrencyPolicyAsync();
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
            new FakeUserContext { UserId = Guid.NewGuid(), Roles = ["Admin"] },
            new SqlPlatformCurrencyPolicyRefRepository(_fixture.DbContext));

        var act = () => handler.Handle(new UpdateCouponCommand
        {
            Id              = couponId,
            Name            = "New Name",
            Code            = "EXPIRED01",
            StartDateTime   = DateTimeOffset.UtcNow.AddDays(1),
            EndDateTime     = DateTimeOffset.UtcNow.AddDays(2),
            CurrencyCode    = "VND",
            DiscountAmount  = 10_000,
            MaxUsageCount   = 0,
            ApplicableProductIds = []
        }, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidFieldException>();
    }

    [Fact]
    public async Task Handle_WithOngoingCouponMetadataOnlyUpdate_DoesNotRequireCurrencyCode()
    {
        await SeedCurrencyPolicyAsync();
        var start = DateTimeOffset.UtcNow.AddMinutes(-10);
        var coupon = Coupon.CreateByPlatform(
            Guid.NewGuid().ToString(),
            "ONGOING01",
            "Original Name",
            DiscountType.FixedAmount,
            null,
            Money.FromVND(10_000),
            CouponScope.ItemPrice,
            start,
            start.AddDays(7));

        _fixture.DbContext.Coupons.Add(coupon);
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new UpdateCouponCommandHandler(
            new SqlCouponRepository(_fixture.DbContext),
            new FakeUserContext { UserId = Guid.NewGuid(), Roles = ["Admin"] },
            new SqlPlatformCurrencyPolicyRefRepository(_fixture.DbContext));

        var result = await handler.Handle(new UpdateCouponCommand
        {
            Id = coupon.Id,
            Name = "Renamed",
            Code = "ONGOING01",
            StartDateTime = start,
            EndDateTime = start.AddDays(10),
            MaxUsageCount = coupon.MaxUsageCount,
            MinOrderAmount = 0,
            ApplicableProductIds = []
        }, CancellationToken.None);

        result.Name.Should().Be("Renamed");
        result.CurrencyCode.Should().Be("VND");
    }

    [Fact]
    public async Task Handle_WithDisabledCurrency_ThrowsInvalidFieldException()
    {
        await SeedCurrencyPolicyAsync("VND");
        var futureStart = DateTimeOffset.UtcNow.AddDays(1);
        var coupon = Coupon.CreateByPlatform(
            Guid.NewGuid().ToString(),
            "DISABLED01",
            "Original Name",
            DiscountType.FixedAmount,
            null,
            Money.FromVND(10_000),
            CouponScope.ItemPrice,
            futureStart,
            futureStart.AddDays(7));

        _fixture.DbContext.Coupons.Add(coupon);
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new UpdateCouponCommandHandler(
            new SqlCouponRepository(_fixture.DbContext),
            new FakeUserContext { UserId = Guid.NewGuid(), Roles = ["Admin"] },
            new SqlPlatformCurrencyPolicyRefRepository(_fixture.DbContext));

        var act = () => handler.Handle(new UpdateCouponCommand
        {
            Id = coupon.Id,
            Name = "Updated Name",
            Code = "DISABLED01",
            StartDateTime = futureStart,
            EndDateTime = futureStart.AddDays(14),
            CurrencyCode = "USD",
            DiscountAmount = 10_000,
            MinOrderAmount = 1,
            MaxUsageCount = coupon.MaxUsageCount,
            ApplicableProductIds = []
        }, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidFieldException>();
    }

    private async Task SeedCurrencyPolicyAsync(params string[] enabledCodes)
    {
        var codes = enabledCodes.Length == 0 ? ["VND"] : enabledCodes;
        var existing = await _fixture.DbContext.PlatformCurrencyPolicyRefs.FirstOrDefaultAsync();
        if (existing is null)
        {
            _fixture.DbContext.PlatformCurrencyPolicyRefs.Add(
                new PlatformCurrencyPolicyRef(Guid.NewGuid(), codes[0], 1, DateTimeOffset.UtcNow, codes));
        }
        else
        {
            existing.Update(codes[0], existing.Version + 1, DateTimeOffset.UtcNow, codes);
        }

        await _fixture.DbContext.SaveChangesAsync();
    }
}
