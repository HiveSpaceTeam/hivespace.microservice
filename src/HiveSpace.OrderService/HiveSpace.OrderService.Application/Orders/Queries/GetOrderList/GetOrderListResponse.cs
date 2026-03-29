using HiveSpace.OrderService.Application.Orders.Dtos;

namespace HiveSpace.OrderService.Application.Orders.Queries.GetOrderList;

public record GetOrderListResponse(
    List<OrderSummaryDto> Items,
    int TotalCount,
    int Page,
    int PageSize);
