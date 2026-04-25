namespace HiveSpace.NotificationService.Core.DomainModels;

public class UserGroupPreference
{
    public Guid                UserId     { get; private set; }
    public NotificationChannel Channel    { get; private set; }
    public string              EventGroup { get; private set; } = default!;
    public bool                Enabled    { get; private set; }
    public DateTimeOffset      UpdatedAt  { get; private set; }

    protected UserGroupPreference() { }

    public static UserGroupPreference Create(
        Guid userId, NotificationChannel channel, string eventGroup, bool enabled)
        => new()
        {
            UserId     = userId,
            Channel    = channel,
            EventGroup = eventGroup,
            Enabled    = enabled,
            UpdatedAt  = DateTimeOffset.UtcNow,
        };

    public void SetEnabled(bool enabled)
    {
        Enabled   = enabled;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
