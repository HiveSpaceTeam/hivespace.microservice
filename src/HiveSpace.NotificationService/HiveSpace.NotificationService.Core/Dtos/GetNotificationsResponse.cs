namespace HiveSpace.NotificationService.Core.Dtos;

public record GetNotificationsResponse(
    List<NotificationDto> Notifications,
    bool HasMore
);
