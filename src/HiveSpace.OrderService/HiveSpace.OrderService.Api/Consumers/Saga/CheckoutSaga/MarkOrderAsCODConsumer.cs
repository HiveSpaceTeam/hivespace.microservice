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

        var orders = await orderRepository.GetByIdsAsync(message.OrderIds, ct);
        var missingId = message.OrderIds.FirstOrDefault(id => orders.All(o => o.Id != id));
        if (missingId != default)
        {
            logger.LogWarning("Order {OrderId} not found for COD marking", missingId);
            await context.RespondAsync<MarkOrderAsCODFailed>(new
            {
                message.CorrelationId,
                OrderId = missingId,
                Reason  = $"Order {missingId} not found"
            });
            return;
        }

        foreach (var order in orders)
        {
            try
            {
                if (order.Status.Name != OrderStatus.COD.Name)
                    order.MarkAsCOD();
            }
            catch (DomainException ex) when (
                ex.ErrorCode.Code == OrderDomainErrorCode.OrderExceedsCODLimit.Code ||
                ex.ErrorCode.Code == OrderDomainErrorCode.OrderInvalidStatusForCOD.Code)
            {
                logger.LogWarning("Order {OrderId} cannot be marked as COD: {ErrorCode}", order.Id, ex.ErrorCode.Code);
                await context.RespondAsync<MarkOrderAsCODFailed>(new
                {
                    message.CorrelationId,
                    OrderId = order.Id,
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
