namespace HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;

public record InventoryReserved
{
    public Guid                          CorrelationId         { get; init; }
    public Guid                          OrderId               { get; init; }
    public List<Guid>                    ReservationIds        { get; init; } = new();
    public DateTimeOffset                ExpiresAt             { get; init; }
    public Dictionary<Guid, List<Guid>>  PackageReservationMap { get; init; } = new();
}
