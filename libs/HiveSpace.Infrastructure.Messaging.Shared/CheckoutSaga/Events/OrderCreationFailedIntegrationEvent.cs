using HiveSpace.Infrastructure.Messaging.Events;
namespace HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;

public record OrderCreationFailedIntegrationEvent : IntegrationEvent
{
    public Guid         CorrelationId { get; init; }
    public string       Reason        { get; init; } = null!;
    public List<string> Errors        { get; init; } = new();
}
