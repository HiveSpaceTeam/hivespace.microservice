using HiveSpace.IdentityService.Domain.DomainEvents;
using MediatR;

namespace HiveSpace.IdentityService.Application.DomainEventHandlers;

public class UserCreatedDomainEventHandler(ILogger<UserCreatedDomainEventHandler> logger) : INotificationHandler<UserCreatedDomainEvent>
{
    private readonly ILogger<UserCreatedDomainEventHandler> _logger = logger;

    public async Task Handle(UserCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        // Example: Log the event
        _logger.LogInformation(
            "User created: Id={UserId}, Email={Email}, FullName={FullName}",
            notification.UserId,
            notification.Email,
            notification.FullName);

        //if (notification.UserId != Guid.Empty)
        //{
        //   throw new ArgumentException("UserId cannot be empty.", nameof(notification.UserId));
        //}
        // TODO: Add additional logic here (e.g., send welcome email, audit, etc.)

        await Task.CompletedTask;
    }
}