using FluentAssertions;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.OrderService.Application.Coupons.Queries.GetAvailableCoupons;
using HiveSpace.OrderService.Domain.Aggregates.Coupons;
using HiveSpace.OrderService.Domain.Enumerations;
using HiveSpace.OrderService.Infrastructure.Repositories;
using HiveSpace.OrderService.Tests.Domain;
using HiveSpace.OrderService.Tests.Fixtures;
using HiveSpace.Testing.Shared.Doubles;
using Xunit;

namespace HiveSpace.OrderService.Tests.Application.Coupons;

public class GetAvailableCouponsQueryHandlerTests : IClassFixture<OrderServiceFixture>
{
    private readonly OrderServiceFixture _fixture;

    public GetAvailableCouponsQueryHandlerTests(OrderServiceFixture fixture)
    {
        _fixture = fixture;
        OrderIdGeneratorFixture.EnsureInitialized();
    }

    [Fact]
    public async Task Handle_WithOngoingStoreCoupons_ReturnsCouponList()
    {
        var storeId = Guid.NewGuid();
        var userId  = Guid.NewGuid();

        _fixture.DbContext.Coupons.Add(
            Coupon.CreateByStore(storeId, Guid.NewGuid(), "AVAIL01", "Available Coupon",
                DiscountType.FixedAmount, null, Money.FromVND(5_000), CouponScope.ItemPrice,
                DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddDays(7)));

        await _fixture.DbContext.SaveChangesAsync();

        var handler = new GetAvailableCouponsQueryHandler(
            new FakeCheckoutQuery(FakeCheckoutQuery.MakeRow(storeId)),
            new SqlCouponRepository(_fixture.DbContext),
            new SqlStoreRefRepository(_fixture.DbContext),
            new FakeUserContext { UserId = userId });

        var result = await handler.Handle(
            new GetAvailableCouponsQuery(storeId), CancellationToken.None);

        result.Should().NotBeNull();
        result.Coupons.Should().ContainSingle(c => c.Code == "AVAIL01");
    }

    [Fact]
    public async Task Handle_WithNoStoreActiveCoupons_ReturnsEmptyList()
    {
        var storeId = Guid.NewGuid();
        var userId  = Guid.NewGuid();

        var handler = new GetAvailableCouponsQueryHandler(
            new FakeCheckoutQuery(FakeCheckoutQuery.MakeRow(storeId)),
            new SqlCouponRepository(_fixture.DbContext),
            new SqlStoreRefRepository(_fixture.DbContext),
            new FakeUserContext { UserId = userId });

        var result = await handler.Handle(
            new GetAvailableCouponsQuery(storeId), CancellationToken.None);

        result.Should().NotBeNull();
        result.Coupons.Should().BeEmpty();
    }
}
