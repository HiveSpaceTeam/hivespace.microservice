using FluentAssertions;
using HiveSpace.CatalogService.Api.Consumers.Saga.Checkout;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Commands;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace HiveSpace.CatalogService.Tests.Consumers.Saga;

public class ReserveInventoryConsumerTests
{
    private readonly ILogger<ReserveInventoryConsumer> _logger = Substitute.For<ILogger<ReserveInventoryConsumer>>();
    private readonly ReserveInventoryConsumer _consumer;

    public ReserveInventoryConsumerTests()
    {
        _consumer = new ReserveInventoryConsumer(_logger);
    }

    [Fact]
    public async Task Consume_WithEmptyOrderIds_ThrowsInvalidFieldException()
    {
        var message = new ReserveInventory { CorrelationId = Guid.NewGuid(), OrderIds = [], Items = [], ExpirationMinutes = 15 };
        var ctx = Substitute.For<ConsumeContext<ReserveInventory>>();
        ctx.Message.Returns(message);
        ctx.CancellationToken.Returns(CancellationToken.None);

        var act = async () => await _consumer.Consume(ctx);

        await act.Should().ThrowAsync<InvalidFieldException>();
    }

    [Fact]
    public async Task Consume_WithValidOrderIds_RespondsWithReserved()
    {
        var message = new ReserveInventory { CorrelationId = Guid.NewGuid(), OrderIds = [Guid.NewGuid()], Items = [], ExpirationMinutes = 15 };
        var ctx = Substitute.For<ConsumeContext<ReserveInventory>>();
        ctx.Message.Returns(message);
        ctx.CancellationToken.Returns(CancellationToken.None);

        await _consumer.Consume(ctx);

        await ctx.Received(1).RespondAsync<InventoryReservedIntegrationEvent>(Arg.Any<object>());
    }
}
