using HiveSpace.Infrastructure.Messaging.Events;
namespace HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;

public record OrderMarkedAsPaidIntegrationEvent : IntegrationEvent
{
    public Guid           CorrelationId { get; init; }
    public List<Guid>     OrderIds      { get; init; } = [];
    public DateTimeOffset PaidAt        { get; init; }
}
