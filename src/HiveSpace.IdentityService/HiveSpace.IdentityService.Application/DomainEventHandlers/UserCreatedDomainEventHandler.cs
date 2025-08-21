using HiveSpace.IdentityService.Domain.DomainEvents;
using MediatR;

namespace HiveSpace.IdentityService.Application.DomainEventHandlers;

public class UserCreatedDomainEventHandler() : INotificationHandler<UserCreatedDomainEvent>
{

    /// <summary>
    /// Handles a <see cref="UserCreatedDomainEvent"/> raised when a user account is created.
    /// </summary>
    /// <param name="notification">The domain event containing the created user's details.</param>
    /// <param name="cancellationToken">Cancellation token to observe while handling the event.</param>
    /// <returns>A task that represents the asynchronous handling operation.</returns>
    public async Task Handle(UserCreatedDomainEvent notification, CancellationToken cancellationToken)
    {


        //if (notification.UserId != Guid.Empty)
        //{
        //   throw new ArgumentException("UserId cannot be empty.", nameof(notification.UserId));
        //}
        // TODO: Add additional logic here (e.g., send welcome email, audit, etc.)

        await Task.CompletedTask;
    }
}