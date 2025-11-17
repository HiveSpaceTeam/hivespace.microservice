using HiveSpace.Infrastructure.Messaging.Abstractions;
using HiveSpace.UserService.Application.IntegrationEvents;
using HiveSpace.UserService.Application.Interfaces.Messaging;
using HiveSpace.UserService.Domain.Aggregates.User;

namespace HiveSpace.UserService.Infrastructure.Messaging.Publishers;

public class UserEventPublisher : IUserEventPublisher
{
    private readonly IEventPublisher _eventPublisher;

    public UserEventPublisher(IEventPublisher eventPublisher)
    {
        _eventPublisher = eventPublisher;
    }

    public Task PublishUserCreatedAsync(User user, CancellationToken cancellationToken = default)
    {
        var evt = new UserCreatedIntegrationEvent(user.Id, user.Email.Value, user.PhoneNumber?.Value);
        return _eventPublisher.PublishAsync(evt, cancellationToken);
    }

    public Task PublishUserUpdatedAsync(User user, CancellationToken cancellationToken = default)
    {
        var evt = new UserUpdatedIntegrationEvent(user.Id, user.Email.Value, user.PhoneNumber?.Value);
        return _eventPublisher.PublishAsync(evt, cancellationToken);
    }

    public Task PublishUserDeletedAsync(Guid userId, string? deletedBy, CancellationToken cancellationToken = default)
    {
        var evt = new UserDeletedIntegrationEvent(userId, deletedBy);
        return _eventPublisher.PublishAsync(evt, cancellationToken);
    }
}

