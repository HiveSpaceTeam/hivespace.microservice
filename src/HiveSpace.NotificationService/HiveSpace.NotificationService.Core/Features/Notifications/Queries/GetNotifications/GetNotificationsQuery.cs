using HiveSpace.Application.Shared.Queries;
using HiveSpace.NotificationService.Core.Features.Notifications.Dtos;

namespace HiveSpace.NotificationService.Core.Features.Notifications.Queries.GetNotifications;

public record GetNotificationsQuery(int Page, int PageSize, bool UnreadOnly) : IQuery<GetNotificationsResponse>;
