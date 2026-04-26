using HiveSpace.Application.Shared.Commands;

namespace HiveSpace.NotificationService.Core.Features.Notifications.Commands.MarkNotificationRead;

public record MarkNotificationReadCommand(Guid NotificationId) : ICommand;
