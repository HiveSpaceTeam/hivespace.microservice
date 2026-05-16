using HiveSpace.Infrastructure.Messaging.Shared.Events.Stores;
using HiveSpace.OrderService.Domain.Enumerations;
using HiveSpace.OrderService.Domain.External;
using HiveSpace.OrderService.Infrastructure.Data;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace HiveSpace.OrderService.Api.Consumers.Sync;

public class StoreRefSyncConsumer(OrderDbContext db, ILogger<StoreRefSyncConsumer> logger)
    : IConsumer<StoreCreatedIntegrationEvent>,
      IConsumer<StoreUpdatedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<StoreCreatedIntegrationEvent> context)
        => await UpsertStoreRefAsync(
            context.Message.Id,
            context.Message.OwnerId,
            context.Message.StoreName,
            context.Message.LogoUrl,
            context.CancellationToken);

    public async Task Consume(ConsumeContext<StoreUpdatedIntegrationEvent> context)
        => await UpsertStoreRefAsync(
            context.Message.Id,
            context.Message.OwnerId,
            context.Message.StoreName,
            context.Message.LogoUrl,
            context.CancellationToken);

    private async Task UpsertStoreRefAsync(
        Guid storeId,
        Guid ownerId,
        string storeName,
        string? logoUrl,
        CancellationToken cancellationToken)
    {
        var existing = await db.StoreRefs.FindAsync([storeId], cancellationToken);
        if (existing is null)
        {
            db.StoreRefs.Add(new StoreRef(storeId, storeName, logoUrl, SellerStatus.Active, ownerId));
            logger.LogInformation("StoreRef created. StoreId={StoreId} OwnerId={OwnerId}", storeId, ownerId);
        }
        else
        {
            existing.Update(storeName, logoUrl, SellerStatus.Active);
            logger.LogInformation("StoreRef updated. StoreId={StoreId}", storeId);
        }
        await db.SaveChangesAsync(cancellationToken);
    }
}
