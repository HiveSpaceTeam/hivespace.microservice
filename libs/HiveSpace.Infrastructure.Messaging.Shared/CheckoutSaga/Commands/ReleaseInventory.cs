namespace HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Commands;

public record ReleaseInventory
{
    public Guid       CorrelationId  { get; init; }
    public Guid       OrderId        { get; init; }
    public List<Guid> ReservationIds { get; init; } = new();
}
