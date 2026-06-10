using HiveSpace.Infrastructure.Messaging.Events;

namespace HiveSpace.Infrastructure.Messaging.Shared.Events.Users;

public record IdentityUserReadyIntegrationEvent : IntegrationEvent
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = default!;
    public string? UserName { get; init; }
    public string? FullName { get; init; }
    public DateTime ReadyAt { get; init; }
}
