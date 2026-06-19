using FluentAssertions;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.OrderService.Application.Orders.Queries.GetOrderById;
using HiveSpace.OrderService.Domain.Aggregates.Orders;
using HiveSpace.OrderService.Domain.ValueObjects;
using HiveSpace.OrderService.Infrastructure.Repositories;
using HiveSpace.OrderService.Tests.Domain;
using HiveSpace.OrderService.Tests.Fixtures;
using HiveSpace.Testing.Shared.Doubles;
using Xunit;

namespace HiveSpace.OrderService.Tests.Application.BuyerOrders;

public class GetOrderByIdQueryHandlerTests : IClassFixture<OrderServiceFixture>
{
    private readonly OrderServiceFixture _fixture;

    public GetOrderByIdQueryHandlerTests(OrderServiceFixture fixture)
    {
        _fixture = fixture;
        OrderIdGeneratorFixture.EnsureInitialized();
    }

    [Fact]
    public async Task Handle_AsOwner_ReturnsOrderDetail()
    {
        var buyerId = Guid.NewGuid();
        var order = Order.Create(buyerId, ValidAddress(), Guid.NewGuid());
        _fixture.DbContext.Orders.Add(order);
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new GetOrderByIdQueryHandler(
            new SqlOrderRepository(_fixture.DbContext),
            new FakeUserContext { UserId = buyerId });

        var result = await handler.Handle(new GetOrderByIdQuery(order.Id), CancellationToken.None);

        result.Should().NotBeNull();
        result.UserId.Should().Be(buyerId);
    }

    [Fact]
    public async Task Handle_AsDifferentUser_ThrowsForbiddenException()
    {
        var buyerId = Guid.NewGuid();
        var order = Order.Create(buyerId, ValidAddress(), Guid.NewGuid());
        _fixture.DbContext.Orders.Add(order);
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new GetOrderByIdQueryHandler(
            new SqlOrderRepository(_fixture.DbContext),
            new FakeUserContext { UserId = Guid.NewGuid() });

        var act = () => handler.Handle(new GetOrderByIdQuery(order.Id), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    private static DeliveryAddress ValidAddress() =>
        new("Test User", new PhoneNumber("0901234567"), "123 Main St", "Ward 1", "Hanoi");
}
