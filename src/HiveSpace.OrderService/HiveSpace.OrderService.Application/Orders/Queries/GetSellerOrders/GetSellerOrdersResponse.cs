using HiveSpace.Core.Models.Pagination;
using HiveSpace.OrderService.Application.Orders.Dtos;

namespace HiveSpace.OrderService.Application.Orders.Queries.GetSellerOrders;

public record GetSellerOrdersResponse(
    List<SellerOrderSummaryDto> Orders,
    PaginationMetadata Pagination);
