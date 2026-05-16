using HiveSpace.Domain.Shared.Enumerations;

namespace HiveSpace.Infrastructure.Messaging.Shared.Events.Users;

public record UserEmailVerificationRequestedIntegrationEvent
{
    public Guid   UserId           { get; init; }
    public string ToEmail          { get; init; } = default!;
    public string ToName           { get; init; } = default!;
    public string VerificationLink { get; init; } = default!;
    public DateTime ExpiresAt      { get; init; }
    public Culture Locale          { get; init; } = Culture.Vi;
}
