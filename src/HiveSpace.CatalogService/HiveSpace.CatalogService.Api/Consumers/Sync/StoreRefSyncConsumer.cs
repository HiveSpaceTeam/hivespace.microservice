using HiveSpace.CatalogService.Domain.Aggregates.External;
using HiveSpace.CatalogService.Domain.Repositories.External;
using HiveSpace.Infrastructure.Messaging.Shared.Events.Stores;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace HiveSpace.CatalogService.Api.Consumers.Sync;

public class StoreRefSyncConsumer(
    IStoreRefRepository storeRefs,
    ILogger<StoreRefSyncConsumer> logger)
    : IConsumer<StoreCreatedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<StoreCreatedIntegrationEvent> context)
    {
        var msg = context.Message;
        var existing = await storeRefs.GetByIdAsync(msg.Id, context.CancellationToken);
        if (existing is null)
        {
            var now = DateTimeOffset.UtcNow;
            await storeRefs.AddAsync(
                new StoreRef(msg.Id, msg.OwnerId, msg.StoreName, msg.Description, msg.LogoUrl, msg.Address, now, now),
                context.CancellationToken);
            logger.LogInformation("StoreRef created. StoreId={StoreId}", msg.Id);
        }
        else
        {
            existing.Update(msg.StoreName, msg.Description, msg.LogoUrl, msg.Address);
            await storeRefs.SaveChangesAsync(context.CancellationToken);
            logger.LogInformation("StoreRef updated. StoreId={StoreId}", msg.Id);
        }
    }
}
