using HiveSpace.CatalogService.Domain.Aggregates.External;
using HiveSpace.CatalogService.Domain.Repositories.External;
using HiveSpace.Infrastructure.Messaging.Shared.Events.Stores;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace HiveSpace.CatalogService.Api.Consumers.Sync;

public class StoreRefSyncConsumer(
    IStoreRefRepository storeRefs,
    ILogger<StoreRefSyncConsumer> logger)
    : IConsumer<StoreCreatedIntegrationEvent>,
      IConsumer<StoreUpdatedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<StoreCreatedIntegrationEvent> context)
        => await UpsertAsync(
            context.Message.Id,
            context.Message.OwnerId,
            context.Message.StoreName,
            context.Message.Description,
            context.Message.LogoUrl,
            context.Message.Address,
            context.CancellationToken);

    public async Task Consume(ConsumeContext<StoreUpdatedIntegrationEvent> context)
        => await UpsertAsync(
            context.Message.Id,
            context.Message.OwnerId,
            context.Message.StoreName,
            context.Message.Description,
            context.Message.LogoUrl,
            context.Message.Address,
            context.CancellationToken);

    private async Task UpsertAsync(
        Guid storeId,
        Guid ownerId,
        string storeName,
        string? description,
        string? logoUrl,
        string address,
        CancellationToken cancellationToken)
    {
        var existing = await storeRefs.GetByIdAsync(storeId, cancellationToken);
        if (existing is null)
        {
            var now = DateTimeOffset.UtcNow;
            await storeRefs.AddAsync(
                new StoreRef(storeId, ownerId, storeName, description, logoUrl, address, now, now),
                cancellationToken);
            logger.LogInformation("StoreRef created. StoreId={StoreId}", storeId);
        }
        else
        {
            existing.Update(storeName, description, logoUrl, address);
            await storeRefs.SaveChangesAsync(cancellationToken);
            logger.LogInformation("StoreRef updated. StoreId={StoreId}", storeId);
        }
    }
}
