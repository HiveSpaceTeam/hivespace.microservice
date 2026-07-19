using HiveSpace.Domain.Shared.Entities;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.PaymentService.Domain.Exceptions;

namespace HiveSpace.PaymentService.Domain.Aggregates.Payments;

public class PaymentLinkedOrder : Entity<Guid>
{
    public Guid PaymentId { get; private set; }
    public Guid OrderId { get; private set; }
    public string OrderCode { get; private set; } = null!;
    public Guid StoreId { get; private set; }
    public long Amount { get; private set; }
    public string CurrencyCode { get; private set; } = null!;
    public string? StatusSnapshot { get; private set; }

    private PaymentLinkedOrder() { }

    public PaymentLinkedOrder(
        Guid orderId,
        string orderCode,
        Guid storeId,
        long amount,
        string currencyCode,
        string? statusSnapshot = null)
    {
        if (orderId == Guid.Empty)
            throw new InvalidFieldException(PaymentDomainErrorCode.PaymentOrderIdRequired, nameof(orderId));
        if (string.IsNullOrWhiteSpace(orderCode))
            throw new InvalidFieldException(PaymentDomainErrorCode.PaymentLinkedOrdersRequired, nameof(orderCode));
        if (storeId == Guid.Empty)
            throw new InvalidFieldException(PaymentDomainErrorCode.PaymentLinkedOrdersRequired, nameof(storeId));
        if (amount <= 0)
            throw new InvalidFieldException(PaymentDomainErrorCode.PaymentAmountRequired, nameof(amount));
        if (string.IsNullOrWhiteSpace(currencyCode))
            throw new InvalidFieldException(PaymentDomainErrorCode.PaymentAmountRequired, nameof(currencyCode));

        Id = Guid.NewGuid();
        OrderId = orderId;
        OrderCode = orderCode;
        StoreId = storeId;
        Amount = amount;
        CurrencyCode = currencyCode;
        StatusSnapshot = statusSnapshot;
    }

    internal void AttachToPayment(Guid paymentId)
    {
        PaymentId = paymentId;
    }
}
