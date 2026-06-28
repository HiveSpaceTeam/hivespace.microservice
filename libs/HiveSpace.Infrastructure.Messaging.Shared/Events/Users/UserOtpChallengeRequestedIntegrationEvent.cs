using HiveSpace.Infrastructure.Messaging.Events;

namespace HiveSpace.Infrastructure.Messaging.Shared.Events.Users;

public record UserOtpChallengeRequestedIntegrationEvent : IntegrationEvent
{
    public string RecipientEmail { get; init; } = default!;
    public string OtpCode { get; init; } = default!;
    public DateTimeOffset ExpiresAt { get; init; }
    public string Purpose { get; init; } = default!;
}
