using HiveSpace.Infrastructure.Messaging.Events;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Commands;
namespace HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;

public record PaymentInitiationFailedIntegrationEvent : IntegrationEvent
{
    public Guid CorrelationId { get; init; }
    public Guid? PaymentId { get; init; }
    public Guid? PaymentAttemptId { get; init; }
    public int? AttemptNo { get; init; }
    public Guid BuyerId { get; init; }
    public string MethodCode { get; init; } = string.Empty;
    public long Amount { get; init; }
    public string CurrencyCode { get; init; } = string.Empty;
    public string ReasonCode { get; init; } = string.Empty;
    public string Reason { get; init; } = null!;
    public IReadOnlyList<CheckoutPaymentOrderDto> Orders { get; init; } = [];
}
