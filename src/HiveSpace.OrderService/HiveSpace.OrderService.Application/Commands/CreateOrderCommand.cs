using HiveSpace.OrderService.Application.DTOs;
using HiveSpace.OrderService.Domain.Enums;
using MediatR;

namespace HiveSpace.OrderService.Application.Commands;

public record CreateOrderCommand(
    Guid CustomerId,
    double ShippingFee,
    double Discount,
    PaymentMethod PaymentMethod,
    ShippingAddressDto ShippingAddress,
    IReadOnlyList<OrderItemBaseDto> Items
) : IRequest<Guid>;