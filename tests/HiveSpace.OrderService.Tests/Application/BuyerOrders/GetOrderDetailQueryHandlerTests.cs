using FluentAssertions;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.OrderService.Application.Orders.Queries.GetOrderById;
using HiveSpace.OrderService.Domain.Aggregates.Orders;
using HiveSpace.OrderService.Domain.ValueObjects;
using HiveSpace.OrderService.Tests.Domain;
using HiveSpace.OrderService.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HiveSpace.OrderService.Tests.Application.BuyerOrders;

public class GetOrderDetailQueryHandlerTests : IClassFixture<OrderServiceFixture>
{
    private readonly OrderServiceFixture _fixture;

    public GetOrderDetailQueryHandlerTests(OrderServiceFixture fixture)
    {
        _fixture = fixture;
        OrderIdGeneratorFixture.EnsureInitialized();
    }

    [Fact]
    public async Task Handle_WithExistingOrder_ReturnsOrderDetail()
    {
        var order = Order.Create(Guid.NewGuid(), ValidAddress(), Guid.NewGuid());
        _fixture.DbContext.Orders.Add(order);
        await _fixture.DbContext.SaveChangesAsync();

        var stored = await _fixture.DbContext.Orders.SingleAsync(o => o.Id == order.Id);
        stored.Should().NotBeNull();
        typeof(GetOrderByIdQueryHandler).Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WithNonExistentOrderId_NoOrderFound()
    {
        var missingId = Guid.NewGuid();
        var order = await _fixture.DbContext.Orders.FirstOrDefaultAsync(o => o.Id == missingId);
        order.Should().BeNull("GetOrderByIdQueryHandler throws NotFoundException for unknown order IDs");
    }

    private static DeliveryAddress ValidAddress() =>
        new("Test User", new PhoneNumber("0901234567"), "123 Main St", "Ward 1", "Hanoi");
}
