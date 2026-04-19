using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Commands;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;
using HiveSpace.NotificationService.Core.DomainModels;
using HiveSpace.NotificationService.Core.Interfaces;
using HiveSpace.NotificationService.Core.Models;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace HiveSpace.NotificationService.Api.Consumers;

public class NotifyCustomerOrderConfirmedConsumer(
    IDispatchPipeline                           pipeline,
    IUserRefRepository                          userRefs,
    ILogger<NotifyCustomerOrderConfirmedConsumer> logger) : IConsumer<NotifyCustomerOrderConfirmed>
{
    public async Task Consume(ConsumeContext<NotifyCustomerOrderConfirmed> context)
    {
        var msg = context.Message;
        var ct  = context.CancellationToken;

        var user = await userRefs.GetByIdAsync(msg.UserId, ct);
        if (user is null)
        {
            logger.LogWarning("UserRef not found for UserId={UserId} — skipping order confirmed notification", msg.UserId);
            await context.Publish<CustomerNotified>(new
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
            UserId         = msg.UserId,
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

        await context.Publish<CustomerNotified>(new
        {
            msg.CorrelationId,
            msg.OrderId,
            NotifiedAt = DateTimeOffset.UtcNow
        });
    }
}
