using HiveSpace.Infrastructure.Messaging.Shared.Events.Stores;
using HiveSpace.NotificationService.Core.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace HiveSpace.NotificationService.Api.Consumers.Sync;

public class StoreSyncConsumer(
    IUserRefRepository          userRefs,
    ILogger<StoreSyncConsumer>  logger) : IConsumer<StoreCreatedIntegrationEvent>, IConsumer<StoreUpdatedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<StoreCreatedIntegrationEvent> context)
        => await UpsertAsync(
            context.Message.Id,
            context.Message.OwnerId,
            context.Message.StoreName,
            context.Message.LogoUrl,
            context.CancellationToken);

    public async Task Consume(ConsumeContext<StoreUpdatedIntegrationEvent> context)
        => await UpsertAsync(
            context.Message.Id,
            context.Message.OwnerId,
            context.Message.StoreName,
            context.Message.LogoUrl,
            context.CancellationToken);

    private async Task UpsertAsync(
        Guid storeId,
        Guid ownerId,
        string storeName,
        string? logoUrl,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Syncing store info on UserRef. OwnerId={OwnerId} StoreId={StoreId}",
            ownerId, storeId);

        var userRef = await userRefs.GetByIdAsync(ownerId, cancellationToken);
        if (userRef is null)
        {
            logger.LogWarning(
                "UserRef not found for OwnerId={OwnerId}. Store sync skipped.", ownerId);
            return;
        }

        userRef.UpdateStore(storeId, storeName, logoUrl);
        await userRefs.UpsertAsync(userRef, cancellationToken);
    }
}
