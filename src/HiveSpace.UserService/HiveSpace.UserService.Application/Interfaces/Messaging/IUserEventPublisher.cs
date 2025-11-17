using HiveSpace.UserService.Domain.Aggregates.User;

namespace HiveSpace.UserService.Application.Interfaces.Messaging;

public interface IUserEventPublisher
{
    Task PublishUserCreatedAsync(User user, CancellationToken cancellationToken = default);
    Task PublishUserUpdatedAsync(User user, CancellationToken cancellationToken = default);
    Task PublishUserDeletedAsync(Guid userId, string? deletedBy, CancellationToken cancellationToken = default);
}

