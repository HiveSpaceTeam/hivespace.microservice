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

        var order = await orderRepository.GetByIdAsync(message.OrderId, cancellationToken: ct);
        if (order is null)
        {
            logger.LogWarning("Order {OrderId} not found for COD marking", message.OrderId);
            await PublishCodFailed(context, message.OrderId, "Order not found for COD marking");
            return;
        }

        try
        {
            order.MarkAsCOD();
            await orderRepository.SaveChangesAsync(ct);
            await PublishMarkedAsCOD(context, message.OrderId);
        }
        catch (DomainException ex) when (ex.ErrorCode.Code == OrderDomainErrorCode.OrderExceedsCODLimit.Code)
        {
            logger.LogWarning("Order {OrderId} exceeds COD limit: {Message}", message.OrderId, ex.Message);
            await PublishCodFailed(context, message.OrderId, ex.Message);
        }
        catch (DomainException ex) when (ex.ErrorCode.Code == OrderDomainErrorCode.OrderInvalidStatusForCOD.Code)
        {
            if (order.Status.Name == OrderStatus.COD.Name)
            {
                logger.LogInformation("Order {OrderId} is already COD, treating as idempotent success", message.OrderId);
                await PublishMarkedAsCOD(context, message.OrderId);
                return;
            }

            logger.LogWarning("Order {OrderId} cannot be marked COD due to status {Status}", message.OrderId, order.Status.Name);
            await PublishCodFailed(context, message.OrderId, ex.Message);
        }
    }

    private static Task PublishMarkedAsCOD(ConsumeContext<MarkOrderAsCOD> context, Guid orderId)
    {
        return context.RespondAsync<OrderMarkedAsCOD>(new
        {
            context.Message.CorrelationId,
            OrderId = orderId,
            MarkedAt = DateTimeOffset.UtcNow
        });
    }

    private static Task PublishCodFailed(ConsumeContext<MarkOrderAsCOD> context, Guid orderId, string reason)
    {
        return context.RespondAsync<MarkOrderAsCODFailed>(new
        {
            context.Message.CorrelationId,
            OrderId = orderId,
            Reason = reason
        });
    }
}
