using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.Infrastructure.Messaging.Shared.FulfillmentSaga.Commands;
using HiveSpace.Infrastructure.Messaging.Shared.FulfillmentSaga.Events;
using HiveSpace.NotificationService.Core.Dispatch.Models;
using HiveSpace.NotificationService.Api.Consumers;
using HiveSpace.NotificationService.Core.DomainModels;
using HiveSpace.NotificationService.Core.DomainModels.External;
using HiveSpace.NotificationService.Core.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace HiveSpace.NotificationService.Tests.Consumers;

public class NotifyBuyerOrderCancelledConsumerTests
{
    private readonly IDispatchPipeline _pipeline = Substitute.For<IDispatchPipeline>();
    private readonly IUserRefRepository _userRefs = Substitute.For<IUserRefRepository>();
    private readonly ILogger<NotifyBuyerOrderCancelledConsumer> _logger = Substitute.For<ILogger<NotifyBuyerOrderCancelledConsumer>>();
    private readonly NotifyBuyerOrderCancelledConsumer _consumer;

    public NotifyBuyerOrderCancelledConsumerTests()
    {
        _consumer = new NotifyBuyerOrderCancelledConsumer(_pipeline, _userRefs, _logger);
    }

    [Fact]
    public async Task Consume_WhenUserNotFound_PublishesBuyerNotifiedWithoutDispatching()
    {
        var msg = new NotifyBuyerOrderCancelled { CorrelationId = Guid.NewGuid(), OrderId = Guid.NewGuid(), BuyerId = Guid.NewGuid(), RefundAmount = 0, OrderCode = "ORD-001" };
        var ctx = Substitute.For<ConsumeContext<NotifyBuyerOrderCancelled>>();
        ctx.Message.Returns(msg);
        ctx.CancellationToken.Returns(CancellationToken.None);
        _userRefs.GetByIdAsync(msg.BuyerId, Arg.Any<CancellationToken>()).Returns((UserRef?)null);

        await _consumer.Consume(ctx);

        await _pipeline.DidNotReceive().DispatchAsync(Arg.Any<NotificationRequest>(), Arg.Any<CancellationToken>());
        await ctx.Received(1).Publish<BuyerNotifiedIntegrationEvent>(Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_WhenUserFound_DispatchesAndPublishesBuyerNotified()
    {
        var buyerId = Guid.NewGuid();
        var msg = new NotifyBuyerOrderCancelled { CorrelationId = Guid.NewGuid(), OrderId = Guid.NewGuid(), BuyerId = buyerId, RefundAmount = 50_000, OrderCode = "ORD-002" };
        var ctx = Substitute.For<ConsumeContext<NotifyBuyerOrderCancelled>>();
        ctx.Message.Returns(msg);
        ctx.CancellationToken.Returns(CancellationToken.None);
        var userRef = UserRef.Create(buyerId, "buyer@example.com", "Buyer", locale: Culture.Vi);
        _userRefs.GetByIdAsync(buyerId, Arg.Any<CancellationToken>()).Returns(userRef);

        await _consumer.Consume(ctx);

        await _pipeline.Received(1).DispatchAsync(
            Arg.Is<NotificationRequest>(r => r.EventType == NotificationEventType.OrderCancelled),
            Arg.Any<CancellationToken>());
        await ctx.Received(1).Publish<BuyerNotifiedIntegrationEvent>(Arg.Any<object>(), Arg.Any<CancellationToken>());
    }
}
