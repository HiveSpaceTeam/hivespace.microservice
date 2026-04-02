namespace HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Commands;

public record NotifyCustomer
{
    public Guid    CorrelationId { get; init; }
    public Guid    OrderId       { get; init; }
    public Guid    UserId        { get; init; }
    public bool    WasConfirmed  { get; init; }
    public long    RefundAmount  { get; init; }
}
