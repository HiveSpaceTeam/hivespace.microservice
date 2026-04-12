using HiveSpace.Infrastructure.Messaging.Shared.IntegrationEvents;
using HiveSpace.PaymentService.Domain.DomainEvents;
using MassTransit;
using MediatR;

namespace HiveSpace.PaymentService.Application.Payments.EventHandlers;

// This handler is dispatched by DispatchDomainEventInterceptor.SavingChangesAsync, which runs
// inside the outer paymentRepository.SaveChangesAsync(). Querying paymentRepository here would
// re-track the already-Modified Payment entity, interfering with EF Core's change tracker and
// causing DbUpdateConcurrencyException on the outer save. IdempotencyKey is now carried on the
// domain event so no DB query is needed.
public class PaymentSucceededDomainEventHandler(IPublishEndpoint publishEndpoint)
    : INotificationHandler<PaymentSucceededDomainEvent>
{
    public async Task Handle(PaymentSucceededDomainEvent notification, CancellationToken cancellationToken)
    {
        // IdempotencyKey is set to SagaCorrelationId.ToString() by InitiatePaymentConsumer
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
}
