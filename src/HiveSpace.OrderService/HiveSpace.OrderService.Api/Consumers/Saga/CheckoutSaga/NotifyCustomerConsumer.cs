using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Commands;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace HiveSpace.OrderService.Api.Consumers.Saga.CheckoutSaga;

public class NotifyCustomerConsumer(ILogger<NotifyCustomerConsumer> logger) : IConsumer<NotifyCustomer>
{
    public async Task Consume(ConsumeContext<NotifyCustomer> context)
    {
        var message = context.Message;

        logger.LogInformation(
            "[STUB] Notify customer {UserId} for order {OrderId}. Confirmed: {WasConfirmed}",
            message.UserId, message.OrderId, message.WasConfirmed);

        await context.Publish<CustomerNotified>(new
        {
            message.CorrelationId,
            message.OrderId,
            NotifiedAt = DateTimeOffset.UtcNow
        });
    }
}
