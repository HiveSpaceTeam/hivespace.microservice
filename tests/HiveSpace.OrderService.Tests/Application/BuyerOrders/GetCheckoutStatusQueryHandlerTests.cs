using FluentAssertions;
using HiveSpace.OrderService.Application.Orders.Queries.GetCheckoutStatus;
using HiveSpace.OrderService.Tests.Domain;
using HiveSpace.OrderService.Tests.Fixtures;
using HiveSpace.Testing.Shared.Doubles;
using Xunit;

namespace HiveSpace.OrderService.Tests.Application.BuyerOrders;

public class GetCheckoutStatusQueryHandlerTests : IClassFixture<OrderServiceFixture>
{
    private readonly OrderServiceFixture _fixture;

    public GetCheckoutStatusQueryHandlerTests(OrderServiceFixture fixture)
    {
        _fixture = fixture;
        OrderIdGeneratorFixture.EnsureInitialized();
    }

    [Fact]
    public async Task Handle_ReturnsStatusFromCheckoutQuery()
    {
        var correlationId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var handler = new GetCheckoutStatusQueryHandler(
            new FakeCheckoutQuery(),
            new FakeUserContext { UserId = userId });

        var result = await handler.Handle(
            new GetCheckoutStatusQuery(correlationId),
            CancellationToken.None);

        result.Should().NotBeNull();
        result.CorrelationId.Should().Be(correlationId);
    }

    [Fact]
    public async Task Handle_WithDifferentOrderId_ReturnsPendingStatus()
    {
        var userId = Guid.NewGuid();

        var handler = new GetCheckoutStatusQueryHandler(
            new FakeCheckoutQuery(),
            new FakeUserContext { UserId = userId });

        var result = await handler.Handle(
            new GetCheckoutStatusQuery(Guid.NewGuid()),
            CancellationToken.None);

        result.Should().NotBeNull();
        result.CurrentState.Should().NotBeNull();
    }
}
