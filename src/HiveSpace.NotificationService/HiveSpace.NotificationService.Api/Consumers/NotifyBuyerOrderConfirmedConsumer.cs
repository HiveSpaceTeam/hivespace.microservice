using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Commands;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;
using HiveSpace.NotificationService.Core.DomainModels;
using HiveSpace.NotificationService.Core.Interfaces;
using HiveSpace.NotificationService.Core.Dispatch.Models;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace HiveSpace.NotificationService.Api.Consumers;

public class NotifyBuyerOrderConfirmedConsumer(
    IDispatchPipeline                          pipeline,
    IUserRefRepository                         userRefs,
    ILogger<NotifyBuyerOrderConfirmedConsumer> logger) : IConsumer<NotifyBuyerOrderConfirmed>
{
    public async Task Consume(ConsumeContext<NotifyBuyerOrderConfirmed> context)
    {
        var msg = context.Message;
        var ct  = context.CancellationToken;

        var user = await userRefs.GetByIdAsync(msg.BuyerId, ct);
        if (user is null)
        {
            logger.LogWarning("UserRef not found for BuyerId={BuyerId} — skipping order confirmed notification", msg.BuyerId);
            await context.Publish<BuyerNotified>(new
            {
                msg.CorrelationId,
                msg.OrderId,
                NotifiedAt = DateTimeOffset.UtcNow
            });
            return;
        }

        var store = await userRefs.GetByStoreIdAsync(msg.StoreId, ct);
        if (store is null)
            logger.LogWarning("Seller UserRef not found for StoreId={StoreId} — store info will be empty", msg.StoreId);

        await pipeline.DispatchAsync(new NotificationRequest
        {
            UserId         = msg.BuyerId,
            EventType      = NotificationEventType.OrderConfirmed,
            IdempotencyKey = $"{NotificationEventType.OrderConfirmed}:{msg.OrderId}",
            Locale         = user.Locale,
            TemplateData   = new Dictionary<string, object>
            {
                ["orderId"]       = msg.OrderId,
                ["orderCode"]     = msg.OrderCode,
                ["buyerName"]     = user.FullName,
                ["storeName"]     = store?.StoreName    ?? string.Empty,
                ["avatarUrl"]     = store?.StoreLogoUrl ?? string.Empty,
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
