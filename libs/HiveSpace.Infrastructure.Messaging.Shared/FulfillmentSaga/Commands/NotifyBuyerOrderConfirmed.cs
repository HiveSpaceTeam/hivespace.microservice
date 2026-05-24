namespace HiveSpace.Infrastructure.Messaging.Shared.FulfillmentSaga.Commands;

public record NotifyBuyerOrderConfirmed
{
    public Guid   CorrelationId { get; init; }
    public Guid   OrderId       { get; init; }
    public Guid   BuyerId       { get; init; }
    public Guid   StoreId       { get; init; }
    public string OrderCode     { get; init; } = default!;
}
