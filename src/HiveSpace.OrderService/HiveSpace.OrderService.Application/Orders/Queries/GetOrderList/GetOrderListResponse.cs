using HiveSpace.Core.Models.Pagination;
using HiveSpace.OrderService.Application.Orders.Dtos;

namespace HiveSpace.OrderService.Application.Orders.Queries.GetOrderList;

public record GetOrderListResponse(
    List<BuyerOrderSummaryDto> Orders,
    PaginationMetadata Pagination);
