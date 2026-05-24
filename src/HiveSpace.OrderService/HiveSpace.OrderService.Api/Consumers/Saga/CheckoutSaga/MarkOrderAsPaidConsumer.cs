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

        var orders = await orderRepository.GetByIdsAsync(message.OrderIds, ct);
        var missingId = message.OrderIds.FirstOrDefault(id => orders.All(o => o.Id != id));
        if (missingId != default)
        {
            logger.LogWarning("Order {OrderId} not found for paid marking", missingId);
            await context.RespondAsync<MarkOrderAsPaidFailedIntegrationEvent>(new
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
                order.MarkAsPaid(message.PaymentId);
            }
            catch (DomainException ex) when (
                ex.ErrorCode.Code == OrderDomainErrorCode.OrderInvalidStatusForPayment.Code)
            {
                logger.LogWarning("Order {OrderId} cannot be marked as paid: {ErrorCode}", order.Id, ex.ErrorCode.Code);
                await context.RespondAsync<MarkOrderAsPaidFailedIntegrationEvent>(new
                {
                    message.CorrelationId,
                    OrderId = order.Id,
                    Reason  = ex.Message
                });
                return;
            }
        }

        await orderRepository.SaveChangesAsync(ct);

        await context.RespondAsync<OrderMarkedAsPaidIntegrationEvent>(new
        {
            message.CorrelationId,
            OrderIds = message.OrderIds,
            PaidAt   = DateTimeOffset.UtcNow
        });
    }
}
