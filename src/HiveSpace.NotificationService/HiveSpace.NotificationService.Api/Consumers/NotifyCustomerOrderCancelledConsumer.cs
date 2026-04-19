using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Commands;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;
using HiveSpace.NotificationService.Core.DomainModels;
using HiveSpace.NotificationService.Core.Interfaces;
using HiveSpace.NotificationService.Core.Models;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace HiveSpace.NotificationService.Api.Consumers;

public class NotifyCustomerOrderCancelledConsumer(
    IDispatchPipeline                            pipeline,
    IUserRefRepository                           userRefs,
    ILogger<NotifyCustomerOrderCancelledConsumer> logger) : IConsumer<NotifyCustomerOrderCancelled>
{
    public async Task Consume(ConsumeContext<NotifyCustomerOrderCancelled> context)
    {
        var msg = context.Message;
        var ct  = context.CancellationToken;

        var user = await userRefs.GetByIdAsync(msg.UserId, ct);
        if (user is null)
        {
            logger.LogWarning("UserRef not found for UserId={UserId} — skipping order cancelled notification", msg.UserId);
            await context.Publish<CustomerNotified>(new
            {
                msg.CorrelationId,
                msg.OrderId,
                NotifiedAt = DateTimeOffset.UtcNow
            });
            return;
        }

        await pipeline.DispatchAsync(new NotificationRequest
        {
            UserId         = msg.UserId,
            EventType      = NotificationEventType.OrderCancelled,
            IdempotencyKey = $"{NotificationEventType.OrderCancelled}:{msg.OrderId}",
            Locale         = user.Locale,
            TemplateData   = new Dictionary<string, object>
            {
                ["order_id"]      = msg.OrderId,
                ["order_code"]    = msg.OrderCode,
                ["refund_amount"] = msg.RefundAmount,
                ["buyer_name"]    = user.FullName,
                ["avatar_url"]    = user.AvatarUrl ?? string.Empty,
            }
        }, ct);

        await context.Publish<CustomerNotified>(new
        {
            msg.CorrelationId,
            msg.OrderId,
            NotifiedAt = DateTimeOffset.UtcNow
        });
    }
}
