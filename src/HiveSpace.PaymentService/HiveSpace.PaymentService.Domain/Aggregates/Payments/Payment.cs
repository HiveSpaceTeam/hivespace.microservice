using HiveSpace.Domain.Shared.Entities;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Domain.Shared.Interfaces;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.PaymentService.Domain.Aggregates.Payments.Enumerations;
using HiveSpace.PaymentService.Domain.Exceptions;
using HiveSpace.PaymentService.Domain.ValueObjects;
using PaymentMethodCodes = HiveSpace.Domain.Shared.Enumerations.PaymentMethodCodes;

namespace HiveSpace.PaymentService.Domain.Aggregates.Payments;

public class Payment : AggregateRoot<Guid>, IAuditable
{
    public Guid OrderId { get; private set; }
    public string? ReferenceNo { get; private set; }
    public Guid BuyerId { get; private set; }
    public Guid? CheckoutCorrelationId { get; private set; }
    public Money Amount { get; private set; } = null!;
    public PaymentMethod PaymentMethod { get; private set; } = null!;
    public string MethodCode { get; private set; } = null!;
    public PaymentStatus Status { get; private set; }
    public PaymentGateway Gateway { get; private set; }
    public Guid? CurrentAttemptId { get; private set; }
    public string? GatewayTransactionId { get; private set; }
    public string? GatewayPaymentUrl { get; private set; }
    public GatewayResponse? GatewayResponse { get; private set; }
    public string IdempotencyKey { get; private set; } = null!;
    public DateTimeOffset? PaidAt { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public List<PaymentLinkedOrder> LinkedOrders { get; private set; } = [];
    public List<PaymentAttempt> Attempts { get; private set; } = [];
    public PaymentAttempt? CurrentAttempt => Attempts.FirstOrDefault(a => a.Id == CurrentAttemptId);

    private Payment() { }

    public static Payment CreateForOrder(
        Guid orderId,
        Guid buyerId,
        Money amount,
        PaymentMethod paymentMethod,
        PaymentGateway gateway,
        string idempotencyKey)
    {
        if (orderId == Guid.Empty)
            throw new InvalidFieldException(PaymentDomainErrorCode.PaymentOrderIdRequired, nameof(orderId));
        if (buyerId == Guid.Empty)
            throw new InvalidFieldException(PaymentDomainErrorCode.PaymentBuyerIdRequired, nameof(buyerId));
        if (amount is null || !amount.IsPositive())
            throw new InvalidFieldException(PaymentDomainErrorCode.PaymentAmountRequired, nameof(amount));
        if (string.IsNullOrWhiteSpace(idempotencyKey))
            throw new InvalidFieldException(PaymentDomainErrorCode.PaymentIdempotencyKeyRequired, nameof(idempotencyKey));

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            BuyerId = buyerId,
            Amount = amount,
            PaymentMethod = paymentMethod,
            MethodCode = gateway == PaymentGateway.VNPay ? PaymentMethodCodes.VNPAY : gateway.ToString().ToUpperInvariant(),
            Gateway = gateway,
            Status = PaymentStatus.Pending,
            IdempotencyKey = idempotencyKey,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(15),
            CreatedAt = DateTimeOffset.UtcNow
        };

        return payment;
    }

    public static Payment CreateCheckout(
        Guid checkoutCorrelationId,
        string referenceNo,
        Guid buyerId,
        Money amount,
        string methodCode,
        string idempotencyKey,
        IEnumerable<PaymentLinkedOrder> linkedOrders,
        string? redirectUrl = null)
    {
        if (checkoutCorrelationId == Guid.Empty)
            throw new InvalidFieldException(PaymentDomainErrorCode.PaymentNotFound, nameof(checkoutCorrelationId));
        if (string.IsNullOrWhiteSpace(referenceNo))
            throw new InvalidFieldException(PaymentDomainErrorCode.PaymentReferenceNoRequired, nameof(referenceNo));
        if (buyerId == Guid.Empty)
            throw new InvalidFieldException(PaymentDomainErrorCode.PaymentBuyerIdRequired, nameof(buyerId));
        if (amount is null || !amount.IsPositive())
            throw new InvalidFieldException(PaymentDomainErrorCode.PaymentAmountRequired, nameof(amount));
        if (string.IsNullOrWhiteSpace(methodCode))
            throw new InvalidFieldException(PaymentDomainErrorCode.PaymentMethodInvalid, nameof(methodCode));
        if (string.IsNullOrWhiteSpace(idempotencyKey))
            throw new InvalidFieldException(PaymentDomainErrorCode.PaymentIdempotencyKeyRequired, nameof(idempotencyKey));

        var orders = linkedOrders.ToList();
        if (orders.Count == 0)
            throw new InvalidFieldException(PaymentDomainErrorCode.PaymentLinkedOrdersRequired, nameof(linkedOrders));
        if (orders.Select(o => o.OrderId).Distinct().Count() != orders.Count)
            throw new InvalidFieldException(PaymentDomainErrorCode.PaymentLinkedOrderDuplicate, nameof(linkedOrders));
        if (orders.Any(o => !o.CurrencyCode.Equals(amount.Currency.ToString(), StringComparison.OrdinalIgnoreCase)) ||
            orders.Sum(o => o.Amount) != amount.Amount)
            throw new InvalidFieldException(PaymentDomainErrorCode.PaymentLinkedOrderTotalMismatch, nameof(linkedOrders));

        var normalizedMethod = methodCode.Trim().ToUpperInvariant();
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = orders[0].OrderId,
            ReferenceNo = referenceNo.Trim().ToUpperInvariant(),
            BuyerId = buyerId,
            CheckoutCorrelationId = checkoutCorrelationId,
            Amount = amount,
            PaymentMethod = ToPaymentMethod(normalizedMethod),
            MethodCode = normalizedMethod,
            Gateway = ToGateway(normalizedMethod),
            Status = PaymentStatus.Pending,
            IdempotencyKey = idempotencyKey,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(15),
            CreatedAt = DateTimeOffset.UtcNow
        };

        foreach (var order in orders)
        {
            order.AttachToPayment(payment.Id);
            payment.LinkedOrders.Add(order);
        }

        payment.AddAttempt(normalizedMethod, idempotencyKey, redirectUrl);
        return payment;
    }

    public void MarkAsProcessing(string gatewayPaymentUrl)
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidFieldException(PaymentDomainErrorCode.PaymentInvalidStatus, nameof(Status));
        ThrowIfExpired();

        Status = PaymentStatus.Processing;
        GatewayPaymentUrl = gatewayPaymentUrl;
        CurrentAttempt?.SetRedirectUrl(gatewayPaymentUrl);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private void ThrowIfExpired()
    {
        if (DateTimeOffset.UtcNow > ExpiresAt)
            throw new InvalidFieldException(PaymentDomainErrorCode.PaymentExpired, nameof(ExpiresAt));
    }

    public void MarkAsSucceeded(string gatewayTransactionId, GatewayResponse response)
    {
        if (Status != PaymentStatus.Processing)
            throw new InvalidFieldException(PaymentDomainErrorCode.PaymentInvalidStatus, nameof(Status));

        Status = PaymentStatus.Succeeded;
        GatewayTransactionId = gatewayTransactionId;
        GatewayResponse = response;
        PaidAt = DateTimeOffset.UtcNow;
        CurrentAttempt?.MarkSucceeded(gatewayTransactionId, response.RawResponse);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkAsFailed(string reason, GatewayResponse? response = null)
    {
        if (Status == PaymentStatus.Succeeded)
            throw new InvalidFieldException(PaymentDomainErrorCode.PaymentAlreadySucceeded, nameof(Status));

        Status = PaymentStatus.Failed;
        GatewayResponse = response;
        CurrentAttempt?.MarkFailedOrExpired("Failed", reason, response?.RawResponse);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Cancel()
    {
        if (Status == PaymentStatus.Succeeded)
            throw new InvalidFieldException(PaymentDomainErrorCode.PaymentAlreadySucceeded, nameof(Status));

        Status = PaymentStatus.Cancelled;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkAsExpired()
    {
        if (Status != PaymentStatus.Pending && Status != PaymentStatus.Processing) return;

        Status = PaymentStatus.Expired;
        CurrentAttempt?.MarkFailedOrExpired("Expired", "Payment expired");
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public PaymentAttempt AddAttempt(string methodCode, string idempotencyKey, string? redirectUrl = null)
    {
        if (Status == PaymentStatus.Succeeded || Attempts.Any(a => a.Status == Enumerations.PaymentAttemptStatus.Succeeded))
            throw new InvalidFieldException(PaymentDomainErrorCode.PaymentRetryNotAllowed, nameof(Status));
        if (Attempts.Count > 0 && CurrentAttempt?.Status is Enumerations.PaymentAttemptStatus.Pending or Enumerations.PaymentAttemptStatus.Processing)
            throw new InvalidFieldException(PaymentDomainErrorCode.PaymentRetryNotAllowed, nameof(CurrentAttempt));

        var normalizedMethod = methodCode.Trim().ToUpperInvariant();
        if (normalizedMethod == PaymentMethodCodes.Stripe)
            throw new InvalidFieldException(PaymentDomainErrorCode.PaymentMethodInvalid, nameof(methodCode));

        var attempt = PaymentAttempt.Create(
            Id,
            Attempts.Count + 1,
            normalizedMethod,
            Amount.Amount,
            Amount.Currency.ToString(),
            idempotencyKey,
            redirectUrl);

        Attempts.Add(attempt);
        CurrentAttemptId = attempt.Id;
        MethodCode = normalizedMethod;
        PaymentMethod = ToPaymentMethod(normalizedMethod);
        Gateway = ToGateway(normalizedMethod);
        Status = redirectUrl is null ? PaymentStatus.Pending : PaymentStatus.Processing;
        GatewayPaymentUrl = redirectUrl;
        UpdatedAt = DateTimeOffset.UtcNow;
        return attempt;
    }

    public bool TryMarkAttemptSucceeded(Guid attemptId, string gatewayTransactionId, string? rawResponse = null)
    {
        if (Status == PaymentStatus.Succeeded && CurrentAttemptId != attemptId)
            return false;
        if (CurrentAttemptId != attemptId)
            return false;

        MarkAttemptSucceeded(attemptId, gatewayTransactionId, rawResponse);
        return true;
    }

    public bool TryMarkAttemptSucceeded(int attemptNo, string gatewayTransactionId, string? rawResponse = null)
    {
        var attempt = Attempts.FirstOrDefault(a => a.AttemptNo == attemptNo);
        return attempt is not null && TryMarkAttemptSucceeded(attempt.Id, gatewayTransactionId, rawResponse);
    }

    public void MarkAttemptSucceeded(Guid attemptId, string gatewayTransactionId, string? rawResponse = null)
    {
        var attempt = Attempts.FirstOrDefault(a => a.Id == attemptId)
            ?? throw new InvalidFieldException(PaymentDomainErrorCode.PaymentInvalidStatus, nameof(attemptId));
        if (CurrentAttemptId != attemptId)
            throw new InvalidFieldException(PaymentDomainErrorCode.PaymentInvalidStatus, nameof(attemptId));

        attempt.MarkSucceeded(gatewayTransactionId, rawResponse);
        Status = PaymentStatus.Succeeded;
        GatewayTransactionId = gatewayTransactionId;
        GatewayResponse = rawResponse is null ? GatewayResponse : new GatewayResponse(rawResponse, true);
        PaidAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkAttemptFailedOrExpired(Guid attemptId, string failureType, string? reason, string? rawResponse = null)
    {
        var attempt = Attempts.FirstOrDefault(a => a.Id == attemptId)
            ?? throw new InvalidFieldException(PaymentDomainErrorCode.PaymentInvalidStatus, nameof(attemptId));
        if (attempt.Status == Enumerations.PaymentAttemptStatus.Succeeded)
            throw new InvalidFieldException(PaymentDomainErrorCode.PaymentAlreadySucceeded, nameof(attemptId));

        attempt.MarkFailedOrExpired(failureType, reason, rawResponse);
        if (CurrentAttemptId == attemptId)
        {
            Status = failureType.Equals("Expired", StringComparison.OrdinalIgnoreCase)
                ? PaymentStatus.Expired
                : PaymentStatus.Failed;
            GatewayResponse = rawResponse is null ? GatewayResponse : new GatewayResponse(rawResponse, false, reason);
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }

    public bool TryMarkAttemptFailedOrExpired(int attemptNo, string failureType, string? reason, string? rawResponse = null)
    {
        var attempt = Attempts.FirstOrDefault(a => a.AttemptNo == attemptNo);
        if (attempt is null || CurrentAttemptId != attempt.Id)
            return false;

        MarkAttemptFailedOrExpired(attempt.Id, failureType, reason, rawResponse);
        return true;
    }

    private static PaymentGateway ToGateway(string methodCode)
    {
        if (methodCode.Equals(PaymentMethodCodes.COD, StringComparison.OrdinalIgnoreCase))
            return PaymentGateway.None;

        return methodCode.Equals(PaymentMethodCodes.Stripe, StringComparison.OrdinalIgnoreCase)
            ? PaymentGateway.Stripe
            : PaymentGateway.VNPay;
    }

    private static PaymentMethod ToPaymentMethod(string methodCode)
    {
        return methodCode.Equals(PaymentMethodCodes.COD, StringComparison.OrdinalIgnoreCase)
            ? PaymentMethod.COD()
            : PaymentMethod.BankTransfer(methodCode);
    }
}
