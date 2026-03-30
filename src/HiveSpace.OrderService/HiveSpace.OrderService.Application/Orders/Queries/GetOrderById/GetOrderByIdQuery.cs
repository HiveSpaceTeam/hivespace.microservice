using HiveSpace.OrderService.Application.Orders.Dtos;
using MediatR;

namespace HiveSpace.OrderService.Application.Orders.Queries.GetOrderById;

public record GetOrderByIdQuery(Guid OrderId) : IRequest<OrderDetailDto>;
