using HiveSpace.Infrastructure.Messaging.Events;
namespace HiveSpace.Infrastructure.Messaging.Shared.FulfillmentSaga.Events;

public record SellerConfirmationExpiredIntegrationEvent : IntegrationEvent
{
    public Guid CorrelationId { get; init; }
    public Guid OrderId       { get; init; }
}
