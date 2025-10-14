using HiveSpace.Domain.Shared.Interfaces;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Domain.DomainEvents;
using HiveSpace.UserService.Domain.Repositories;
using HiveSpace.UserService.Domain.Services;
using MediatR;

namespace HiveSpace.UserService.Application.DomainEventHandlers;

public class StoreCreatedDomainEventHandler : INotificationHandler<StoreCreatedDomainEvent>
{
    private readonly IUserRepository _userRepository;
    private readonly UserManager _userManager;

    public StoreCreatedDomainEventHandler(IUserRepository userRepository, UserManager userManager)
    {
        _userRepository = userRepository;
        _userManager = userManager;
    }

    public async Task Handle(StoreCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        // Get the user who created the store
        var user = await _userRepository.GetByIdAsync(notification.OwnerId);
        if (user is null)
            return; // User not found, nothing to update

        // Set store ID and seller role
        user.AssignStore(notification.StoreId);

        // Save changes
        await _userRepository.UpdateUserAsync(user, cancellationToken);
    }
}