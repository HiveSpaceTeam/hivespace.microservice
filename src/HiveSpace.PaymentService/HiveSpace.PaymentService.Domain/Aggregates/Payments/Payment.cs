using HiveSpace.Domain.Shared.Entities;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Domain.Shared.Interfaces;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.PaymentService.Domain.Aggregates.Payments.Enumerations;
using HiveSpace.PaymentService.Domain.Exceptions;
using HiveSpace.PaymentService.Domain.ValueObjects;

namespace HiveSpace.PaymentService.Domain.Aggregates.Payments;

public class Payment : AggregateRoot<Guid>, IAuditable
{
    public Guid OrderId { get; private set; }
    public Guid BuyerId { get; private set; }
    public Money Amount { get; private set; } = null!;
    public PaymentMethod PaymentMethod { get; private set; } = null!;
    public PaymentStatus Status { get; private set; }
    public PaymentGateway Gateway { get; private set; }
    public string? GatewayTransactionId { get; private set; }
    public string? GatewayPaymentUrl { get; private set; }
    public GatewayResponse? GatewayResponse { get; private set; }
    public string IdempotencyKey { get; private set; } = null!;
    public DateTimeOffset? PaidAt { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

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
            Gateway = gateway,
            Status = PaymentStatus.Pending,
            IdempotencyKey = idempotencyKey,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(15),
            CreatedAt = DateTimeOffset.UtcNow
        };

        return payment;
    }

    public void MarkAsProcessing(string gatewayPaymentUrl)
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidFieldException(PaymentDomainErrorCode.PaymentInvalidStatus, nameof(Status));
        if (DateTimeOffset.UtcNow > ExpiresAt)
            throw new InvalidFieldException(PaymentDomainErrorCode.PaymentExpired, nameof(ExpiresAt));

        Status = PaymentStatus.Processing;
        GatewayPaymentUrl = gatewayPaymentUrl;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkAsSucceeded(string gatewayTransactionId, GatewayResponse response)
    {
        if (Status != PaymentStatus.Processing)
            throw new InvalidFieldException(PaymentDomainErrorCode.PaymentInvalidStatus, nameof(Status));

        Status = PaymentStatus.Succeeded;
        GatewayTransactionId = gatewayTransactionId;
        GatewayResponse = response;
        PaidAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkAsFailed(string reason, GatewayResponse? response = null)
    {
        if (Status == PaymentStatus.Succeeded)
            throw new InvalidFieldException(PaymentDomainErrorCode.PaymentAlreadySucceeded, nameof(Status));

        Status = PaymentStatus.Failed;
        GatewayResponse = response;
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
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
