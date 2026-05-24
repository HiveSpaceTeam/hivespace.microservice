using HiveSpace.Infrastructure.Messaging.Events;
namespace HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;

public record CouponUsageCommittedIntegrationEvent : IntegrationEvent
{
    public Guid CorrelationId { get; init; }
    public List<Guid> OrderIds { get; init; } = [];
    public DateTimeOffset CommittedAt { get; init; }
}
