using HiveSpace.IdentityService.Domain.DomainEvents;
using MediatR;

namespace HiveSpace.IdentityService.Application.DomainEventHandlers;

public class UserCreatedDomainEventHandler() : INotificationHandler<UserCreatedDomainEvent>
{

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