using HiveSpace.IdentityService.Domain.DomainEvents;
using MediatR;

namespace HiveSpace.IdentityService.Application.DomainEventHandlers;

public class UserCreatedDomainEventHandler() : INotificationHandler<UserCreatedDomainEvent>
{

    /// <summary>
    /// Handles a UserCreatedDomainEvent.
    /// </summary>
    /// <remarks>
    /// Current implementation is a no-op placeholder; replace with real logic (e.g., send welcome email, audit) as needed.
    /// </remarks>
    /// <param name="notification">The domain event containing the created user's details.</param>
    /// <param name="cancellationToken">Token to observe for cancellation of the asynchronous operation.</param>
    /// <returns>A task that completes when event handling is finished.</returns>
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