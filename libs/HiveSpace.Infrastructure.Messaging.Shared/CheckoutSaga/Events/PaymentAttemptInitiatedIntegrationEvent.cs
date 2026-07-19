using HiveSpace.Infrastructure.Messaging.Events;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Commands;

namespace HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;

public record PaymentAttemptInitiatedIntegrationEvent : IntegrationEvent
{
    public Guid CorrelationId { get; init; }
    public Guid PaymentId { get; init; }
    public Guid PaymentAttemptId { get; init; }
    public int AttemptNo { get; init; }
    public string ReferenceNo { get; init; } = string.Empty;
    public Guid BuyerId { get; init; }
    public string MethodCode { get; init; } = string.Empty;
    public long Amount { get; init; }
    public string CurrencyCode { get; init; } = string.Empty;
    public string? RedirectUrl { get; init; }
    public IReadOnlyList<CheckoutPaymentOrderDto> Orders { get; init; } = [];
    public DateTimeOffset ExpiresAt { get; init; }
}
