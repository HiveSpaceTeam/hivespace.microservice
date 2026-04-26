using HiveSpace.PaymentService.Domain.Aggregates.Payments;

namespace HiveSpace.PaymentService.Application.Interfaces.Messaging;

public interface IPaymentEventPublisher
{
    Task PublishPaymentSucceededAsync(Payment payment, Guid sagaCorrelationId, CancellationToken cancellationToken = default);
    Task PublishPaymentFailedAsync(Payment payment, Guid sagaCorrelationId, CancellationToken cancellationToken = default);
}
