namespace HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Commands;

public record NotifySellerNewOrder
{
    public Guid   CorrelationId { get; init; }
    public Guid   OrderId       { get; init; }
    public Guid   StoreId       { get; init; }
    public Guid   SellerId      { get; init; }
    public Guid   BuyerId       { get; init; }
    public string OrderCode     { get; init; } = default!;
}
