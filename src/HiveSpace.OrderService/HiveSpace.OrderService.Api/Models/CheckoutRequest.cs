using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Dtos;

namespace HiveSpace.OrderService.Api.Models;

public record CheckoutRequest
{
    public DeliveryAddressDto DeliveryAddress { get; init; } = null!;
    public List<string>?      CouponCodes     { get; init; }
    public int?               PaymentMethod   { get; init; }   // 1=COD, 2=VNPAY, 3=MOMO, 4=BankTransfer, 5=Balance, 6=PayPal
}
