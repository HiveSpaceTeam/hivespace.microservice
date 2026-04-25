using HiveSpace.NotificationService.Core.DomainModels;

namespace HiveSpace.NotificationService.Core.Dtos;

public record NotificationDto
{
    public Guid Id { get; init; }
    public NotificationChannel Channel { get; init; }
    public string EventType { get; init; } = string.Empty;
    public NotificationStatus Status { get; init; }
    public string Payload { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? ReadAt { get; init; }
}
