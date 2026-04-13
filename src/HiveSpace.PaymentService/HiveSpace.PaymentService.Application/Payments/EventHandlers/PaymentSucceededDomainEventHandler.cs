using HiveSpace.PaymentService.Application.Interfaces.Messaging;
using HiveSpace.PaymentService.Domain.DomainEvents;
using MediatR;

namespace HiveSpace.PaymentService.Application.Payments.EventHandlers;

// This handler is dispatched by DispatchDomainEventInterceptor.SavingChangesAsync, which runs
// inside the outer paymentRepository.SaveChangesAsync(). Querying paymentRepository here would
// re-track the already-Modified Payment entity, interfering with EF Core's change tracker and
// causing DbUpdateConcurrencyException on the outer save. IdempotencyKey is now carried on the
// domain event so no DB query is needed.
public class PaymentSucceededDomainEventHandler(IPaymentEventPublisher paymentEventPublisher)
    : INotificationHandler<PaymentSucceededDomainEvent>
{
    public async Task Handle(PaymentSucceededDomainEvent notification, CancellationToken cancellationToken)
    {
        // IdempotencyKey is set to SagaCorrelationId.ToString() by InitiatePaymentConsumer
        if (!Guid.TryParse(notification.IdempotencyKey, out var sagaCorrelationId) || sagaCorrelationId == Guid.Empty)
            throw new InvalidOperationException(
                $"Invalid saga correlation id from IdempotencyKey: '{notification.IdempotencyKey}'.");

        await paymentEventPublisher.PublishPaymentSucceededAsync(notification, cancellationToken);
    }
}
