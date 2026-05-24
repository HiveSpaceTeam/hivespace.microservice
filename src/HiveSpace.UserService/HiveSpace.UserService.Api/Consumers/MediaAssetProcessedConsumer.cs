using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Infrastructure.Messaging.Shared.Events.Media;
using HiveSpace.UserService.Application.Interfaces.Messaging;
using HiveSpace.UserService.Domain.Aggregates.Store;
using HiveSpace.UserService.Domain.Exceptions;
using HiveSpace.UserService.Infrastructure.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HiveSpace.UserService.Api.Consumers;

public class MediaAssetProcessedConsumer(
    UserDbContext db,
    IStoreEventPublisher storeEventPublisher,
    ILogger<MediaAssetProcessedConsumer> logger)
    : IConsumer<MediaAssetProcessedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<MediaAssetProcessedIntegrationEvent> context)
    {
        var msg = context.Message;

        switch (msg.EntityType)
        {
            case "user_avatar":
                await HandleUserAvatarAsync(msg.FileId, msg.PublicUrl, context.CancellationToken);
                break;
            case "store_logo":
                await HandleStoreLogoAsync(msg.FileId, msg.PublicUrl, context.CancellationToken);
                break;
            default:
                logger.LogDebug("MediaAssetProcessedConsumer: unhandled EntityType={EntityType}", msg.EntityType);
                break;
        }
    }

    private async Task HandleUserAvatarAsync(string fileId, string publicUrl, CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.AvatarFileId == fileId, ct);
        if (user is null)
            throw new NotFoundException(UserDomainErrorCode.UserNotFound, nameof(HiveSpace.UserService.Domain.Aggregates.User.User));

        user.SetAvatarUrl(publicUrl);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("User avatar URL updated. UserId={UserId}", user.Id);
    }

    private async Task HandleStoreLogoAsync(string fileId, string publicUrl, CancellationToken ct)
    {
        var store = await db.Stores.FirstOrDefaultAsync(s => s.LogoFileId == fileId, ct);
        if (store is null)
            throw new NotFoundException(UserDomainErrorCode.StoreNotFound, nameof(Store));

        store.SetLogoUrl(publicUrl);
        await storeEventPublisher.PublishStoreUpdatedAsync(store, ct);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Store logo URL updated. StoreId={StoreId}", store.Id);
    }
}
