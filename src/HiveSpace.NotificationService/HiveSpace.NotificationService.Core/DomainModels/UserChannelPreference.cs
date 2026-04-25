namespace HiveSpace.NotificationService.Core.DomainModels;

public class UserChannelPreference
{
    public Guid                UserId    { get; private set; }
    public NotificationChannel Channel   { get; private set; }
    public bool                Enabled   { get; private set; }
    public DateTimeOffset      UpdatedAt { get; private set; }

    protected UserChannelPreference() { }

    public static UserChannelPreference Create(Guid userId, NotificationChannel channel, bool enabled)
        => new() { UserId = userId, Channel = channel, Enabled = enabled, UpdatedAt = DateTimeOffset.UtcNow };

    public void SetEnabled(bool enabled)
    {
        Enabled   = enabled;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
