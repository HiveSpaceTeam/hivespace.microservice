using HiveSpace.Infrastructure.Messaging.Events;
namespace HiveSpace.Application.Shared.Events.Users;
public record UserDeletedIntegrationEvent : IntegrationEvent
{
    public UserDeletedIntegrationEvent(Guid userId, string? deletedBy)
    {
        UserId = userId;
        DeletedBy = deletedBy;
    }

    public UserDeletedIntegrationEvent()
        : this(Guid.Empty, null)
    {
    }

    public Guid UserId { get; init; }
    public string? DeletedBy { get; init; }
}

