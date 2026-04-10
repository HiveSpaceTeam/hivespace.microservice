using HiveSpace.Application.Shared.Queries;
using HiveSpace.OrderService.Application.Orders.Enums;

namespace HiveSpace.OrderService.Application.Orders.Queries.GetOrderList;

public record GetOrderListQuery(
    int Page = 1,
    int PageSize = 20,
    CustomerOrderProcessStatus? ProcessStatus = null,
    string? SearchField = null,
    string? SearchValue = null)
    : IQuery<GetOrderListResponse>;
