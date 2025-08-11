using HiveSpace.OrderService.Domain.Enums;

namespace HiveSpace.OrderService.Application.DTOs;

public record OrderResponse(
    Guid Id,
    Guid CustomerId,
    double SubTotal,
    double ShippingFee,
    double Discount,
    double TotalPrice,
    DateTimeOffset OrderDate,
    OrderStatus Status,
    PaymentMethod PaymentMethod,
    ShippingAddressDto ShippingAddress,
    IReadOnlyList<OrderItemResponse> Items
);