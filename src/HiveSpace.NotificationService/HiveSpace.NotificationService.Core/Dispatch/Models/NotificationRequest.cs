using HiveSpace.Domain.Shared.Enumerations;

namespace HiveSpace.NotificationService.Core.Dispatch.Models;

public class NotificationRequest
{
    public Guid   UserId         { get; init; }
    public string EventType      { get; init; } = default!;

    /// <summary>Stable, deterministic key: "{eventType}:{sourceId}"</summary>
    public string IdempotencyKey { get; init; } = default!;

    public Dictionary<string, object> TemplateData { get; init; } = new();

    /// <summary>Notification locale. Falls back to Vietnamese if template not found.</summary>
    public Culture Locale { get; init; } = Culture.Vi;

    /// <summary>
    /// When true, skips user preference checks and always sends to Email channel.
    /// Use for transactional notifications (verification, password reset) that must
    /// be delivered regardless of opt-out preferences.
    /// </summary>
    public bool IsTransactional { get; init; } = false;
}
