using HiveSpace.Infrastructure.Messaging.Events;
namespace HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;

public record PaymentTimeoutIntegrationEvent : IntegrationEvent
{
    public Guid CorrelationId { get; init; }
}
