using HiveSpace.Application.Shared.Handlers;
using HiveSpace.Application.Shared.Queries;
using HiveSpace.Core.Contexts;
using HiveSpace.NotificationService.Core.Interfaces;

namespace HiveSpace.NotificationService.Core.Features.Notifications.Queries.GetUnreadCount;

public class GetUnreadCountQueryHandler(
    INotificationRepository repo,
    IUserContext userContext) : IQueryHandler<GetUnreadCountQuery, int>
{
    public Task<int> Handle(GetUnreadCountQuery request, CancellationToken cancellationToken)
        => repo.CountUnreadInAppAsync(userContext.UserId, cancellationToken);
}
