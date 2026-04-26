using HiveSpace.Infrastructure.Messaging.Shared.IntegrationEvents;
using HiveSpace.PaymentService.Application.Interfaces.Messaging;
using HiveSpace.PaymentService.Domain.Aggregates.Payments;
using MassTransit;

namespace HiveSpace.PaymentService.Infrastructure.Messaging.Publishers;

public class PaymentEventPublisher(IPublishEndpoint publishEndpoint) : IPaymentEventPublisher
{
    public async Task PublishPaymentSucceededAsync(Payment payment, Guid sagaCorrelationId, CancellationToken cancellationToken = default)
    {
        await publishEndpoint.Publish(new PaymentSucceededIntegrationEvent
        {
            SagaCorrelationId = sagaCorrelationId,
            PaymentId = payment.Id,
            OrderId = payment.OrderId,
            BuyerId = payment.BuyerId,
            Amount = payment.Amount.Amount,
            Currency = payment.Amount.Currency.ToString(),
            PaidAt = payment.PaidAt ?? DateTimeOffset.UtcNow
        }, cancellationToken);
    }

    public async Task PublishPaymentFailedAsync(Payment payment, Guid sagaCorrelationId, CancellationToken cancellationToken = default)
    {
        await publishEndpoint.Publish(new PaymentFailedIntegrationEvent
        {
            SagaCorrelationId = sagaCorrelationId,
            PaymentId = payment.Id,
            OrderId = payment.OrderId,
            BuyerId = payment.BuyerId,
            Reason = payment.GatewayResponse?.ErrorMessage ?? "Payment failed"
        }, cancellationToken);
    }
}
