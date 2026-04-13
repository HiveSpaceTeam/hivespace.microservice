using HiveSpace.Infrastructure.Messaging.Shared.IntegrationEvents;
using HiveSpace.PaymentService.Application.Interfaces.Messaging;
using HiveSpace.PaymentService.Domain.DomainEvents;
using MassTransit;

namespace HiveSpace.PaymentService.Infrastructure.Messaging.Publishers;

public class PaymentEventPublisher(IPublishEndpoint publishEndpoint) : IPaymentEventPublisher
{
    public async Task PublishPaymentSucceededAsync(PaymentSucceededDomainEvent notification, CancellationToken cancellationToken = default)
    {
        Guid.TryParse(notification.IdempotencyKey, out var sagaCorrelationId);

        await publishEndpoint.Publish(new PaymentSucceededIntegrationEvent
        {
            SagaCorrelationId = sagaCorrelationId,
            PaymentId = notification.PaymentId,
            OrderId = notification.OrderId,
            BuyerId = notification.BuyerId,
            Amount = notification.Amount.Amount,
            Currency = notification.Amount.Currency.ToString(),
            PaidAt = DateTimeOffset.UtcNow
        }, cancellationToken);
    }

    public async Task PublishPaymentFailedAsync(PaymentFailedDomainEvent notification, Guid sagaCorrelationId, CancellationToken cancellationToken = default)
    {
        await publishEndpoint.Publish(new PaymentFailedIntegrationEvent
        {
            SagaCorrelationId = sagaCorrelationId,
            PaymentId = notification.PaymentId,
            OrderId = notification.OrderId,
            BuyerId = notification.BuyerId,
            Reason = notification.Reason
        }, cancellationToken);
    }
}
