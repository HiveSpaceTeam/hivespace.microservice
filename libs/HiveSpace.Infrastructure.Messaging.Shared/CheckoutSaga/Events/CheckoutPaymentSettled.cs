namespace HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;

public record CheckoutPaymentSettled
{
    public Guid                         CorrelationId         { get; init; }   // = OrderId
    public Guid                         UserId                { get; init; }
    public List<Guid>                   PackageIds            { get; init; } = new();
    public List<Guid>                   ReservationIds        { get; init; } = new();
    public Dictionary<Guid, List<Guid>> PackageReservationMap { get; init; } = new();
    public long                         GrandTotal            { get; init; }
}
