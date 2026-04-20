using HiveSpace.Infrastructure.Messaging.Shared.Events.Stores;
using HiveSpace.NotificationService.Core.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace HiveSpace.NotificationService.Api.Consumers.Sync;

public class StoreSyncConsumer(
    IUserRefRepository          userRefs,
    ILogger<StoreSyncConsumer>  logger) : IConsumer<StoreCreatedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<StoreCreatedIntegrationEvent> context)
    {
        var msg = context.Message;
        logger.LogInformation("Syncing store info on UserRef. OwnerId={OwnerId} StoreId={StoreId}",
            msg.OwnerId, msg.Id);

        var userRef = await userRefs.GetByIdAsync(msg.OwnerId, context.CancellationToken);
        if (userRef is null)
        {
            logger.LogWarning(
                "UserRef not found for OwnerId={OwnerId}. Store sync skipped.", msg.OwnerId);
            return;
        }

        userRef.UpdateStore(msg.Id, msg.StoreName, msg.LogoUrl);
        await userRefs.UpsertAsync(userRef, context.CancellationToken);
    }
}
