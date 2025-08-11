using HiveSpace.OrderService.Application.DTOs;
using MediatR;

namespace HiveSpace.OrderService.Application.Queries;

public record GetOrderByIdQuery(Guid OrderId) : IRequest<OrderResponse?>;