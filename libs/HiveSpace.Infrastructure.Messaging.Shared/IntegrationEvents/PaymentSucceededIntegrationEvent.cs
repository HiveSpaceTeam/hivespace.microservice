using HiveSpace.Infrastructure.Messaging.Events;

namespace HiveSpace.Infrastructure.Messaging.Shared.IntegrationEvents;

public record PaymentSucceededIntegrationEvent : IntegrationEvent
{
    public Guid SagaCorrelationId { get; init; }   // CheckoutSaga.CorrelationId — for saga correlation
    public Guid PaymentId { get; init; }
    public Guid OrderId { get; init; }
    public Guid BuyerId { get; init; }
    public long Amount { get; init; }
    public string Currency { get; init; } = null!;
    public DateTimeOffset PaidAt { get; init; }
}
