using HiveSpace.Infrastructure.Messaging.Events;
namespace HiveSpace.Infrastructure.Messaging.Shared.FulfillmentSaga.Events;

public record OrderRejectedBySellerIntegrationEvent : IntegrationEvent
{
    public Guid   CorrelationId { get; init; }
    public Guid   OrderId       { get; init; }
    public string Reason        { get; init; } = null!;
    public long   OrderAmount   { get; init; }
}
