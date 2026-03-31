namespace HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Commands;

public record NotifySeller
{
    public Guid CorrelationId { get; init; }
    public Guid OrderId       { get; init; }
    public Guid StoreId       { get; init; }
}
