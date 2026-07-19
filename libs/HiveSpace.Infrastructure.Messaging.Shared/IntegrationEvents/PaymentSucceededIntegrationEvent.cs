using HiveSpace.Infrastructure.Messaging.Events;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Commands;

namespace HiveSpace.Infrastructure.Messaging.Shared.IntegrationEvents;

public record PaymentSucceededIntegrationEvent : IntegrationEvent
{
    public Guid SagaCorrelationId { get; init; }
    public Guid CorrelationId { get; init; }
    public Guid PaymentId { get; init; }
    public Guid PaymentAttemptId { get; init; }
    public int AttemptNo { get; init; }
    public string ReferenceNo { get; init; } = string.Empty;
    public string MethodCode { get; init; } = string.Empty;
    public Guid OrderId { get; init; }
    public Guid BuyerId { get; init; }
    public long Amount { get; init; }
    public string Currency { get; init; } = null!;
    public string CurrencyCode { get; init; } = string.Empty;
    public string? GatewayTransactionId { get; init; }
    public IReadOnlyList<CheckoutPaymentOrderDto> Orders { get; init; } = [];
    public DateTimeOffset PaidAt { get; init; }
}
