using HiveSpace.Application.Shared.Handlers;
using HiveSpace.Core.Contexts;
using HiveSpace.OrderService.Application.Orders;

namespace HiveSpace.OrderService.Application.Orders.Queries.GetOrderList;

public class GetOrderListQueryHandler(IOrderDataQuery orderDataQuery, IUserContext userContext)
    : IQueryHandler<GetOrderListQuery, GetOrderListResponse>
{
    public Task<GetOrderListResponse> Handle(GetOrderListQuery request, CancellationToken cancellationToken)
        => orderDataQuery.GetPagedOrdersAsync(userContext.UserId, request.Page, request.PageSize, cancellationToken);
}
