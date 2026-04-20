namespace HiveSpace.NotificationService.Core.DomainModels;

public class UserPreference
{
    public Guid                UserId         { get; private set; }
    public NotificationChannel Channel        { get; private set; }
    public string              EventType      { get; private set; } = default!;
    public bool                Enabled        { get; private set; }

    /// <summary>Optional quiet hours as JSON: { "start": "22:00", "end": "08:00", "timezone": "Asia/Ho_Chi_Minh" }</summary>
    public string?             QuietHoursJson { get; private set; }

    public DateTimeOffset      UpdatedAt      { get; private set; }

    protected UserPreference() { }

    public static UserPreference Create(
        Guid userId, NotificationChannel channel, string eventType, bool enabled)
        => new()
        {
            UserId    = userId,
            Channel   = channel,
            EventType = eventType,
            Enabled   = enabled,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

    public void Update(bool enabled, string? quietHoursJson)
    {
        Enabled        = enabled;
        QuietHoursJson = quietHoursJson;
        UpdatedAt      = DateTimeOffset.UtcNow;
    }
}
