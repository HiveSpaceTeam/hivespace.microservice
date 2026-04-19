using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Commands;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;
using HiveSpace.NotificationService.Core.DomainModels;
using HiveSpace.NotificationService.Core.Interfaces;
using HiveSpace.NotificationService.Core.Models;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace HiveSpace.NotificationService.Api.Consumers;

public class NotifySellerNewOrderConsumer(
    IDispatchPipeline                    pipeline,
    IUserRefRepository                   userRefs,
    ILogger<NotifySellerNewOrderConsumer> logger) : IConsumer<NotifySellerNewOrder>
{
    public async Task Consume(ConsumeContext<NotifySellerNewOrder> context)
    {
        var msg = context.Message;
        var ct  = context.CancellationToken;

        if (msg.SellerId == Guid.Empty)
        {
            logger.LogWarning("SellerId is empty for OrderId={OrderId} — skipping seller notification", msg.OrderId);
            await context.Publish<SellerNewOrderNotified>(new { msg.CorrelationId, msg.OrderId });
            return;
        }

        var seller = await userRefs.GetByIdAsync(msg.SellerId, ct);
        var buyer  = await userRefs.GetByIdAsync(msg.BuyerId, ct);

        await pipeline.DispatchAsync(new NotificationRequest
        {
            UserId         = msg.SellerId,
            EventType      = NotificationEventType.NewOrderReceived,
            IdempotencyKey = $"{NotificationEventType.NewOrderReceived}:{msg.OrderId}",
            Locale         = seller?.Locale ?? "vi",
            TemplateData   = new Dictionary<string, object>
            {
                ["orderId"]   = msg.OrderId,
                ["orderCode"] = msg.OrderCode,
                ["storeId"]   = msg.StoreId,
                ["buyerName"] = buyer?.FullName ?? "Customer",
                ["avatarUrl"] = buyer?.AvatarUrl ?? string.Empty,
            }
        }, ct);

        await context.Publish<SellerNewOrderNotified>(new { msg.CorrelationId, msg.OrderId });
    }
}
