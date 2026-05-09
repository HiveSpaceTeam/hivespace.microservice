using HiveSpace.Application.Shared.Handlers;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.PaymentService.Application.Interfaces.Messaging;
using HiveSpace.PaymentService.Domain.Aggregates.Payments;
using HiveSpace.PaymentService.Domain.Aggregates.Payments.Enumerations;
using HiveSpace.PaymentService.Domain.Exceptions;
using HiveSpace.PaymentService.Domain.Repositories;
using HiveSpace.PaymentService.Domain.Services;
using HiveSpace.PaymentService.Domain.ValueObjects;

namespace HiveSpace.PaymentService.Application.Payments.Commands.ProcessPaymentWebhook;

public class ProcessPaymentWebhookCommandHandler(
    IPaymentRepository paymentRepository,
    IPaymentEventPublisher paymentEventPublisher,
    IPaymentGatewayFactory gatewayFactory)
    : ICommandHandler<ProcessPaymentWebhookCommand>
{
    public async Task Handle(ProcessPaymentWebhookCommand request, CancellationToken cancellationToken)
    {
        var payment = await paymentRepository.GetByIdAsync(request.PaymentId, cancellationToken)
            ?? throw new NotFoundException(PaymentDomainErrorCode.PaymentNotFound, nameof(Payment));

        // Idempotency: VNPay retries the IPN until it gets a valid response.
        // If the payment is already in a terminal state, acknowledge without reprocessing.
        if (payment.Status is PaymentStatus.Succeeded or PaymentStatus.Failed or PaymentStatus.Cancelled or PaymentStatus.Expired)
            return;

        var gateway = gatewayFactory.GetGateway(request.Gateway);
        var result = await gateway.VerifyWebhookAsync(request.Payload, cancellationToken);
        var gatewayResponse = new GatewayResponse(result.RawResponse, result.Success, result.ErrorMessage);

        Guid.TryParse(payment.IdempotencyKey, out var sagaCorrelationId);

        if (result.Success)
        {
            payment.MarkAsSucceeded(result.TransactionId, gatewayResponse);
            await paymentEventPublisher.PublishPaymentSucceededAsync(payment, sagaCorrelationId, cancellationToken);
        }
        else
        {
            payment.MarkAsFailed(result.ErrorMessage ?? "Payment failed", gatewayResponse);
            await paymentEventPublisher.PublishPaymentFailedAsync(payment, sagaCorrelationId, cancellationToken);
        }

        await paymentRepository.SaveChangesAsync(cancellationToken);
    }
}
