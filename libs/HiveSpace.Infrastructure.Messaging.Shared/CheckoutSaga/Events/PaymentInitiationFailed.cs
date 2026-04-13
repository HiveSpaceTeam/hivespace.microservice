namespace HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;

public record PaymentInitiationFailed
{
    public Guid CorrelationId { get; init; }
    public string Reason { get; init; } = null!;
}
