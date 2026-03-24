using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Commands;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;
using HiveSpace.OrderService.Application.Orders.Commands.CancelOrder;
using MassTransit;
using MediatR;

namespace HiveSpace.OrderService.Api.Consumers.Saga.CheckoutSaga;

public class CancelOrderConsumer(ISender mediator) : IConsumer<CancelOrder>
{
    public async Task Consume(ConsumeContext<CancelOrder> context)
    {
        var message = context.Message;

        await mediator.Send(
            new CancelOrderCommand(message.OrderId, message.Reason, CancelledBy: Guid.Empty),
            context.CancellationToken);

        await context.Publish<OrderCancelled>(new
        {
            message.CorrelationId,
            message.OrderId,
            message.Reason,
            CancelledAt = DateTimeOffset.UtcNow
        });
    }
}
