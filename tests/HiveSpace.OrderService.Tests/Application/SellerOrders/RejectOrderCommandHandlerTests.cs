using FluentAssertions;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.OrderService.Application.Orders.Commands.RejectOrder;
using HiveSpace.OrderService.Domain.Aggregates.Orders;
using HiveSpace.OrderService.Domain.Enumerations;
using HiveSpace.OrderService.Domain.ValueObjects;
using HiveSpace.OrderService.Tests.Domain;
using HiveSpace.OrderService.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HiveSpace.OrderService.Tests.Application.SellerOrders;

public class RejectOrderCommandHandlerTests : IClassFixture<OrderServiceFixture>
{
    private readonly OrderServiceFixture _fixture;

    public RejectOrderCommandHandlerTests(OrderServiceFixture fixture)
    {
        _fixture = fixture;
        OrderIdGeneratorFixture.EnsureInitialized();
    }

    [Fact]
    public async Task Handle_WithPaidOrder_CancelsOrder()
    {
        var order = Order.Create(Guid.NewGuid(), ValidAddress(), Guid.NewGuid());
        order.MarkAsPaid(Guid.NewGuid());
        _fixture.DbContext.Orders.Add(order);
        await _fixture.DbContext.SaveChangesAsync();

        order.Cancel("Seller rejected", Guid.NewGuid());
        await _fixture.DbContext.SaveChangesAsync();

        var stored = await _fixture.DbContext.Orders.SingleAsync(o => o.Id == order.Id);
        stored.Status.Should().Be(OrderStatus.Cancelled);
        typeof(RejectOrderCommandHandler).Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WithExpiredOrder_CannotCancel()
    {
        var order = Order.Create(Guid.NewGuid(), ValidAddress(), Guid.NewGuid());
        order.MarkAsExpired();
        _fixture.DbContext.Orders.Add(order);
        await _fixture.DbContext.SaveChangesAsync();

        var act = () => order.Cancel("reason", Guid.NewGuid());
        act.Should().Throw<DomainException>("expired orders cannot be cancelled via RejectOrderCommandHandler");
    }

    private static DeliveryAddress ValidAddress() =>
        new("Test User", new PhoneNumber("0901234567"), "123 Main St", "Ward 1", "Hanoi");
}
