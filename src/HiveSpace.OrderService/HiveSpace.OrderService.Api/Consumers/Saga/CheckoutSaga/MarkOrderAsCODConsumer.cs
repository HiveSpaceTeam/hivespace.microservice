using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Commands;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;
using HiveSpace.OrderService.Domain.Enumerations;
using HiveSpace.OrderService.Domain.Exceptions;
using HiveSpace.OrderService.Domain.Repositories;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace HiveSpace.OrderService.Api.Consumers.Saga.CheckoutSaga;

public class MarkOrderAsCODConsumer(
    IOrderRepository orderRepository,
    ILogger<MarkOrderAsCODConsumer> logger) : IConsumer<MarkOrderAsCOD>
{
    public async Task Consume(ConsumeContext<MarkOrderAsCOD> context)
    {
        var message = context.Message;
        var ct = context.CancellationToken;

        foreach (var orderId in message.OrderIds)
        {
            var order = await orderRepository.GetByIdAsync(orderId, ct);
            if (order is null)
            {
                logger.LogWarning("Order {OrderId} not found for COD marking", orderId);
                await context.RespondAsync<MarkOrderAsCODFailed>(new
                {
                    message.CorrelationId,
                    OrderId = orderId,
                    Reason  = $"Order {orderId} not found"
                });
                return;
            }

            try
            {
                if (order.Status.Name != OrderStatus.COD.Name)
                    order.MarkAsCOD();
            }
            catch (DomainException ex) when (
                ex.ErrorCode.Code == OrderDomainErrorCode.OrderExceedsCODLimit.Code ||
                ex.ErrorCode.Code == OrderDomainErrorCode.OrderInvalidStatusForCOD.Code)
            {
                logger.LogWarning("Order {OrderId} cannot be marked as COD: {ErrorCode}", orderId, ex.ErrorCode.Code);
                await context.RespondAsync<MarkOrderAsCODFailed>(new
                {
                    message.CorrelationId,
                    OrderId = orderId,
                    Reason  = ex.Message
                });
                return;
            }
        }

        await orderRepository.SaveChangesAsync(ct);

        await context.RespondAsync<OrderMarkedAsCOD>(new
        {
            message.CorrelationId,
            OrderIds = message.OrderIds,
            MarkedAt = DateTimeOffset.UtcNow
        });
    }
}
