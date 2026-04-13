using HiveSpace.PaymentService.Application.Interfaces.Messaging;
using HiveSpace.PaymentService.Domain.DomainEvents;
using HiveSpace.PaymentService.Domain.Repositories;
using MediatR;

namespace HiveSpace.PaymentService.Application.Payments.EventHandlers;

public class PaymentFailedDomainEventHandler(
    IPaymentEventPublisher paymentEventPublisher,
    IPaymentRepository paymentRepository)
    : INotificationHandler<PaymentFailedDomainEvent>
{
    public async Task Handle(PaymentFailedDomainEvent notification, CancellationToken cancellationToken)
    {
        var payment = await paymentRepository.GetByIdAsync(notification.PaymentId, cancellationToken);
        Guid.TryParse(payment?.IdempotencyKey, out var sagaCorrelationId);

        await paymentEventPublisher.PublishPaymentFailedAsync(notification, sagaCorrelationId, cancellationToken);
    }
}
