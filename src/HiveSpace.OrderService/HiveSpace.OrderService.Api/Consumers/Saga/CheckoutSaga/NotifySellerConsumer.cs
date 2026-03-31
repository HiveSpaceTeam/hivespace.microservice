using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Commands;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace HiveSpace.OrderService.Api.Consumers.Saga.CheckoutSaga;

public class NotifySellerConsumer(ILogger<NotifySellerConsumer> logger) : IConsumer<NotifySeller>
{
    public async Task Consume(ConsumeContext<NotifySeller> context)
    {
        var message = context.Message;

        logger.LogInformation("[STUB] Notify seller for order {OrderId} (store {StoreId})",
            message.OrderId, message.StoreId);

        await context.Publish<SellerNotified>(new
        {
            message.CorrelationId,
            message.OrderId
        });
    }
}
