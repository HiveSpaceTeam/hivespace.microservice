using HiveSpace.Infrastructure.Messaging.Events;
namespace HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;

public record PaymentInitiationFailedIntegrationEvent : IntegrationEvent
{
    public Guid CorrelationId { get; init; }
    public string Reason { get; init; } = null!;
}
