using FluentAssertions;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.OrderService.Application.Coupons.Queries.GetCouponList;
using HiveSpace.OrderService.Domain.Aggregates.Coupons;
using HiveSpace.OrderService.Domain.Enumerations;
using HiveSpace.OrderService.Infrastructure.Repositories;
using HiveSpace.OrderService.Tests.Domain;
using HiveSpace.OrderService.Tests.Fixtures;
using HiveSpace.Testing.Shared.Doubles;
using Xunit;

namespace HiveSpace.OrderService.Tests.Application.Coupons;

public class GetCouponListQueryHandlerTests : IClassFixture<OrderServiceFixture>
{
    private readonly OrderServiceFixture _fixture;

    public GetCouponListQueryHandlerTests(OrderServiceFixture fixture)
    {
        _fixture = fixture;
        OrderIdGeneratorFixture.EnsureInitialized();
    }

    [Fact]
    public async Task Handle_AsSellerWithCoupons_ReturnsPaginatedList()
    {
        var storeId = Guid.NewGuid();
        var storeOwnerId = Guid.NewGuid();

        _fixture.DbContext.Coupons.AddRange(
            Coupon.CreateByStore(storeId, storeOwnerId, "LIST01", "Store Coupon 1",
                DiscountType.FixedAmount, null, Money.FromVND(5_000), CouponScope.ItemPrice,
                DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddDays(5)),
            Coupon.CreateByStore(storeId, storeOwnerId, "LIST02", "Store Coupon 2",
                DiscountType.FixedAmount, null, Money.FromVND(8_000), CouponScope.ItemPrice,
                DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddDays(5)));

        await _fixture.DbContext.SaveChangesAsync();

        var handler = new GetCouponListQueryHandler(
            new SqlCouponRepository(_fixture.DbContext),
            new FakeUserContext { UserId = storeOwnerId, Roles = ["Seller"], StoreId = storeId });

        var result = await handler.Handle(
            new GetCouponListQuery(null, null, 1, 20), CancellationToken.None);

        result.Should().NotBeNull();
        result.Coupons.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WithStatusFilter_ReturnsOnlyMatchingCoupons()
    {
        var storeId = Guid.NewGuid();
        var storeOwnerId = Guid.NewGuid();

        _fixture.DbContext.Coupons.AddRange(
            Coupon.CreateByStore(storeId, storeOwnerId, "FILTER01", "Active",
                DiscountType.FixedAmount, null, Money.FromVND(5_000), CouponScope.ItemPrice,
                DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddDays(5)),
            Coupon.CreateByStore(storeId, storeOwnerId, "FILTER02", "Upcoming",
                DiscountType.FixedAmount, null, Money.FromVND(5_000), CouponScope.ItemPrice,
                DateTimeOffset.UtcNow.AddDays(1), DateTimeOffset.UtcNow.AddDays(7)));

        await _fixture.DbContext.SaveChangesAsync();

        var handler = new GetCouponListQueryHandler(
            new SqlCouponRepository(_fixture.DbContext),
            new FakeUserContext { UserId = storeOwnerId, Roles = ["Seller"], StoreId = storeId });

        var result = await handler.Handle(
            new GetCouponListQuery(CouponStatus.Ongoing, null, 1, 20), CancellationToken.None);

        result.Should().NotBeNull();
        result.Coupons.Should().ContainSingle(c => c.Code == "FILTER01");
    }

    [Fact]
    public async Task Handle_WithCodeFilter_ReturnsOnlyMatchingCode()
    {
        var storeId = Guid.NewGuid();
        var storeOwnerId = Guid.NewGuid();

        _fixture.DbContext.Coupons.AddRange(
            Coupon.CreateByStore(storeId, storeOwnerId, "GCQH_CODE1", "Code Match",
                DiscountType.FixedAmount, null, Money.FromVND(5_000), CouponScope.ItemPrice,
                DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddDays(5)),
            Coupon.CreateByStore(storeId, storeOwnerId, "GCQH_OTHER1", "Other",
                DiscountType.FixedAmount, null, Money.FromVND(5_000), CouponScope.ItemPrice,
                DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddDays(5)));
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new GetCouponListQueryHandler(
            new SqlCouponRepository(_fixture.DbContext),
            new FakeUserContext { UserId = storeOwnerId, Roles = ["Seller"], StoreId = storeId });

        var result = await handler.Handle(
            new GetCouponListQuery(null, null, 1, 20, CouponCode: "GCQH_CODE1"), CancellationToken.None);

        result.Coupons.Should().ContainSingle(c => c.Code == "GCQH_CODE1");
    }

    [Fact]
    public async Task Handle_WithExpiredStatus_ReturnsExpiredCoupons()
    {
        var storeId = Guid.NewGuid();
        var storeOwnerId = Guid.NewGuid();

        _fixture.DbContext.Coupons.Add(
            Coupon.CreateByStore(storeId, storeOwnerId, "GCQH_EXP1", "Expired Coupon",
                DiscountType.FixedAmount, null, Money.FromVND(5_000), CouponScope.ItemPrice,
                DateTimeOffset.UtcNow.AddDays(-7), DateTimeOffset.UtcNow.AddMinutes(-1)));
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new GetCouponListQueryHandler(
            new SqlCouponRepository(_fixture.DbContext),
            new FakeUserContext { UserId = storeOwnerId, Roles = ["Seller"], StoreId = storeId });

        var result = await handler.Handle(
            new GetCouponListQuery(CouponStatus.Expired, null, 1, 20), CancellationToken.None);

        result.Coupons.Should().Contain(c => c.Code == "GCQH_EXP1");
    }

    [Fact]
    public async Task Handle_WithUpcomingStatus_ReturnsUpcomingCoupons()
    {
        var storeId = Guid.NewGuid();
        var storeOwnerId = Guid.NewGuid();

        _fixture.DbContext.Coupons.Add(
            Coupon.CreateByStore(storeId, storeOwnerId, "GCQL_UPC1", "Upcoming Coupon",
                DiscountType.FixedAmount, null, Money.FromVND(5_000), CouponScope.ItemPrice,
                DateTimeOffset.UtcNow.AddDays(1), DateTimeOffset.UtcNow.AddDays(7)));
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new GetCouponListQueryHandler(
            new SqlCouponRepository(_fixture.DbContext),
            new FakeUserContext { UserId = storeOwnerId, Roles = ["Seller"], StoreId = storeId });

        var result = await handler.Handle(
            new GetCouponListQuery(CouponStatus.Upcoming, null, 1, 20), CancellationToken.None);

        result.Coupons.Should().Contain(c => c.Code == "GCQL_UPC1");
    }

    [Fact]
    public async Task Handle_WithNameFilter_ReturnsMatchingCoupons()
    {
        var storeId = Guid.NewGuid();
        var storeOwnerId = Guid.NewGuid();

        _fixture.DbContext.Coupons.AddRange(
            Coupon.CreateByStore(storeId, storeOwnerId, "GCQL_NAME1", "Summer Sale",
                DiscountType.FixedAmount, null, Money.FromVND(5_000), CouponScope.ItemPrice,
                DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(7)),
            Coupon.CreateByStore(storeId, storeOwnerId, "GCQL_NAME2", "Winter Deal",
                DiscountType.FixedAmount, null, Money.FromVND(5_000), CouponScope.ItemPrice,
                DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(7)));
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new GetCouponListQueryHandler(
            new SqlCouponRepository(_fixture.DbContext),
            new FakeUserContext { UserId = storeOwnerId, Roles = ["Seller"], StoreId = storeId });

        var result = await handler.Handle(
            new GetCouponListQuery(null, null, 1, 20, CouponName: "Summer"), CancellationToken.None);

        result.Coupons.Should().Contain(c => c.Code == "GCQL_NAME1");
        result.Coupons.Should().NotContain(c => c.Code == "GCQL_NAME2");
    }

    [Fact]
    public async Task Handle_AsPlatformAdmin_ReturnsCouponsWithoutStoreFilter()
    {
        var adminId = Guid.NewGuid();

        _fixture.DbContext.Coupons.Add(
            Coupon.CreateByPlatform(adminId.ToString(), "GCQL_ADMIN1", "Admin Coupon",
                DiscountType.FixedAmount, null, Money.FromVND(5_000), CouponScope.ItemPrice,
                DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(7)));
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new GetCouponListQueryHandler(
            new SqlCouponRepository(_fixture.DbContext),
            new FakeUserContext { UserId = adminId, Roles = ["Admin"] });

        var result = await handler.Handle(
            new GetCouponListQuery(null, null, 1, 20), CancellationToken.None);

        result.Should().NotBeNull();
        result.Coupons.Should().Contain(c => c.Code == "GCQL_ADMIN1");
    }
}
