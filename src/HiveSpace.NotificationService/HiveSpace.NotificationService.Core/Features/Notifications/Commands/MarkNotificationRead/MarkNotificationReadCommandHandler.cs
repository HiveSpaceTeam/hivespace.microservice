using HiveSpace.Application.Shared.Commands;
using HiveSpace.Application.Shared.Handlers;
using HiveSpace.Core.Contexts;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.NotificationService.Core.DomainModels;
using HiveSpace.NotificationService.Core.Exceptions;
using HiveSpace.NotificationService.Core.Interfaces;

namespace HiveSpace.NotificationService.Core.Features.Notifications.Commands.MarkNotificationRead;

public class MarkNotificationReadCommandHandler(
    INotificationRepository repo,
    IUserContext userContext) : ICommandHandler<MarkNotificationReadCommand>
{
    public async Task Handle(MarkNotificationReadCommand request, CancellationToken cancellationToken)
    {
        var notification = await repo.GetByIdAsync(request.NotificationId, cancellationToken)
            ?? throw new NotFoundException(NotificationDomainErrorCode.NotificationNotFound, nameof(Notification));

        if (notification.UserId != userContext.UserId)
            throw new ForbiddenException(NotificationDomainErrorCode.NotNotificationOwner, nameof(Notification));

        notification.MarkRead();
        await repo.SaveChangesAsync(cancellationToken);
    }
}
