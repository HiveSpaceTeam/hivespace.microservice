namespace HiveSpace.NotificationService.Core.Features.Notifications.Dtos;

public record GetNotificationsResponse(
    List<NotificationDto> Notifications,
    bool HasMore
);
