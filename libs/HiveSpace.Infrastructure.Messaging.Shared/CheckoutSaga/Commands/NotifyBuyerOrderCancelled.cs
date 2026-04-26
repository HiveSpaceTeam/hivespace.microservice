namespace HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Commands;

public record NotifyBuyerOrderCancelled
{
    public Guid   CorrelationId { get; init; }
    public Guid   OrderId       { get; init; }
    public Guid   BuyerId       { get; init; }
    public long   RefundAmount  { get; init; }
    public string OrderCode     { get; init; } = default!;
}
