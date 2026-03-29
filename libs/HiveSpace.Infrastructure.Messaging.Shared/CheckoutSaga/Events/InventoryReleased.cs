namespace HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;

public record InventoryReleased
{
    public Guid       CorrelationId  { get; init; }
    public Guid       OrderId        { get; init; }
    public List<Guid> ReservationIds { get; init; } = new();
}
