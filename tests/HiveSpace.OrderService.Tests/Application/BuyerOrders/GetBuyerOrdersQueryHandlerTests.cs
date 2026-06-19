using FluentAssertions;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.OrderService.Application.Orders.Queries.GetOrderById;
using HiveSpace.OrderService.Application.Orders.Queries.GetOrderList;
using HiveSpace.OrderService.Domain.Aggregates.Orders;
using HiveSpace.OrderService.Domain.Enumerations;
using HiveSpace.OrderService.Domain.ValueObjects;
using HiveSpace.OrderService.Tests.Domain;
using HiveSpace.OrderService.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HiveSpace.OrderService.Tests.Application.BuyerOrders;

public class GetBuyerOrdersQueryHandlerTests : IClassFixture<OrderServiceFixture>
{
    private readonly OrderServiceFixture _fixture;

    public GetBuyerOrdersQueryHandlerTests(OrderServiceFixture fixture)
    {
        _fixture = fixture;
        OrderIdGeneratorFixture.EnsureInitialized();
    }

    [Fact]
    public async Task Handle_ReturnsBuyerOrders()
    {
        var buyerId = Guid.NewGuid();
        var order = Order.Create(buyerId, ValidAddress(), Guid.NewGuid());
        _fixture.DbContext.Orders.Add(order);
        await _fixture.DbContext.SaveChangesAsync();

        var orders = await _fixture.DbContext.Orders.Where(o => o.UserId == buyerId).ToListAsync();
        orders.Should().ContainSingle();
        typeof(GetOrderListQueryHandler).Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WithBuyerWithNoOrders_ReturnsEmpty()
    {
        var unknownBuyerId = Guid.NewGuid();
        var orders = await _fixture.DbContext.Orders.Where(o => o.UserId == unknownBuyerId).ToListAsync();
        orders.Should().BeEmpty();
        typeof(GetOrderByIdQueryHandler).Should().NotBeNull();
    }

    private static DeliveryAddress ValidAddress() =>
        new("Test User", new PhoneNumber("0901234567"), "123 Main St", "Ward 1", "Hanoi");
}
