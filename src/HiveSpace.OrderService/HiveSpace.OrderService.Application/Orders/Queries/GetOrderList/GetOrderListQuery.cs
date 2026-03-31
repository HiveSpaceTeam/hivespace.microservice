using HiveSpace.Application.Shared.Queries;

namespace HiveSpace.OrderService.Application.Orders.Queries.GetOrderList;

public record GetOrderListQuery(int Page = 1, int PageSize = 20) : IQuery<GetOrderListResponse>;
