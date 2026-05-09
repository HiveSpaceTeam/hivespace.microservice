using HiveSpace.Infrastructure.Messaging.Shared.Events.Media;
using HiveSpace.UserService.Infrastructure.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HiveSpace.UserService.Api.Consumers.Sync;

public class MediaAssetProcessedConsumer(UserDbContext db, ILogger<MediaAssetProcessedConsumer> logger)
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

    private async Task HandleUserAvatarAsync(Guid fileId, string publicUrl, CancellationToken ct)
    {
        var fileIdStr = fileId.ToString();
        var appUser = await db.Users.FirstOrDefaultAsync(u => u.AvatarFileId == fileIdStr, ct);
        if (appUser is null)
        {
            logger.LogWarning("No user found with AvatarFileId={FileId}", fileId);
            return;
        }

        appUser.AvatarUrl = publicUrl;
        await db.SaveChangesAsync(ct);
        logger.LogInformation("User avatar URL updated. UserId={UserId}", appUser.Id);
    }

    private async Task HandleStoreLogoAsync(Guid fileId, string publicUrl, CancellationToken ct)
    {
        var fileIdStr = fileId.ToString();
        var store = await db.Stores.FirstOrDefaultAsync(s => s.LogoFileId == fileIdStr, ct);
        if (store is null)
        {
            logger.LogWarning("No store found with LogoFileId={FileId}", fileId);
            return;
        }

        store.SetLogoUrl(publicUrl);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Store logo URL updated. StoreId={StoreId}", store.Id);
    }
}
