using HiveSpace.Infrastructure.Messaging.Events;

namespace HiveSpace.Infrastructure.Messaging.Shared.IntegrationEvents;

public record PaymentFailedIntegrationEvent : IntegrationEvent
{
    public Guid SagaCorrelationId { get; init; }   // CheckoutSaga.CorrelationId — for saga correlation
    public Guid PaymentId { get; init; }
    public Guid OrderId { get; init; }
    public Guid BuyerId { get; init; }
    public string Reason { get; init; } = null!;
}
