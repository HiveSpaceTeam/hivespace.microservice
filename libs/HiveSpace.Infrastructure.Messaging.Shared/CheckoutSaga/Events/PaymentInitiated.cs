namespace HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;

public record PaymentInitiated
{
    public Guid CorrelationId { get; init; }
    public Guid PaymentId { get; init; }
    public string PaymentUrl { get; init; } = null!;
    public DateTimeOffset ExpiresAt { get; init; }
}
