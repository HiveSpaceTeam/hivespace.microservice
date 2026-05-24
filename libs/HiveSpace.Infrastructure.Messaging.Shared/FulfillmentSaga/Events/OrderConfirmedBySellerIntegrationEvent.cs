using HiveSpace.Infrastructure.Messaging.Events;
namespace HiveSpace.Infrastructure.Messaging.Shared.FulfillmentSaga.Events;

public record OrderConfirmedBySellerIntegrationEvent : IntegrationEvent
{
    public Guid           CorrelationId { get; init; }
    public Guid           OrderId       { get; init; }
    public Guid           StoreId       { get; init; }
    public DateTimeOffset ConfirmedAt   { get; init; }
}
