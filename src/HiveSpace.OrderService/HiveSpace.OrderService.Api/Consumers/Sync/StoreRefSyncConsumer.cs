using HiveSpace.Infrastructure.Messaging.Shared.Events.Stores;
using HiveSpace.OrderService.Domain.Enumerations;
using HiveSpace.OrderService.Domain.External;
using HiveSpace.OrderService.Infrastructure.Data;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace HiveSpace.OrderService.Api.Consumers.Sync;

public class StoreRefSyncConsumer(OrderDbContext db, ILogger<StoreRefSyncConsumer> logger)
    : IConsumer<StoreCreatedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<StoreCreatedIntegrationEvent> context)
    {
        var msg = context.Message;
        var existing = await db.StoreRefs.FindAsync([msg.Id], context.CancellationToken);
        if (existing is null)
        {
            db.StoreRefs.Add(new StoreRef(msg.Id, msg.StoreName, SellerStatus.Active, msg.OwnerId));
            logger.LogInformation("StoreRef created. StoreId={StoreId} OwnerId={OwnerId}", msg.Id, msg.OwnerId);
        }
        else
        {
            existing.Update(msg.StoreName, SellerStatus.Active);
            logger.LogInformation("StoreRef updated. StoreId={StoreId}", msg.Id);
        }
        await db.SaveChangesAsync(context.CancellationToken);
    }
}
