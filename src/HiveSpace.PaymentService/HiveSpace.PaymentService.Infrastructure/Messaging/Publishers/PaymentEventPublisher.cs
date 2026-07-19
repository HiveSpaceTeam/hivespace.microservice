using HiveSpace.Infrastructure.Messaging.Shared.IntegrationEvents;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Commands;
using HiveSpace.PaymentService.Application.Interfaces.Messaging;
using HiveSpace.PaymentService.Domain.Aggregates.Payments;
using MassTransit;

namespace HiveSpace.PaymentService.Infrastructure.Messaging.Publishers;

public class PaymentEventPublisher(IPublishEndpoint publishEndpoint) : IPaymentEventPublisher
{
    public async Task PublishPaymentAttemptInitiatedAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        await publishEndpoint.Publish(new PaymentAttemptInitiatedIntegrationEvent
        {
            CorrelationId = payment.CheckoutCorrelationId ?? Guid.Empty,
            PaymentId = payment.Id,
            PaymentAttemptId = payment.CurrentAttemptId ?? Guid.Empty,
            AttemptNo = payment.CurrentAttempt?.AttemptNo ?? 0,
            ReferenceNo = payment.ReferenceNo ?? string.Empty,
            BuyerId = payment.BuyerId,
            MethodCode = payment.MethodCode,
            Amount = payment.Amount.Amount,
            CurrencyCode = payment.Amount.Currency.ToString(),
            RedirectUrl = payment.CurrentAttempt?.RedirectUrl ?? payment.GatewayPaymentUrl,
            Orders = ToOrderDtos(payment),
            ExpiresAt = payment.CurrentAttempt?.ExpiresAt ?? DateTimeOffset.UtcNow
        }, cancellationToken);
    }

    public async Task PublishPaymentSucceededAsync(Payment payment, Guid sagaCorrelationId, CancellationToken cancellationToken = default)
    {
        await publishEndpoint.Publish(new PaymentSucceededIntegrationEvent
        {
            SagaCorrelationId = sagaCorrelationId,
            CorrelationId = payment.CheckoutCorrelationId ?? sagaCorrelationId,
            PaymentId = payment.Id,
            PaymentAttemptId = payment.CurrentAttemptId ?? Guid.Empty,
            AttemptNo = payment.CurrentAttempt?.AttemptNo ?? 0,
            ReferenceNo = payment.ReferenceNo ?? string.Empty,
            MethodCode = payment.MethodCode,
            OrderId = payment.OrderId,
            BuyerId = payment.BuyerId,
            Amount = payment.Amount.Amount,
            Currency = payment.Amount.Currency.ToString(),
            CurrencyCode = payment.Amount.Currency.ToString(),
            GatewayTransactionId = payment.GatewayTransactionId,
            Orders = ToOrderDtos(payment),
            PaidAt = payment.PaidAt ?? DateTimeOffset.UtcNow
        }, cancellationToken);
    }

    public async Task PublishPaymentFailedAsync(Payment payment, Guid sagaCorrelationId, CancellationToken cancellationToken = default)
    {
        await publishEndpoint.Publish(new PaymentFailedIntegrationEvent
        {
            SagaCorrelationId = sagaCorrelationId,
            CorrelationId = payment.CheckoutCorrelationId ?? sagaCorrelationId,
            PaymentId = payment.Id,
            PaymentAttemptId = payment.CurrentAttemptId ?? Guid.Empty,
            AttemptNo = payment.CurrentAttempt?.AttemptNo ?? 0,
            ReferenceNo = payment.ReferenceNo ?? string.Empty,
            MethodCode = payment.MethodCode,
            OrderId = payment.OrderId,
            BuyerId = payment.BuyerId,
            Amount = payment.Amount.Amount,
            CurrencyCode = payment.Amount.Currency.ToString(),
            FailureType = payment.Status.ToString(),
            Reason = payment.GatewayResponse?.ErrorMessage ?? "Payment failed",
            Orders = ToOrderDtos(payment)
        }, cancellationToken);
    }

    private static IReadOnlyList<CheckoutPaymentOrderDto> ToOrderDtos(Payment payment)
    {
        if (payment.LinkedOrders.Count == 0)
            return [new CheckoutPaymentOrderDto(payment.OrderId, payment.OrderId.ToString(), Guid.Empty, payment.Amount.Amount, payment.Amount.Currency.ToString())];

        return payment.LinkedOrders
            .Select(o => new CheckoutPaymentOrderDto(o.OrderId, o.OrderCode, o.StoreId, o.Amount, o.CurrencyCode))
            .ToList();
    }
}
