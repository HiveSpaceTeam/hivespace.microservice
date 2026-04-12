using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Commands;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;
using HiveSpace.OrderService.Domain.Enumerations;
using HiveSpace.OrderService.Domain.Exceptions;
using HiveSpace.OrderService.Domain.Repositories;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace HiveSpace.OrderService.Api.Consumers.Saga.CheckoutSaga;

public class MarkOrderAsPaidConsumer(
    IOrderRepository orderRepository,
    ILogger<MarkOrderAsPaidConsumer> logger) : IConsumer<MarkOrderAsPaid>
{
    public async Task Consume(ConsumeContext<MarkOrderAsPaid> context)
    {
        var message = context.Message;
        var ct = context.CancellationToken;

        foreach (var orderId in message.OrderIds)
        {
            var order = await orderRepository.GetByIdAsync(orderId, ct);
            if (order is null)
            {
                logger.LogWarning("Order {OrderId} not found for paid marking", orderId);
                await context.RespondAsync<MarkOrderAsPaidFailed>(new
                {
                    message.CorrelationId,
                    OrderId = orderId,
                    Reason  = $"Order {orderId} not found"
                });
                return;
            }

            try
            {
                order.MarkAsPaid(message.PaymentId);
            }
            catch (DomainException ex) when (
                ex.ErrorCode.Code == OrderDomainErrorCode.OrderInvalidStatusForPayment.Code)
            {
                logger.LogWarning("Order {OrderId} cannot be marked as paid: {ErrorCode}", orderId, ex.ErrorCode.Code);
                await context.RespondAsync<MarkOrderAsPaidFailed>(new
                {
                    message.CorrelationId,
                    OrderId = orderId,
                    Reason  = ex.Message
                });
                return;
            }
        }

        await orderRepository.SaveChangesAsync(ct);

        await context.RespondAsync<OrderMarkedAsPaid>(new
        {
            message.CorrelationId,
            OrderIds = message.OrderIds,
            PaidAt   = DateTimeOffset.UtcNow
        });
    }
}
