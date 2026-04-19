using HiveSpace.Domain.Shared.Enumerations;

namespace HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;

public record OrderReadyForFulfillment
{
    public Guid          CorrelationId  { get; init; }
    public Guid          UserId         { get; init; }
    public Guid          StoreId        { get; init; }
    public List<Guid>    ReservationIds { get; init; } = new();
    public long          GrandTotal     { get; init; }
    public PaymentMethod PaymentMethod  { get; init; } = PaymentMethod.COD;
    public string        OrderCode      { get; init; } = default!;
}
