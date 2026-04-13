using HiveSpace.PaymentService.Domain.DomainEvents;

namespace HiveSpace.PaymentService.Application.Interfaces.Messaging;

public interface IPaymentEventPublisher
{
    Task PublishPaymentSucceededAsync(PaymentSucceededDomainEvent notification, CancellationToken cancellationToken = default);
    Task PublishPaymentFailedAsync(PaymentFailedDomainEvent notification, Guid sagaCorrelationId, CancellationToken cancellationToken = default);
}
