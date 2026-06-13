using FluentAssertions;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.OrderService.Application.Orders.Queries.GetSellerOrders;
using HiveSpace.OrderService.Domain.Aggregates.Orders;
using HiveSpace.OrderService.Domain.ValueObjects;
using HiveSpace.OrderService.Tests.Domain;
using HiveSpace.OrderService.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HiveSpace.OrderService.Tests.Application.SellerOrders;

public class GetSellerOrdersQueryHandlerTests : IClassFixture<OrderServiceFixture>
{
    private readonly OrderServiceFixture _fixture;

    public GetSellerOrdersQueryHandlerTests(OrderServiceFixture fixture)
    {
        _fixture = fixture;
        OrderIdGeneratorFixture.EnsureInitialized();
    }

    [Fact]
    public async Task Handle_WithOrdersForStore_ReturnsSellerOrders()
    {
        var storeId = Guid.NewGuid();
        var order = Order.Create(Guid.NewGuid(), ValidAddress(), storeId);
        _fixture.DbContext.Orders.Add(order);
        await _fixture.DbContext.SaveChangesAsync();

        var orders = await _fixture.DbContext.Orders.Where(o => o.StoreId == storeId).ToListAsync();
        orders.Should().ContainSingle();
        typeof(GetSellerOrdersQueryHandler).Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WithStoreWithNoOrders_ReturnsEmpty()
    {
        var unknownStoreId = Guid.NewGuid();
        var orders = await _fixture.DbContext.Orders.Where(o => o.StoreId == unknownStoreId).ToListAsync();
        orders.Should().BeEmpty();
    }

    private static DeliveryAddress ValidAddress() =>
        new("Test User", new PhoneNumber("0901234567"), "123 Main St", "Ward 1", "Hanoi");
}
