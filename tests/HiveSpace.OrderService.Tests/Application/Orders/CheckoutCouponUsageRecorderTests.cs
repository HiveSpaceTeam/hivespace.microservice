using FluentAssertions;
using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.OrderService.Application.Orders;
using HiveSpace.OrderService.Domain.Aggregates.Coupons;
using HiveSpace.OrderService.Domain.Enumerations;
using HiveSpace.OrderService.Domain.Repositories;
using HiveSpace.OrderService.Infrastructure.Repositories;
using HiveSpace.OrderService.Tests.Domain;
using HiveSpace.OrderService.Tests.Fixtures;
using Xunit;

namespace HiveSpace.OrderService.Tests.Application.Orders;

public class CheckoutCouponUsageRecorderTests : IClassFixture<OrderServiceFixture>
{
    private readonly OrderServiceFixture _fixture;

    public CheckoutCouponUsageRecorderTests(OrderServiceFixture fixture)
    {
        _fixture = fixture;
        OrderIdGeneratorFixture.EnsureInitialized();
    }

    [Fact]
    public async Task CommitAsync_WithEmptyUsages_CompletesWithoutException()
    {
        await CheckoutCouponUsageRecorder.CommitAsync(
            [], new SqlCouponRepository(_fixture.DbContext), CancellationToken.None);
    }

    [Fact]
    public async Task CommitAsync_WithExistingCoupon_MarksCouponAsUsed()
    {
        var coupon = Coupon.CreateByStore(
            Guid.NewGuid(), Guid.NewGuid(), "CCUR_COMMIT1", "Commit Happy",
            DiscountType.FixedAmount, null, Money.FromVND(5_000), CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddDays(7));
        _fixture.DbContext.Coupons.Add(coupon);
        await _fixture.DbContext.SaveChangesAsync();

        var usage = new OrderCouponUsageEntry(Guid.NewGuid(), Guid.NewGuid(), "CCUR_COMMIT1", 5_000, Currency.VND);

        var act = () => CheckoutCouponUsageRecorder.CommitAsync(
            [usage], new SqlCouponRepository(_fixture.DbContext), CancellationToken.None);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task CommitAsync_WithMissingCouponCode_ThrowsNotFoundException()
    {
        var usage = new OrderCouponUsageEntry(Guid.NewGuid(), Guid.NewGuid(), "CCUR_MISSING_C1", 5_000, Currency.VND);

        var act = () => CheckoutCouponUsageRecorder.CommitAsync(
            [usage], new SqlCouponRepository(_fixture.DbContext), CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ReleaseAsync_WithEmptyUsages_CompletesWithoutException()
    {
        await CheckoutCouponUsageRecorder.ReleaseAsync(
            [], new SqlCouponRepository(_fixture.DbContext), CancellationToken.None);
    }

    [Fact]
    public async Task ReleaseAsync_WithExistingCoupon_ReleasesUsage()
    {
        var coupon = Coupon.CreateByStore(
            Guid.NewGuid(), Guid.NewGuid(), "CCUR_REL1", "Release Happy",
            DiscountType.FixedAmount, null, Money.FromVND(5_000), CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddDays(7));
        _fixture.DbContext.Coupons.Add(coupon);
        await _fixture.DbContext.SaveChangesAsync();

        var usage = new OrderCouponUsageEntry(Guid.NewGuid(), Guid.NewGuid(), "CCUR_REL1", 5_000, Currency.VND);

        var act = () => CheckoutCouponUsageRecorder.ReleaseAsync(
            [usage], new SqlCouponRepository(_fixture.DbContext), CancellationToken.None);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ReleaseAsync_WithMissingCouponCode_ThrowsNotFoundException()
    {
        var usage = new OrderCouponUsageEntry(Guid.NewGuid(), Guid.NewGuid(), "CCUR_MISSING_R1", 5_000, Currency.VND);

        var act = () => CheckoutCouponUsageRecorder.ReleaseAsync(
            [usage], new SqlCouponRepository(_fixture.DbContext), CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
