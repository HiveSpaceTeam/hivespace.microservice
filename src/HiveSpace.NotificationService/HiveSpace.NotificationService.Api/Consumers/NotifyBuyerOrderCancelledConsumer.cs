using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Commands;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;
using HiveSpace.NotificationService.Core.DomainModels;
using HiveSpace.NotificationService.Core.Interfaces;
using HiveSpace.NotificationService.Core.Dispatch.Models;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace HiveSpace.NotificationService.Api.Consumers;

public class NotifyBuyerOrderCancelledConsumer(
    IDispatchPipeline                           pipeline,
    IUserRefRepository                          userRefs,
    ILogger<NotifyBuyerOrderCancelledConsumer>  logger) : IConsumer<NotifyBuyerOrderCancelled>
{
    public async Task Consume(ConsumeContext<NotifyBuyerOrderCancelled> context)
    {
        var msg = context.Message;
        var ct  = context.CancellationToken;

        var user = await userRefs.GetByIdAsync(msg.BuyerId, ct);
        if (user is null)
        {
            logger.LogWarning("UserRef not found for BuyerId={BuyerId} — skipping order cancelled notification", msg.BuyerId);
            await context.Publish<BuyerNotified>(new
            {
                msg.CorrelationId,
                msg.OrderId,
                NotifiedAt = DateTimeOffset.UtcNow
            });
            return;
        }

        await pipeline.DispatchAsync(new NotificationRequest
        {
            UserId         = msg.BuyerId,
            EventType      = NotificationEventType.OrderCancelled,
            IdempotencyKey = $"{NotificationEventType.OrderCancelled}:{msg.OrderId}",
            Locale         = user.Locale,
            TemplateData   = new Dictionary<string, object>
            {
                ["orderId"]      = msg.OrderId,
                ["orderCode"]    = msg.OrderCode,
                ["refundAmount"] = msg.RefundAmount,
                ["buyerName"]    = user.FullName,
                ["avatarUrl"]    = user.AvatarUrl ?? string.Empty,
            }
        }, ct);

        await context.Publish<BuyerNotified>(new
        {
            msg.CorrelationId,
            msg.OrderId,
            NotifiedAt = DateTimeOffset.UtcNow
        });
    }
}
