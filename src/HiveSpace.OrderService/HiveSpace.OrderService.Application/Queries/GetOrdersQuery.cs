using HiveSpace.OrderService.Application.DTOs;
using MediatR;

namespace HiveSpace.OrderService.Application.Queries;

public record GetOrdersQuery(
    int PageNumber = 1,
    int PageSize = 10,
    Dictionary<string, object>? Filters = null
) : IRequest<List<OrderResponse>>;