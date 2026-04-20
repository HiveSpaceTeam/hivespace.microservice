namespace HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Commands;

public record NotifyCustomerOrderConfirmed
{
    public Guid   CorrelationId { get; init; }
    public Guid   OrderId       { get; init; }
    public Guid   UserId        { get; init; }
    public Guid   StoreId       { get; init; }
    public string OrderCode     { get; init; } = default!;
}
