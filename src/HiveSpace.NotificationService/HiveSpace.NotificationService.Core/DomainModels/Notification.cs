namespace HiveSpace.NotificationService.Core.DomainModels;

public class Notification
{
    public Guid                 Id             { get; private set; }
    public Guid                 UserId         { get; private set; }
    public NotificationChannel  Channel        { get; private set; }
    public string               EventType      { get; private set; } = default!;

    /// <summary>Stable key: "{eventType}:{sourceId}:{channel}". Unique index — prevents duplicate delivery.</summary>
    public string               IdempotencyKey { get; private set; } = default!;

    public NotificationStatus   Status         { get; private set; }

    /// <summary>JSON blob of template variables, stored for audit/replay.</summary>
    public string               Payload        { get; private set; } = "{}";

    public DateTimeOffset       CreatedAt      { get; private set; }
    public DateTimeOffset?      SentAt         { get; private set; }

    /// <summary>InApp channel only.</summary>
    public DateTimeOffset?      ReadAt         { get; private set; }

    public string?              ErrorMessage   { get; private set; }
    public int                  AttemptCount   { get; private set; }

    private readonly List<DeliveryAttempt> _attempts = new();
    public IReadOnlyCollection<DeliveryAttempt> Attempts => _attempts.AsReadOnly();

    protected Notification() { }

    public static Notification Create(
        Guid userId,
        NotificationChannel channel,
        string eventType,
        string idempotencyKey,
        string payload)
        => new()
        {
            Id             = Guid.NewGuid(),
            UserId         = userId,
            Channel        = channel,
            EventType      = eventType,
            IdempotencyKey = idempotencyKey,
            Status         = NotificationStatus.Pending,
            Payload        = payload,
            CreatedAt      = DateTimeOffset.UtcNow,
        };

    public void MarkSent()
    {
        Status = NotificationStatus.Sent;
        SentAt = DateTimeOffset.UtcNow;
    }

    public void MarkFailed(string error)
    {
        Status       = NotificationStatus.Failed;
        ErrorMessage = error;
    }

    public void MarkThrottled() => Status = NotificationStatus.Throttled;

    public void MarkDead(string error)
    {
        Status       = NotificationStatus.Dead;
        ErrorMessage = error;
    }

    public void MarkRead()
    {
        Status = NotificationStatus.Read;
        ReadAt = DateTimeOffset.UtcNow;
    }

    public void IncrementAttempt() => AttemptCount++;

    public void AddAttempt(DeliveryAttempt attempt) => _attempts.Add(attempt);
}
