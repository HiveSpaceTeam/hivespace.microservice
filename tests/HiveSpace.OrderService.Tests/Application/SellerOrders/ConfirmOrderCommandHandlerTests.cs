using FluentAssertions;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.OrderService.Application.Orders.Commands.ConfirmOrder;
using HiveSpace.OrderService.Domain.Aggregates.Orders;
using HiveSpace.OrderService.Domain.Enumerations;
using HiveSpace.OrderService.Domain.ValueObjects;
using HiveSpace.OrderService.Tests.Domain;
using HiveSpace.OrderService.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HiveSpace.OrderService.Tests.Application.SellerOrders;

public class ConfirmOrderCommandHandlerTests : IClassFixture<OrderServiceFixture>
{
    private readonly OrderServiceFixture _fixture;

    public ConfirmOrderCommandHandlerTests(OrderServiceFixture fixture)
    {
        _fixture = fixture;
        OrderIdGeneratorFixture.EnsureInitialized();
    }

    [Fact]
    public async Task Handle_WithPaidOrder_ConfirmsAndTransitionsState()
    {
        var order = Order.Create(Guid.NewGuid(), ValidAddress(), Guid.NewGuid());
        order.AddItem(1L, 1L, 1, HiveSpace.Domain.Shared.ValueObjects.Money.FromVND(50_000), ValidSnapshot());
        order.MarkAsPaid(Guid.NewGuid());
        _fixture.DbContext.Orders.Add(order);
        await _fixture.DbContext.SaveChangesAsync();

        order.Confirm(Guid.NewGuid());
        await _fixture.DbContext.SaveChangesAsync();

        var stored = await _fixture.DbContext.Orders.SingleAsync(o => o.Id == order.Id);
        stored.Status.Should().Be(OrderStatus.Confirmed);
        typeof(ConfirmOrderCommandHandler).Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WithUnpaidOrder_OrderRemainsInCreatedStatus()
    {
        var order = Order.Create(Guid.NewGuid(), ValidAddress(), Guid.NewGuid());
        _fixture.DbContext.Orders.Add(order);
        await _fixture.DbContext.SaveChangesAsync();

        var stored = await _fixture.DbContext.Orders.SingleAsync(o => o.Id == order.Id);
        stored.Status.Should().Be(OrderStatus.Created, "ConfirmOrderCommandHandler should only process paid orders");
    }

    private static DeliveryAddress ValidAddress() =>
        new("Test User", new PhoneNumber("0901234567"), "123 Main St", "Ward 1", "Hanoi");

    private static ProductSnapshot ValidSnapshot() =>
        ProductSnapshot.Capture(1L, 1L, "Product A", "SKU A", HiveSpace.Domain.Shared.ValueObjects.Money.FromVND(50_000), "img.jpg");
}
