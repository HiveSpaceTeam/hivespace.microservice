namespace HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;

public record MarkOrderAsPaidFailed
{
    public Guid   CorrelationId { get; init; }
    public Guid   OrderId       { get; init; }
    public string Reason        { get; init; } = null!;
}
