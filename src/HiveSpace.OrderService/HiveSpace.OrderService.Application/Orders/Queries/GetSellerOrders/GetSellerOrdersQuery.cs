using HiveSpace.Application.Shared.Queries;

namespace HiveSpace.OrderService.Application.Orders.Queries.GetSellerOrders;

public record GetSellerOrdersQuery(int Page = 1, int PageSize = 20, string? Status = null)
    : IQuery<GetSellerOrdersResponse>;
