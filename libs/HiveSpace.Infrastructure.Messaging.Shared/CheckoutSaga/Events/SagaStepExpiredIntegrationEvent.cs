using HiveSpace.Infrastructure.Messaging.Events;
namespace HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;

public record SagaStepExpiredIntegrationEvent : IntegrationEvent
{
    public Guid CorrelationId { get; init; }
    public Guid OrderId       { get; init; }
}
