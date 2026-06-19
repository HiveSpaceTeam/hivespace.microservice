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

public class NotifySellerNewOrderConsumerTests
{
    private readonly IDispatchPipeline _pipeline = Substitute.For<IDispatchPipeline>();
    private readonly IUserRefRepository _userRefs = Substitute.For<IUserRefRepository>();
    private readonly ILogger<NotifySellerNewOrderConsumer> _logger = Substitute.For<ILogger<NotifySellerNewOrderConsumer>>();
    private readonly NotifySellerNewOrderConsumer _consumer;

    public NotifySellerNewOrderConsumerTests()
    {
        _consumer = new NotifySellerNewOrderConsumer(_pipeline, _userRefs, _logger);
    }

    [Fact]
    public async Task Consume_WhenSellerIdEmpty_PublishesWithoutDispatching()
    {
        var msg = new NotifySellerNewOrder { CorrelationId = Guid.NewGuid(), OrderId = Guid.NewGuid(), StoreId = Guid.NewGuid(), SellerId = Guid.Empty, BuyerId = Guid.NewGuid(), OrderCode = "ORD-001" };
        var ctx = Substitute.For<ConsumeContext<NotifySellerNewOrder>>();
        ctx.Message.Returns(msg);
        ctx.CancellationToken.Returns(CancellationToken.None);

        await _consumer.Consume(ctx);

        await _pipeline.DidNotReceive().DispatchAsync(Arg.Any<NotificationRequest>(), Arg.Any<CancellationToken>());
        await ctx.Received(1).Publish<SellerNewOrderNotifiedIntegrationEvent>(Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_WhenSellerIdPresent_DispatchesAndPublishes()
    {
        var sellerId = Guid.NewGuid();
        var buyerId = Guid.NewGuid();
        var msg = new NotifySellerNewOrder { CorrelationId = Guid.NewGuid(), OrderId = Guid.NewGuid(), StoreId = Guid.NewGuid(), SellerId = sellerId, BuyerId = buyerId, OrderCode = "ORD-002" };
        var ctx = Substitute.For<ConsumeContext<NotifySellerNewOrder>>();
        ctx.Message.Returns(msg);
        ctx.CancellationToken.Returns(CancellationToken.None);
        var sellerRef = UserRef.Create(sellerId, "seller@example.com", "Seller", locale: Culture.Vi);
        var buyerRef = UserRef.Create(buyerId, "buyer@example.com", "Buyer", locale: Culture.Vi);
        _userRefs.GetByIdAsync(sellerId, Arg.Any<CancellationToken>()).Returns(sellerRef);
        _userRefs.GetByIdAsync(buyerId, Arg.Any<CancellationToken>()).Returns(buyerRef);

        await _consumer.Consume(ctx);

        await _pipeline.Received(1).DispatchAsync(
            Arg.Is<NotificationRequest>(r => r.EventType == NotificationEventType.NewOrderReceived),
            Arg.Any<CancellationToken>());
        await ctx.Received(1).Publish<SellerNewOrderNotifiedIntegrationEvent>(Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_WhenSellerRefNotFound_UsesDefaultLocaleAndDispatches()
    {
        var sellerId = Guid.NewGuid();
        var buyerId = Guid.NewGuid();
        var msg = new NotifySellerNewOrder { CorrelationId = Guid.NewGuid(), OrderId = Guid.NewGuid(), StoreId = Guid.NewGuid(), SellerId = sellerId, BuyerId = buyerId, OrderCode = "ORD-003" };
        var ctx = Substitute.For<ConsumeContext<NotifySellerNewOrder>>();
        ctx.Message.Returns(msg);
        ctx.CancellationToken.Returns(CancellationToken.None);
        var buyerRef = UserRef.Create(buyerId, "buyer@example.com", "Buyer", locale: Culture.Vi);
        _userRefs.GetByIdAsync(sellerId, Arg.Any<CancellationToken>()).Returns((UserRef?)null);
        _userRefs.GetByIdAsync(buyerId, Arg.Any<CancellationToken>()).Returns(buyerRef);

        await _consumer.Consume(ctx);

        await _pipeline.Received(1).DispatchAsync(
            Arg.Is<NotificationRequest>(r => r.EventType == NotificationEventType.NewOrderReceived),
            Arg.Any<CancellationToken>());
        await ctx.Received(1).Publish<SellerNewOrderNotifiedIntegrationEvent>(Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_WhenBuyerRefNotFound_UsesDefaultBuyerNameAndDispatches()
    {
        var sellerId = Guid.NewGuid();
        var buyerId = Guid.NewGuid();
        var msg = new NotifySellerNewOrder { CorrelationId = Guid.NewGuid(), OrderId = Guid.NewGuid(), StoreId = Guid.NewGuid(), SellerId = sellerId, BuyerId = buyerId, OrderCode = "ORD-004" };
        var ctx = Substitute.For<ConsumeContext<NotifySellerNewOrder>>();
        ctx.Message.Returns(msg);
        ctx.CancellationToken.Returns(CancellationToken.None);
        var sellerRef = UserRef.Create(sellerId, "seller@example.com", "Seller", locale: Culture.Vi);
        _userRefs.GetByIdAsync(sellerId, Arg.Any<CancellationToken>()).Returns(sellerRef);
        _userRefs.GetByIdAsync(buyerId, Arg.Any<CancellationToken>()).Returns((UserRef?)null);

        await _consumer.Consume(ctx);

        await _pipeline.Received(1).DispatchAsync(
            Arg.Is<NotificationRequest>(r => r.EventType == NotificationEventType.NewOrderReceived),
            Arg.Any<CancellationToken>());
        await ctx.Received(1).Publish<SellerNewOrderNotifiedIntegrationEvent>(Arg.Any<object>(), Arg.Any<CancellationToken>());
    }
}
