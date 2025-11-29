using HiveSpace.Infrastructure.Messaging.Events;
namespace HiveSpace.Application.Shared.Events.Users;
public record UserCreatedIntegrationEvent : IntegrationEvent
{
    public UserCreatedIntegrationEvent(Guid userId, string email, string? phoneNumber)
    {
        UserId = userId;
        Email = email;
        PhoneNumber = phoneNumber;
    }

    public UserCreatedIntegrationEvent()
        : this(Guid.Empty, string.Empty, null)
    {
    }

    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
}

