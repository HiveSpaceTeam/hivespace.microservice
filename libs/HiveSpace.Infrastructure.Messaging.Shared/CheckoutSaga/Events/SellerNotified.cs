namespace HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;

public record SellerNotified
{
    public Guid CorrelationId { get; init; }
    public Guid OrderId       { get; init; }
}
