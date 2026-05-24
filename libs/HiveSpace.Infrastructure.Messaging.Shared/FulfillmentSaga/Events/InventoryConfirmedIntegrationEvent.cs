using HiveSpace.Infrastructure.Messaging.Events;
namespace HiveSpace.Infrastructure.Messaging.Shared.FulfillmentSaga.Events;

public record InventoryConfirmedIntegrationEvent : IntegrationEvent
{
    public Guid       CorrelationId  { get; init; }
    public Guid       OrderId        { get; init; }
    public List<Guid> ReservationIds { get; init; } = new();
}
