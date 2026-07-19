using HiveSpace.Domain.Shared.Entities;
using HiveSpace.Domain.Shared.Errors;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Dtos;
using PaymentMethodEnumeration = HiveSpace.Domain.Shared.Enumerations.PaymentMethod;

namespace HiveSpace.OrderService.Api.Models;

public record CheckoutRequest
{
    public DeliveryAddressDto DeliveryAddress { get; init; } = null!;
    public int?               PaymentMethod   { get; init; }   // 1=COD, 2=VNPAY, 3=MOMO, 4=BankTransfer, 5=Balance, 6=PayPal
    public string?             PaymentMethodCode { get; init; }

    public PaymentMethodEnumeration GetPaymentMethod()
    {
        if (PaymentMethod.HasValue)
            return Enumeration.FromValue<PaymentMethodEnumeration>(PaymentMethod.Value);

        if (string.IsNullOrWhiteSpace(PaymentMethodCode))
            return PaymentMethodEnumeration.COD;

        var normalizedCode = PaymentMethodCode.Trim();
        return Enumeration.GetAll<PaymentMethodEnumeration>()
            .FirstOrDefault(method => method.Name.Equals(normalizedCode, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidFieldException(DomainErrorCode.InvalidEnumerationValue, nameof(PaymentMethodCode));
    }
}
