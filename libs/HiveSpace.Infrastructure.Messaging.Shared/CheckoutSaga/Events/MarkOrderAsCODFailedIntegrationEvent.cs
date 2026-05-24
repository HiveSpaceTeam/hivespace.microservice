using HiveSpace.Infrastructure.Messaging.Events;
namespace HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;

public record MarkOrderAsCODFailedIntegrationEvent : IntegrationEvent
{
    public Guid   CorrelationId { get; init; }
    public Guid   OrderId       { get; init; }
    public string Reason        { get; init; } = null!;
}
