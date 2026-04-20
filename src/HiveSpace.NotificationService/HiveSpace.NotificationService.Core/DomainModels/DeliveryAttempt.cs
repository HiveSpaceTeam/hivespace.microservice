namespace HiveSpace.NotificationService.Core.DomainModels;

public class DeliveryAttempt
{
    public Guid           Id               { get; private set; }
    public Guid           NotificationId   { get; private set; }
    public int            AttemptNumber    { get; private set; }
    public DateTimeOffset AttemptedAt      { get; private set; }
    public bool           Success          { get; private set; }
    public string?        ProviderResponse { get; private set; }
    public string?        ErrorMessage     { get; private set; }

    protected DeliveryAttempt() { }

    public static DeliveryAttempt Create(
        Guid notificationId, int attemptNumber,
        bool success, string? providerResponse, string? errorMessage)
        => new()
        {
            Id               = Guid.NewGuid(),
            NotificationId   = notificationId,
            AttemptNumber    = attemptNumber,
            AttemptedAt      = DateTimeOffset.UtcNow,
            Success          = success,
            ProviderResponse = providerResponse,
            ErrorMessage     = errorMessage,
        };
}
