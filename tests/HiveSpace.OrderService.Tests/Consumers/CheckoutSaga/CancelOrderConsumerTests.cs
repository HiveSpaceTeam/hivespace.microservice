using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Commands;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;
using HiveSpace.OrderService.Api.Consumers.Saga.CheckoutSaga;
using HiveSpace.OrderService.Application.Orders.Commands.CancelOrder;
using MassTransit;
using MediatR;
using NSubstitute;
using Xunit;

namespace HiveSpace.OrderService.Tests.Consumers.CheckoutSaga;

public class CancelOrderConsumerTests
{
    private readonly ISender _mediator = Substitute.For<ISender>();
    private readonly CancelOrderConsumer _consumer;

    public CancelOrderConsumerTests()
    {
        _consumer = new CancelOrderConsumer(_mediator);
    }

    [Fact]
    public async Task Consume_SendsCancelOrderCommandAndPublishesEvent()
    {
        var orderId = Guid.NewGuid();
        var message = new CancelOrder { CorrelationId = Guid.NewGuid(), OrderId = orderId, Reason = "customer request" };
        var context = Substitute.For<ConsumeContext<CancelOrder>>();
        context.Message.Returns(message);
        context.CancellationToken.Returns(CancellationToken.None);

        await _consumer.Consume(context);

        await _mediator.Received(1).Send(
            Arg.Is<CancelOrderCommand>(c => c.OrderId == orderId && c.Reason == "customer request"),
            Arg.Any<CancellationToken>());
        await context.Received(1).Publish<OrderCancelledIntegrationEvent>(
            Arg.Any<object>(),
            Arg.Any<CancellationToken>());
    }
}
