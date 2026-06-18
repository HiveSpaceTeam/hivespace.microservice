using HiveSpace.CatalogService.Api.Consumers.Saga.Checkout;
using HiveSpace.Infrastructure.Messaging.Shared.FulfillmentSaga.Commands;
using HiveSpace.Infrastructure.Messaging.Shared.FulfillmentSaga.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace HiveSpace.CatalogService.Tests.Consumers.Saga;

public class ConfirmInventoryConsumerTests
{
    private readonly ILogger<ConfirmInventoryConsumer> _logger = Substitute.For<ILogger<ConfirmInventoryConsumer>>();
    private readonly ConfirmInventoryConsumer _consumer;

    public ConfirmInventoryConsumerTests()
    {
        _consumer = new ConfirmInventoryConsumer(_logger);
    }

    [Fact]
    public async Task Consume_WithNoExpiredReservations_RespondsWithConfirmed()
    {
        var message = new ConfirmInventory { CorrelationId = Guid.NewGuid(), OrderId = Guid.NewGuid(), ReservationIds = [Guid.NewGuid()] };
        var ctx = Substitute.For<ConsumeContext<ConfirmInventory>>();
        ctx.Message.Returns(message);
        ctx.CancellationToken.Returns(CancellationToken.None);

        await _consumer.Consume(ctx);

        await ctx.Received(1).RespondAsync<InventoryConfirmedIntegrationEvent>(Arg.Any<object>());
    }
}
