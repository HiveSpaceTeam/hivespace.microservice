using HiveSpace.Domain.Shared.Entities;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.PaymentService.Domain.Aggregates.Payments.Enumerations;
using HiveSpace.PaymentService.Domain.Exceptions;
using PaymentMethodCodes = HiveSpace.Domain.Shared.Enumerations.PaymentMethodCodes;

namespace HiveSpace.PaymentService.Domain.Aggregates.Payments;

public class PaymentAttempt : Entity<Guid>
{
    public Guid PaymentId { get; private set; }
    public int AttemptNo { get; private set; }
    public string MethodCode { get; private set; } = null!;
    public string? GatewayCode { get; private set; }
    public long Amount { get; private set; }
    public string CurrencyCode { get; private set; } = null!;
    public PaymentAttemptStatus Status { get; private set; }
    public string? GatewayTransactionId { get; private set; }
    public string? GatewayResponse { get; private set; }
    public string? RedirectUrl { get; private set; }
    public string? FailureReasonCode { get; private set; }
    public string? FailureReason { get; private set; }
    public string IdempotencyKey { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }

    private PaymentAttempt() { }

    internal static PaymentAttempt Create(
        Guid paymentId,
        int attemptNo,
        string methodCode,
        long amount,
        string currencyCode,
        string idempotencyKey,
        string? redirectUrl = null)
    {
        if (paymentId == Guid.Empty)
            throw new InvalidFieldException(PaymentDomainErrorCode.PaymentNotFound, nameof(paymentId));
        if (attemptNo <= 0)
            throw new InvalidFieldException(PaymentDomainErrorCode.PaymentInvalidStatus, nameof(attemptNo));
        if (string.IsNullOrWhiteSpace(methodCode))
            throw new InvalidFieldException(PaymentDomainErrorCode.PaymentMethodInvalid, nameof(methodCode));
        if (amount <= 0)
            throw new InvalidFieldException(PaymentDomainErrorCode.PaymentAmountRequired, nameof(amount));
        if (string.IsNullOrWhiteSpace(currencyCode))
            throw new InvalidFieldException(PaymentDomainErrorCode.PaymentAmountRequired, nameof(currencyCode));
        if (string.IsNullOrWhiteSpace(idempotencyKey))
            throw new InvalidFieldException(PaymentDomainErrorCode.PaymentIdempotencyKeyRequired, nameof(idempotencyKey));

        var normalizedMethod = methodCode.Trim().ToUpperInvariant();
        return new PaymentAttempt
        {
            Id = Guid.NewGuid(),
            PaymentId = paymentId,
            AttemptNo = attemptNo,
            MethodCode = normalizedMethod,
            GatewayCode = normalizedMethod == PaymentMethodCodes.COD ? null : normalizedMethod,
            Amount = amount,
            CurrencyCode = currencyCode.Trim().ToUpperInvariant(),
            IdempotencyKey = idempotencyKey,
            RedirectUrl = redirectUrl,
            Status = redirectUrl is null ? PaymentAttemptStatus.Pending : PaymentAttemptStatus.Processing,
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(15)
        };
    }

    internal void SetRedirectUrl(string redirectUrl)
    {
        RedirectUrl = redirectUrl;
        Status = PaymentAttemptStatus.Processing;
    }

    internal void MarkSucceeded(string gatewayTransactionId, string? gatewayResponse = null)
    {
        if (Status == PaymentAttemptStatus.Succeeded)
            return;
        if (Status is PaymentAttemptStatus.Failed or PaymentAttemptStatus.Expired or PaymentAttemptStatus.Cancelled)
            throw new InvalidFieldException(PaymentDomainErrorCode.PaymentInvalidStatus, nameof(Status));

        Status = PaymentAttemptStatus.Succeeded;
        GatewayTransactionId = gatewayTransactionId;
        GatewayResponse = gatewayResponse;
        CompletedAt = DateTimeOffset.UtcNow;
    }

    internal void MarkFailedOrExpired(string failureType, string? reason, string? gatewayResponse = null)
    {
        if (Status == PaymentAttemptStatus.Succeeded)
            throw new InvalidFieldException(PaymentDomainErrorCode.PaymentAlreadySucceeded, nameof(Status));

        Status = failureType.Equals("Expired", StringComparison.OrdinalIgnoreCase)
            ? PaymentAttemptStatus.Expired
            : PaymentAttemptStatus.Failed;
        FailureReasonCode = failureType;
        FailureReason = reason;
        GatewayResponse = gatewayResponse;
        CompletedAt = DateTimeOffset.UtcNow;
    }
}
