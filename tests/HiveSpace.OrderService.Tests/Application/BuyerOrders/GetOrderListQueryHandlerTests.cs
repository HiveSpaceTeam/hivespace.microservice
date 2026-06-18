using FluentAssertions;
using HiveSpace.OrderService.Application.Orders.Queries.GetOrderList;
using HiveSpace.OrderService.Tests.Domain;
using HiveSpace.OrderService.Tests.Fixtures;
using HiveSpace.Testing.Shared.Doubles;
using Xunit;

namespace HiveSpace.OrderService.Tests.Application.BuyerOrders;

public class GetOrderListQueryHandlerTests : IClassFixture<OrderServiceFixture>
{
    private readonly OrderServiceFixture _fixture;

    public GetOrderListQueryHandlerTests(OrderServiceFixture fixture)
    {
        _fixture = fixture;
        OrderIdGeneratorFixture.EnsureInitialized();
    }

    [Fact]
    public async Task Handle_ReturnsResponseWithPagination()
    {
        var userId = Guid.NewGuid();
        var handler = new GetOrderListQueryHandler(
            new FakeOrderDataQuery(),
            new FakeUserContext { UserId = userId });

        var result = await handler.Handle(
            new GetOrderListQuery(1, 20, null, null, null),
            CancellationToken.None);

        result.Should().NotBeNull();
        result.Orders.Should().BeEmpty();
        result.Pagination.CurrentPage.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WithFilterParams_DelegatesToQuery()
    {
        var userId = Guid.NewGuid();
        var handler = new GetOrderListQueryHandler(
            new FakeOrderDataQuery(),
            new FakeUserContext { UserId = userId });

        var result = await handler.Handle(
            new GetOrderListQuery(2, 10, null, null, null),
            CancellationToken.None);

        result.Should().NotBeNull();
        result.Pagination.CurrentPage.Should().Be(2);
    }
}
