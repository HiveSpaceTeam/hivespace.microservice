using HiveSpace.Application.Shared.Handlers;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.PaymentService.Application.Interfaces.Messaging;
using HiveSpace.PaymentService.Domain.Aggregates.Payments;
using HiveSpace.PaymentService.Domain.Aggregates.Payments.Enumerations;
using HiveSpace.PaymentService.Domain.Exceptions;
using HiveSpace.PaymentService.Domain.Repositories;
using HiveSpace.PaymentService.Domain.Services;
using HiveSpace.PaymentService.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace HiveSpace.PaymentService.Application.Payments.Commands.ProcessPaymentWebhook;

public class ProcessPaymentWebhookCommandHandler(
    IPaymentRepository paymentRepository,
    IPaymentEventPublisher paymentEventPublisher,
    IPaymentGatewayFactory gatewayFactory,
    ILogger<ProcessPaymentWebhookCommandHandler>? logger = null)
    : ICommandHandler<ProcessPaymentWebhookCommand>
{
    private readonly ILogger<ProcessPaymentWebhookCommandHandler> _logger =
        logger ?? NullLogger<ProcessPaymentWebhookCommandHandler>.Instance;

    public async Task Handle(ProcessPaymentWebhookCommand request, CancellationToken cancellationToken)
    {
        var payment = request.PaymentId != Guid.Empty
            ? await paymentRepository.GetByIdAsync(request.PaymentId, cancellationToken)
            : await paymentRepository.GetByReferenceNoAsync(request.ReferenceNo ?? string.Empty, cancellationToken);
        if (payment is null)
            throw new NotFoundException(PaymentDomainErrorCode.PaymentNotFound, nameof(Payment));

        // Idempotency: VNPay retries the IPN until it gets a valid response.
        // If the payment is already in a terminal state, acknowledge without reprocessing.
        if (payment.Status is PaymentStatus.Succeeded or PaymentStatus.Cancelled ||
            (payment.Status is PaymentStatus.Failed or PaymentStatus.Expired && request.AttemptNo is null))
        {
            _logger.LogInformation(
                "Skipping terminal payment webhook for payment {PaymentId}, reference {ReferenceNo}, status {Status}, attempt {AttemptNo}",
                payment.Id,
                payment.ReferenceNo,
                payment.Status,
                request.AttemptNo);
            return;
        }

        var gateway = gatewayFactory.GetGateway(request.Gateway);
        var result = await gateway.VerifyWebhookAsync(request.Payload, cancellationToken);
        var gatewayResponse = new GatewayResponse(result.RawResponse, result.Success, result.ErrorMessage);

        Guid.TryParse(payment.IdempotencyKey, out var sagaCorrelationId);

        if (result.Success)
        {
            if (request.AttemptNo.HasValue)
            {
                if (payment.TryMarkAttemptSucceeded(request.AttemptNo.Value, result.TransactionId, result.RawResponse))
                {
                    await paymentEventPublisher.PublishPaymentSucceededAsync(payment, sagaCorrelationId, cancellationToken);
                }
                else
                {
                    LogStaleAttempt(payment, request.AttemptNo.Value, result.TransactionId);
                }
            }
            else if (payment.CurrentAttemptId.HasValue)
            {
                if (payment.TryMarkAttemptSucceeded(payment.CurrentAttemptId.Value, result.TransactionId, result.RawResponse))
                    await paymentEventPublisher.PublishPaymentSucceededAsync(payment, sagaCorrelationId, cancellationToken);
            }
            else
            {
                payment.MarkAsSucceeded(result.TransactionId, gatewayResponse);
                await paymentEventPublisher.PublishPaymentSucceededAsync(payment, sagaCorrelationId, cancellationToken);
            }
        }
        else
        {
            var shouldPublishFailure = true;
            if (request.AttemptNo.HasValue)
            {
                shouldPublishFailure = payment.TryMarkAttemptFailedOrExpired(
                    request.AttemptNo.Value,
                    "Failed",
                    result.ErrorMessage ?? "Payment failed",
                    result.RawResponse);
                if (!shouldPublishFailure)
                    LogStaleAttempt(payment, request.AttemptNo.Value, result.TransactionId);
            }
            else if (payment.CurrentAttemptId.HasValue)
            {
                payment.MarkAttemptFailedOrExpired(
                    payment.CurrentAttemptId.Value,
                    "Failed",
                    result.ErrorMessage ?? "Payment failed",
                    result.RawResponse);
            }
            else
            {
                payment.MarkAsFailed(result.ErrorMessage ?? "Payment failed", gatewayResponse);
            }

            if (shouldPublishFailure)
                await paymentEventPublisher.PublishPaymentFailedAsync(payment, sagaCorrelationId, cancellationToken);
        }

        await paymentRepository.SaveChangesAsync(cancellationToken);
    }

    private void LogStaleAttempt(Payment payment, int attemptNo, string gatewayTransactionId)
    {
        _logger.LogWarning(
            "Ignoring stale payment webhook for payment {PaymentId}, reference {ReferenceNo}, callback attempt {CallbackAttemptNo}, current attempt {CurrentAttemptNo}, gateway transaction {GatewayTransactionId}",
            payment.Id,
            payment.ReferenceNo,
            attemptNo,
            payment.CurrentAttempt?.AttemptNo,
            gatewayTransactionId);
    }
}
