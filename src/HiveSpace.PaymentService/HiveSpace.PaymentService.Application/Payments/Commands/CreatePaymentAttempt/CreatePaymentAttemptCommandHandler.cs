using HiveSpace.Application.Shared.Handlers;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.PaymentService.Application.Interfaces.Messaging;
using HiveSpace.PaymentService.Application.Payments.Dtos;
using HiveSpace.PaymentService.Domain.Aggregates.Payments;
using HiveSpace.PaymentService.Domain.Aggregates.Payments.Enumerations;
using HiveSpace.PaymentService.Domain.Exceptions;
using HiveSpace.PaymentService.Domain.Repositories;
using HiveSpace.PaymentService.Domain.Services;
using PaymentMethodCodes = HiveSpace.Domain.Shared.Enumerations.PaymentMethodCodes;

namespace HiveSpace.PaymentService.Application.Payments.Commands.CreatePaymentAttempt;

public class CreatePaymentAttemptCommandHandler(
    IPaymentRepository paymentRepository,
    IPaymentGatewayFactory gatewayFactory,
    IPaymentEventPublisher eventPublisher)
    : ICommandHandler<CreatePaymentAttemptCommand, CreatePaymentAttemptResponse>
{
    public async Task<CreatePaymentAttemptResponse> Handle(CreatePaymentAttemptCommand request, CancellationToken cancellationToken)
    {
        var payment = await paymentRepository.GetByIdAsync(request.PaymentId, cancellationToken)
            ?? throw new NotFoundException(PaymentDomainErrorCode.PaymentNotFound, nameof(Payment));

        var methodCode = request.MethodCode.Trim().ToUpperInvariant();
        if (methodCode == PaymentMethodCodes.Stripe)
            throw new InvalidFieldException(PaymentDomainErrorCode.PaymentMethodInvalid, nameof(request.MethodCode));
        if (methodCode is not PaymentMethodCodes.COD and not PaymentMethodCodes.VNPAY)
            throw new InvalidFieldException(PaymentDomainErrorCode.PaymentMethodInvalid, nameof(request.MethodCode));
        if (string.IsNullOrWhiteSpace(payment.ReferenceNo) || payment.LinkedOrders.Count == 0)
            throw new InvalidFieldException(PaymentDomainErrorCode.PaymentLinkedOrdersRequired, nameof(payment.LinkedOrders));
        if (LinkedOrdersHaveEnteredFulfillment(payment))
            throw new InvalidFieldException(PaymentDomainErrorCode.PaymentRetryNotAllowed, nameof(payment.LinkedOrders));

        PaymentAttempt attempt;
        if (methodCode == PaymentMethodCodes.COD)
        {
            attempt = payment.AddAttempt(methodCode, request.IdempotencyKey);
        }
        else
        {
            attempt = payment.AddAttempt(methodCode, request.IdempotencyKey);
            var gateway = gatewayFactory.GetGateway(PaymentGateway.VNPay);
            var initiateResult = await gateway.InitiatePaymentAsync(
                payment,
                request.ReturnUrl ?? string.Empty,
                request.CancelUrl ?? request.ReturnUrl ?? string.Empty,
                cancellationToken);
            payment.MarkAsProcessing(initiateResult.PaymentUrl);
        }

        await paymentRepository.SaveChangesAsync(cancellationToken);
        await eventPublisher.PublishPaymentAttemptInitiatedAsync(payment, cancellationToken);

        return new CreatePaymentAttemptResponse(
            payment.Id,
            payment.ReferenceNo ?? payment.Id.ToString("N").ToUpperInvariant(),
            ToDto(attempt));
    }

    private static PaymentAttemptDto ToDto(PaymentAttempt attempt) => new(
        attempt.Id,
        attempt.AttemptNo,
        attempt.MethodCode,
        attempt.GatewayCode,
        attempt.Status.ToString(),
        attempt.RedirectUrl,
        attempt.GatewayTransactionId,
        attempt.FailureReasonCode,
        attempt.CreatedAt,
        attempt.ExpiresAt,
        attempt.CompletedAt);

    private static bool LinkedOrdersHaveEnteredFulfillment(Payment payment)
    {
        string[] fulfillmentStatuses =
        [
            "Confirmed",
            "Processing",
            "ReadyForFulfillment",
            "FulfillmentStarted",
            "Shipped",
            "Delivered",
            "Completed"
        ];

        return payment.LinkedOrders.Any(order =>
            !string.IsNullOrWhiteSpace(order.StatusSnapshot) &&
            fulfillmentStatuses.Contains(order.StatusSnapshot, StringComparer.OrdinalIgnoreCase));
    }
}
