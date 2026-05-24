using HiveSpace.Infrastructure.Messaging.Events;
using HiveSpace.Domain.Shared.Enumerations;

namespace HiveSpace.Infrastructure.Messaging.Shared.Events.Users;

public record UserUpdatedIntegrationEvent : IntegrationEvent
{
    public Guid    UserId      { get; init; }
    public string  Email       { get; init; } = default!;
    public string  FullName    { get; init; } = default!;
    public string? PhoneNumber { get; init; }
    public Culture Locale      { get; init; } = Culture.Vi;
    public string? UserName    { get; init; }
    public string? AvatarUrl   { get; init; }
}
