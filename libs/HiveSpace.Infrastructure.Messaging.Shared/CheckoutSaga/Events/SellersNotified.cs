namespace HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;

public record SellersNotified
{
    public Guid CorrelationId { get; init; }
    public Guid OrderId       { get; init; }
    public int  SellerCount   { get; init; }
}
