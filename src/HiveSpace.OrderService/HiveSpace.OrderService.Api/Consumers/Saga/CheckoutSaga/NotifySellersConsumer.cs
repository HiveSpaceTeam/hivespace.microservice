using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Commands;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace HiveSpace.OrderService.Api.Consumers.Saga.CheckoutSaga;

public class NotifySellersConsumer(ILogger<NotifySellersConsumer> logger) : IConsumer<NotifySellers>
{
    public async Task Consume(ConsumeContext<NotifySellers> context)
    {
        var message = context.Message;

        logger.LogInformation("[STUB] Notify sellers for order {OrderId}, packages: {PackageIds}",
            message.OrderId, string.Join(", ", message.PackageIds));

        await context.Publish<SellersNotified>(new
        {
            message.CorrelationId,
            message.OrderId,
            SellerCount = message.PackageIds.Count
        });
    }
}
