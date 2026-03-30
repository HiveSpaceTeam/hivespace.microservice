using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;
using HiveSpace.OrderService.Domain.Repositories;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace HiveSpace.OrderService.Api.Consumers.Saga.CheckoutSaga;

public class ClearCartConsumer(
    ICartRepository cartRepository,
    ILogger<ClearCartConsumer> logger) : IConsumer<CheckoutPaymentSettled>
{
    public async Task Consume(ConsumeContext<CheckoutPaymentSettled> context)
    {
        var message = context.Message;
        var ct = context.CancellationToken;

        var cart = await cartRepository.GetByUserIdAsync(message.UserId, ct);
        if (cart is null || cart.IsEmpty())
        {
            logger.LogInformation("No cart items to clear for user {UserId} after checkout {CorrelationId}",
                message.UserId, message.CorrelationId);
            return;
        }

        cart.ClearSelectedItems();
        await cartRepository.SaveChangesAsync(ct);

        logger.LogInformation("Selected cart items cleared for user {UserId} after checkout {CorrelationId}",
            message.UserId, message.CorrelationId);
    }
}
