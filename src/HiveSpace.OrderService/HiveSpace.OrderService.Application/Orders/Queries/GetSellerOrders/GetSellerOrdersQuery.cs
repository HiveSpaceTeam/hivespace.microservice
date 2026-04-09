using HiveSpace.Application.Shared.Queries;
using HiveSpace.OrderService.Application.Orders.Enums;

namespace HiveSpace.OrderService.Application.Orders.Queries.GetSellerOrders;

public record GetSellerOrdersQuery(
    int Page = 1,
    int PageSize = 20,
    SellerOrderProcessStatus? ProcessStatus = null,
    string? SearchField = null,
    string? SearchValue = null)
    : IQuery<GetSellerOrdersResponse>;
