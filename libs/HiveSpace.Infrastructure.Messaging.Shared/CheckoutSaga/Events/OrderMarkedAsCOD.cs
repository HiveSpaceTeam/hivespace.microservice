namespace HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;

public record OrderMarkedAsCOD
{
    public Guid           CorrelationId { get; init; }
    public List<Guid>     OrderIds      { get; init; } = new();
    public DateTimeOffset MarkedAt      { get; init; }
}
