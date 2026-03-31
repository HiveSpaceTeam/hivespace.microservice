using HiveSpace.OrderService.Application.Contracts;
using HiveSpace.OrderService.Domain.Repositories;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace HiveSpace.OrderService.Api.Consumers.Saga.CheckoutSaga;

public class ClearCartConsumer(
    ICartRepository cartRepository,
    ILogger<ClearCartConsumer> logger) : IConsumer<ClearCart>
{
    public async Task Consume(ConsumeContext<ClearCart> context)
    {
        var message = context.Message;
        var ct = context.CancellationToken;

        var cart = await cartRepository.GetByUserIdAsync(message.UserId, ct);
        if (cart is not null && !cart.IsEmpty())
        {
            cart.ClearSelectedItems();
            await cartRepository.SaveChangesAsync(ct);
            logger.LogInformation("Selected cart items cleared for user {UserId} (checkout {CorrelationId})",
                message.UserId, message.CorrelationId);
        }

        await context.RespondAsync<CartCleared>(new { message.CorrelationId });
    }
}
