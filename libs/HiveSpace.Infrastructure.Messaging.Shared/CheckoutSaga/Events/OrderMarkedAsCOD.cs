namespace HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;

public record OrderMarkedAsCOD
{
    public Guid           CorrelationId { get; init; }
    public Guid           OrderId       { get; init; }
    public DateTimeOffset MarkedAt      { get; init; }
}
