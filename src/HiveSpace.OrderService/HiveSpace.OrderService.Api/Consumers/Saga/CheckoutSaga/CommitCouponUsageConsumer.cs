using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Commands;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;
using HiveSpace.OrderService.Application.Orders;
using HiveSpace.OrderService.Domain.Repositories;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace HiveSpace.OrderService.Api.Consumers.Saga.CheckoutSaga;

public class CommitCouponUsageConsumer(
    IOrderRepository orderRepository,
    ICouponRepository couponRepository,
    ILogger<CommitCouponUsageConsumer> logger) : IConsumer<CommitCouponUsage>
{
    public async Task Consume(ConsumeContext<CommitCouponUsage> context)
    {
        var message = context.Message;
        var ct = context.CancellationToken;

        var missingOrderId = await GetMissingOrderIdAsync(message.OrderIds, orderRepository, ct);
        if (missingOrderId.HasValue)
        {
            logger.LogWarning("Order {OrderId} not found for coupon usage commit", missingOrderId.Value);
            await context.RespondAsync<CommitCouponUsageFailed>(new
            {
                message.CorrelationId,
                Reason = $"Order {missingOrderId.Value} not found",
                Errors = Array.Empty<string>()
            });
            return;
        }

        var orderCouponUsages = await orderRepository.GetCouponUsageEntriesByOrderIdsAsync(message.OrderIds, ct);

        try
        {
            await CheckoutCouponUsageRecorder.CommitAsync(orderCouponUsages, couponRepository, ct);
        }
        catch (DomainException ex)
        {
            logger.LogWarning("Coupon usage commit failed for checkout {CorrelationId}: {ErrorCode}", message.CorrelationId, ex.ErrorCode.Code);
            await context.RespondAsync<CommitCouponUsageFailed>(new
            {
                message.CorrelationId,
                Reason = ex.Message,
                Errors = new[] { ex.ErrorCode.Name }
            });
            return;
        }

        await couponRepository.SaveChangesAsync(ct);

        await context.RespondAsync<CouponUsageCommitted>(new
        {
            message.CorrelationId,
            message.OrderIds,
            CommittedAt = DateTimeOffset.UtcNow
        });
    }

    private static async Task<Guid?> GetMissingOrderIdAsync(
        IEnumerable<Guid> orderIds,
        IOrderRepository orderRepository,
        CancellationToken cancellationToken)
    {
        foreach (var orderId in orderIds.Distinct())
        {
            if (await orderRepository.GetByIdAsync(orderId, cancellationToken) is null)
                return orderId;
        }

        return null;
    }
}
