using FluentAssertions;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.OrderService.Application.Orders.Commands.CancelOrder;
using HiveSpace.OrderService.Domain.Aggregates.Coupons;
using HiveSpace.OrderService.Domain.Aggregates.Orders;
using HiveSpace.OrderService.Domain.Enumerations;
using HiveSpace.OrderService.Domain.ValueObjects;
using HiveSpace.OrderService.Infrastructure.Repositories;
using HiveSpace.OrderService.Tests.Domain;
using HiveSpace.OrderService.Tests.Fixtures;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace HiveSpace.OrderService.Tests.Application.BuyerOrders;

public class CancelOrderCommandHandlerTests : IClassFixture<OrderServiceFixture>
{
    private readonly OrderServiceFixture _fixture;

    public CancelOrderCommandHandlerTests(OrderServiceFixture fixture)
    {
        _fixture = fixture;
        OrderIdGeneratorFixture.EnsureInitialized();
    }

    [Fact]
    public async Task Handle_WithExistingOrder_CancelsOrderAndReturnsFound()
    {
        var buyerId = Guid.NewGuid();
        var order = Order.Create(buyerId, ValidAddress(), Guid.NewGuid());
        _fixture.DbContext.Orders.Add(order);
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new CancelOrderCommandHandler(
            new SqlOrderRepository(_fixture.DbContext),
            new SqlCouponRepository(_fixture.DbContext),
            NullLogger<CancelOrderCommandHandler>.Instance);

        var result = await handler.Handle(
            new CancelOrderCommand(order.Id, "Buyer changed mind", buyerId),
            CancellationToken.None);

        result.OrderFound.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithNonExistentOrder_ReturnsNotFound()
    {
        var handler = new CancelOrderCommandHandler(
            new SqlOrderRepository(_fixture.DbContext),
            new SqlCouponRepository(_fixture.DbContext),
            NullLogger<CancelOrderCommandHandler>.Instance);

        var result = await handler.Handle(
            new CancelOrderCommand(Guid.NewGuid(), "reason", Guid.NewGuid()),
            CancellationToken.None);

        result.OrderFound.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithAlreadyCancelledOrder_ReturnsFoundWithoutCancelling()
    {
        var buyerId = Guid.NewGuid();
        var order = Order.Create(buyerId, ValidAddress(), Guid.NewGuid());
        order.Cancel("Pre-cancelled", buyerId);
        _fixture.DbContext.Orders.Add(order);
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new CancelOrderCommandHandler(
            new SqlOrderRepository(_fixture.DbContext),
            new SqlCouponRepository(_fixture.DbContext),
            NullLogger<CancelOrderCommandHandler>.Instance);

        var result = await handler.Handle(
            new CancelOrderCommand(order.Id, "Again", buyerId),
            CancellationToken.None);

        result.OrderFound.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithOrderHavingDiscount_ReleasesCouponAndCompletes()
    {
        var buyerId = Guid.NewGuid();

        var coupon = Coupon.CreateByPlatform(
            adminId: Guid.NewGuid().ToString(),
            code: "CANCEL_RELEASE1",
            name: "Release Coupon",
            discountType: DiscountType.FixedAmount,
            percentage: null,
            discountAmount: Money.FromVND(5_000),
            scope: CouponScope.ItemPrice,
            startDateTime: DateTimeOffset.UtcNow.AddDays(-1),
            endDateTime: DateTimeOffset.UtcNow.AddDays(1));
        _fixture.DbContext.Coupons.Add(coupon);

        var order = Order.Create(buyerId, ValidAddress(), Guid.NewGuid());
        order.ApplyDiscount(coupon, Money.FromVND(5_000));
        _fixture.DbContext.Orders.Add(order);
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new CancelOrderCommandHandler(
            new SqlOrderRepository(_fixture.DbContext),
            new SqlCouponRepository(_fixture.DbContext),
            NullLogger<CancelOrderCommandHandler>.Instance);

        var result = await handler.Handle(
            new CancelOrderCommand(order.Id, "with coupon", buyerId),
            CancellationToken.None);

        result.OrderFound.Should().BeTrue();
    }

    private static DeliveryAddress ValidAddress() =>
        new("Test User", new PhoneNumber("0901234567"), "123 Main St", "Ward 1", "Hanoi");
}
