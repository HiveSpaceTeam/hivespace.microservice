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

public class NotifyBuyerOrderConfirmedConsumerTests
{
    private readonly IDispatchPipeline _pipeline = Substitute.For<IDispatchPipeline>();
    private readonly IUserRefRepository _userRefs = Substitute.For<IUserRefRepository>();
    private readonly ILogger<NotifyBuyerOrderConfirmedConsumer> _logger = Substitute.For<ILogger<NotifyBuyerOrderConfirmedConsumer>>();
    private readonly NotifyBuyerOrderConfirmedConsumer _consumer;

    public NotifyBuyerOrderConfirmedConsumerTests()
    {
        _consumer = new NotifyBuyerOrderConfirmedConsumer(_pipeline, _userRefs, _logger);
    }

    [Fact]
    public async Task Consume_WhenBuyerNotFound_PublishesBuyerNotifiedWithoutDispatching()
    {
        var msg = new NotifyBuyerOrderConfirmed { CorrelationId = Guid.NewGuid(), OrderId = Guid.NewGuid(), BuyerId = Guid.NewGuid(), StoreId = Guid.NewGuid(), OrderCode = "ORD-001" };
        var ctx = Substitute.For<ConsumeContext<NotifyBuyerOrderConfirmed>>();
        ctx.Message.Returns(msg);
        ctx.CancellationToken.Returns(CancellationToken.None);
        _userRefs.GetByIdAsync(msg.BuyerId, Arg.Any<CancellationToken>()).Returns((UserRef?)null);

        await _consumer.Consume(ctx);

        await _pipeline.DidNotReceive().DispatchAsync(Arg.Any<NotificationRequest>(), Arg.Any<CancellationToken>());
        await ctx.Received(1).Publish<BuyerNotifiedIntegrationEvent>(Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_WhenBuyerFound_DispatchesAndPublishesBuyerNotified()
    {
        var buyerId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var msg = new NotifyBuyerOrderConfirmed { CorrelationId = Guid.NewGuid(), OrderId = Guid.NewGuid(), BuyerId = buyerId, StoreId = storeId, OrderCode = "ORD-002" };
        var ctx = Substitute.For<ConsumeContext<NotifyBuyerOrderConfirmed>>();
        ctx.Message.Returns(msg);
        ctx.CancellationToken.Returns(CancellationToken.None);
        var buyerRef = UserRef.Create(buyerId, "buyer@example.com", "Buyer", locale: Culture.Vi);
        _userRefs.GetByIdAsync(buyerId, Arg.Any<CancellationToken>()).Returns(buyerRef);
        _userRefs.GetByStoreIdAsync(storeId, Arg.Any<CancellationToken>()).Returns((UserRef?)null);

        await _consumer.Consume(ctx);

        await _pipeline.Received(1).DispatchAsync(
            Arg.Is<NotificationRequest>(r => r.EventType == NotificationEventType.OrderConfirmed),
            Arg.Any<CancellationToken>());
        await ctx.Received(1).Publish<BuyerNotifiedIntegrationEvent>(Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_WhenBuyerAndStoreFound_DispatchesWithStoreInfo()
    {
        var buyerId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var msg = new NotifyBuyerOrderConfirmed { CorrelationId = Guid.NewGuid(), OrderId = Guid.NewGuid(), BuyerId = buyerId, StoreId = storeId, OrderCode = "ORD-003" };
        var ctx = Substitute.For<ConsumeContext<NotifyBuyerOrderConfirmed>>();
        ctx.Message.Returns(msg);
        ctx.CancellationToken.Returns(CancellationToken.None);
        var buyerRef = UserRef.Create(buyerId, "buyer@example.com", "Buyer", locale: Culture.Vi);
        var sellerRef = UserRef.Create(Guid.NewGuid(), "seller@example.com", "Seller",
            storeId: storeId, storeName: "My Store", storeLogoUrl: "https://logo.png");
        _userRefs.GetByIdAsync(buyerId, Arg.Any<CancellationToken>()).Returns(buyerRef);
        _userRefs.GetByStoreIdAsync(storeId, Arg.Any<CancellationToken>()).Returns(sellerRef);

        await _consumer.Consume(ctx);

        await _pipeline.Received(1).DispatchAsync(
            Arg.Is<NotificationRequest>(r => r.EventType == NotificationEventType.OrderConfirmed),
            Arg.Any<CancellationToken>());
        await ctx.Received(1).Publish<BuyerNotifiedIntegrationEvent>(Arg.Any<object>(), Arg.Any<CancellationToken>());
    }
}
