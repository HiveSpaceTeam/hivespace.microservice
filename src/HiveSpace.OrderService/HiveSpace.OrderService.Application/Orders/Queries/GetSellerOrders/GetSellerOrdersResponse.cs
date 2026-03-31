using HiveSpace.OrderService.Application.Orders.Dtos;

namespace HiveSpace.OrderService.Application.Orders.Queries.GetSellerOrders;

public record GetSellerOrdersResponse(
    List<SellerOrderSummaryDto> Items,
    int TotalCount,
    int Page,
    int PageSize);
