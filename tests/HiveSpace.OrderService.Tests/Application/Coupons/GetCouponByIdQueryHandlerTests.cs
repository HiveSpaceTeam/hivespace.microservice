using FluentAssertions;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.OrderService.Application.Coupons.Queries.GetCouponById;
using HiveSpace.OrderService.Domain.Aggregates.Coupons;
using HiveSpace.OrderService.Domain.Enumerations;
using HiveSpace.OrderService.Infrastructure.Repositories;
using HiveSpace.OrderService.Tests.Domain;
using HiveSpace.OrderService.Tests.Fixtures;
using HiveSpace.Testing.Shared.Doubles;
using Xunit;

namespace HiveSpace.OrderService.Tests.Application.Coupons;

public class GetCouponByIdQueryHandlerTests : IClassFixture<OrderServiceFixture>
{
    private readonly OrderServiceFixture _fixture;

    public GetCouponByIdQueryHandlerTests(OrderServiceFixture fixture)
    {
        _fixture = fixture;
        OrderIdGeneratorFixture.EnsureInitialized();
    }

    [Fact]
    public async Task Handle_WithValidId_ReturnsCouponDto()
    {
        var couponId = Guid.NewGuid();
        var coupon = Coupon.CreateByPlatform(
            Guid.NewGuid().ToString(),
            "GETBYID01",
            "Get By Id Coupon",
            DiscountType.FixedAmount,
            null,
            Money.FromVND(20_000),
            CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddMinutes(-1),
            DateTimeOffset.UtcNow.AddDays(5),
            id: couponId);

        _fixture.DbContext.Coupons.Add(coupon);
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new GetCouponByIdQueryHandler(
            new SqlCouponRepository(_fixture.DbContext),
            new FakeUserContext { UserId = Guid.NewGuid(), Roles = ["Admin"] });

        var result = await handler.Handle(new GetCouponByIdQuery(couponId), CancellationToken.None);

        result.Should().NotBeNull();
        result.Code.Should().Be("GETBYID01");
        result.Name.Should().Be("Get By Id Coupon");
    }

    [Fact]
    public async Task Handle_WithNonExistentId_ThrowsNotFoundException()
    {
        var handler = new GetCouponByIdQueryHandler(
            new SqlCouponRepository(_fixture.DbContext),
            new FakeUserContext { UserId = Guid.NewGuid(), Roles = ["Admin"] });

        var act = () => handler.Handle(new GetCouponByIdQuery(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_AsSellerForOtherStoresCoupon_ThrowsForbiddenException()
    {
        var storeA = Guid.NewGuid();
        var storeB = Guid.NewGuid();

        var coupon = Coupon.CreateByStore(
            storeA, Guid.NewGuid(),
            "GETBYID_FORB1", "Forbidden",
            DiscountType.FixedAmount, null, Money.FromVND(5_000),
            CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1));
        _fixture.DbContext.Coupons.Add(coupon);
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new GetCouponByIdQueryHandler(
            new SqlCouponRepository(_fixture.DbContext),
            new FakeUserContext { UserId = Guid.NewGuid(), Roles = ["Seller"], StoreId = storeB });

        var act = () => handler.Handle(new GetCouponByIdQuery(coupon.Id), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }
}
