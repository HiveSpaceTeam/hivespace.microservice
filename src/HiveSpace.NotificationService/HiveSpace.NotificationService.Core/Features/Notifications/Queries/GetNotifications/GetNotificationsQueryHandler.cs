using HiveSpace.Application.Shared.Handlers;
using HiveSpace.Application.Shared.Queries;
using HiveSpace.Core.Contexts;
using HiveSpace.NotificationService.Core.Features.Notifications.Dtos;
using HiveSpace.NotificationService.Core.Interfaces;

namespace HiveSpace.NotificationService.Core.Features.Notifications.Queries.GetNotifications;

public class GetNotificationsQueryHandler(
    INotificationRepository repo,
    IUserContext userContext) : IQueryHandler<GetNotificationsQuery, GetNotificationsResponse>
{
    public async Task<GetNotificationsResponse> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        var (items, hasMore) = await repo.GetByUserAsync(
            userContext.UserId, request.Page, request.PageSize, request.UnreadOnly, cancellationToken);

        var dtos = items.Select(n => new NotificationDto
        {
            Id        = n.Id,
            Channel   = n.Channel,
            EventType = n.EventType,
            Status    = n.Status,
            Payload   = n.Payload,
            CreatedAt = n.CreatedAt,
            ReadAt    = n.ReadAt,
        }).ToList();

        return new GetNotificationsResponse(dtos, hasMore);
    }
}
