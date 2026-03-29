namespace HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Commands;

public record CancelOrder
{
    public Guid   CorrelationId { get; init; }
    public Guid   OrderId       { get; init; }
    public string Reason        { get; init; } = null!;
}
