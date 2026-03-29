using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Dtos;

namespace HiveSpace.OrderService.Api.Models;

public record CheckoutRequest(
    DeliveryAddressDto  DeliveryAddress,
    List<string>?       CouponCodes
);
