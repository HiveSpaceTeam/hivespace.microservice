using HiveSpace.Infrastructure.Messaging.Shared.IntegrationEvents;
using HiveSpace.PaymentService.Domain.DomainEvents;
using HiveSpace.PaymentService.Domain.Repositories;
using MassTransit;
using MediatR;

namespace HiveSpace.PaymentService.Application.Payments.EventHandlers;

public class PaymentFailedDomainEventHandler(
    IPublishEndpoint publishEndpoint,
    IPaymentRepository paymentRepository)
    : INotificationHandler<PaymentFailedDomainEvent>
{
    public async Task Handle(PaymentFailedDomainEvent notification, CancellationToken cancellationToken)
    {
        var payment = await paymentRepository.GetByIdAsync(notification.PaymentId, cancellationToken);
        Guid.TryParse(payment?.IdempotencyKey, out var sagaCorrelationId);

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
