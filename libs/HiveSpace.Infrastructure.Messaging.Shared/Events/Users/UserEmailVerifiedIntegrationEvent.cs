using HiveSpace.Infrastructure.Messaging.Events;
using HiveSpace.Domain.Shared.Enumerations;

namespace HiveSpace.Infrastructure.Messaging.Shared.Events.Users;

public record UserEmailVerifiedIntegrationEvent : IntegrationEvent
{
    public Guid   UserId  { get; init; }
    public string ToEmail { get; init; } = default!;
    public string ToName  { get; init; } = default!;
    public Culture Locale { get; init; } = Culture.Vi;
}
