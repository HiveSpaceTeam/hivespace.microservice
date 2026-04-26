using HiveSpace.Infrastructure.Messaging.Shared.Events.Users;
using HiveSpace.NotificationService.Core.DomainModels.External;
using HiveSpace.NotificationService.Core.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace HiveSpace.NotificationService.Api.Consumers.Sync;

public class UserSyncConsumer(
    IUserRefRepository         userRefs,
    ILogger<UserSyncConsumer>  logger) : IConsumer<UserCreatedIntegrationEvent>, IConsumer<UserUpdatedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<UserCreatedIntegrationEvent> context)
    {
        var msg = context.Message;
        logger.LogInformation("Syncing UserRef for UserId={UserId}", msg.UserId);

        var userRef = UserRef.Create(msg.UserId, msg.Email, msg.FullName, msg.PhoneNumber, msg.Locale,
            msg.UserName, msg.AvatarUrl);
        await userRefs.UpsertAsync(userRef, context.CancellationToken);
    }

    public async Task Consume(ConsumeContext<UserUpdatedIntegrationEvent> context)
    {
        var msg = context.Message;
        logger.LogInformation("Updating UserRef for UserId={UserId}", msg.UserId);

        var existing = await userRefs.GetByIdAsync(msg.UserId, context.CancellationToken);
        if (existing is not null)
        {
            existing.Update(msg.Email, msg.FullName, msg.PhoneNumber, msg.Locale,
                msg.UserName, msg.AvatarUrl);
            await userRefs.UpsertAsync(existing, context.CancellationToken);
        }
        else
        {
            var userRef = UserRef.Create(msg.UserId, msg.Email, msg.FullName, msg.PhoneNumber, msg.Locale,
                msg.UserName, msg.AvatarUrl);
            await userRefs.UpsertAsync(userRef, context.CancellationToken);
        }
    }
}
