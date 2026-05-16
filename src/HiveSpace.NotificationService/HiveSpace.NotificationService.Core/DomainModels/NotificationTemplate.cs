using HiveSpace.Domain.Shared.Enumerations;

namespace HiveSpace.NotificationService.Core.DomainModels;

public class NotificationTemplate
{
    public string              EventType    { get; private set; } = default!;
    public NotificationChannel Channel      { get; private set; }
    public Culture             Locale       { get; private set; } = Culture.Vi;

    /// <summary>Used as email subject and push notification title.</summary>
    public string              Subject      { get; private set; } = default!;

    /// <summary>Scriban template for the notification body.</summary>
    public string              BodyTemplate { get; private set; } = default!;

    public DateTimeOffset      UpdatedAt    { get; private set; }

    protected NotificationTemplate() { }

    public static NotificationTemplate Create(
        string eventType, NotificationChannel channel, Culture locale,
        string subject, string bodyTemplate)
        => new()
        {
            EventType    = eventType,
            Channel      = channel,
            Locale       = locale,
            Subject      = subject,
            BodyTemplate = bodyTemplate,
            UpdatedAt    = DateTimeOffset.UtcNow,
        };

    public void Update(string subject, string bodyTemplate)
    {
        Subject      = subject;
        BodyTemplate = bodyTemplate;
        UpdatedAt    = DateTimeOffset.UtcNow;
    }
}
