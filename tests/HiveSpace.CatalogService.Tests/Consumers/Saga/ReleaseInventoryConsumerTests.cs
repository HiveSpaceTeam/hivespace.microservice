using HiveSpace.CatalogService.Api.Consumers.Saga.Checkout;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Commands;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace HiveSpace.CatalogService.Tests.Consumers.Saga;

public class ReleaseInventoryConsumerTests
{
    private readonly ILogger<ReleaseInventoryConsumer> _logger = Substitute.For<ILogger<ReleaseInventoryConsumer>>();
    private readonly ReleaseInventoryConsumer _consumer;

    public ReleaseInventoryConsumerTests()
    {
        _consumer = new ReleaseInventoryConsumer(_logger);
    }

    [Fact]
    public async Task Consume_PublishesInventoryReleasedEvent()
    {
        var message = new ReleaseInventory { CorrelationId = Guid.NewGuid(), OrderId = Guid.NewGuid(), ReservationIds = [Guid.NewGuid()] };
        var ctx = Substitute.For<ConsumeContext<ReleaseInventory>>();
        ctx.Message.Returns(message);
        ctx.CancellationToken.Returns(CancellationToken.None);

        await _consumer.Consume(ctx);

        await ctx.Received(1).Publish<InventoryReleasedIntegrationEvent>(Arg.Any<object>(), Arg.Any<CancellationToken>());
    }
}
