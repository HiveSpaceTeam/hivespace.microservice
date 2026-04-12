namespace HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;

public record PaymentTimeout
{
    public Guid CorrelationId { get; init; }
}
