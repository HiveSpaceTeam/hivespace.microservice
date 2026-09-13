using HiveSpace.Infrastructure.Messaging.Abstractions;
using HiveSpace.Infrastructure.Messaging.Shared.Events.Users;
using HiveSpace.UserService.Application.Interfaces.Messaging;
using HiveSpace.UserService.Domain.Aggregates.User;

namespace HiveSpace.UserService.Infrastructure.Messaging.Publishers;

public class UserEventPublisher(IEventPublisher eventPublisher) : IUserEventPublisher
{
    public Task PublishUserCreatedAsync(User user, CancellationToken cancellationToken = default)
        => eventPublisher.PublishAsync(ToCreatedEvent(user), cancellationToken);

    public Task PublishUserUpdatedAsync(User user, CancellationToken cancellationToken = default)
        => eventPublisher.PublishAsync(ToUpdatedEvent(user), cancellationToken);

    private static UserCreatedIntegrationEvent ToCreatedEvent(User user)
        => new()
        {
            UserId = user.Id,
            Email = user.Email.Value,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber?.Value,
            Locale = user.Settings.Culture,
            UserName = user.UserName,
            AvatarUrl = user.AvatarUrl
        };

    private static UserUpdatedIntegrationEvent ToUpdatedEvent(User user)
        => new()
        {
            UserId = user.Id,
            Email = user.Email.Value,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber?.Value,
            Locale = user.Settings.Culture,
            UserName = user.UserName,
            AvatarUrl = user.AvatarUrl
        };
}
