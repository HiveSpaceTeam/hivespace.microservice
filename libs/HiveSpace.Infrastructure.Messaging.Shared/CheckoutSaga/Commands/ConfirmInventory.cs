namespace HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Commands;

public record ConfirmInventory
{
    public Guid       CorrelationId  { get; init; }
    public Guid       OrderId        { get; init; }
    public List<Guid> ReservationIds { get; init; } = new();
}
