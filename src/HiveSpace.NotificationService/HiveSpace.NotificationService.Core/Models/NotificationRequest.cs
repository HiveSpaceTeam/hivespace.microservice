namespace HiveSpace.NotificationService.Core.Models;

public class NotificationRequest
{
    public Guid   UserId         { get; init; }
    public string EventType      { get; init; } = default!;

    /// <summary>Stable, deterministic key: "{eventType}:{sourceId}"</summary>
    public string IdempotencyKey { get; init; } = default!;

    public Dictionary<string, object> TemplateData { get; init; } = new();

    /// <summary>BCP-47 locale. Falls back to "vi" if template not found.</summary>
    public string Locale { get; init; } = "vi";
}
