namespace HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Commands;

public record NotifyCustomerOrderCancelled
{
    public Guid   CorrelationId { get; init; }
    public Guid   OrderId       { get; init; }
    public Guid   UserId        { get; init; }
    public long   RefundAmount  { get; init; }
    public string OrderCode     { get; init; } = default!;
}
