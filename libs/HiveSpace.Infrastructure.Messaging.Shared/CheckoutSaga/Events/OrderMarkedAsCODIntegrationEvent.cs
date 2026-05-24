using HiveSpace.Infrastructure.Messaging.Events;
namespace HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;

public record OrderMarkedAsCODIntegrationEvent : IntegrationEvent
{
    public Guid           CorrelationId { get; init; }
    public List<Guid>     OrderIds      { get; init; } = new();
    public DateTimeOffset MarkedAt      { get; init; }
}
