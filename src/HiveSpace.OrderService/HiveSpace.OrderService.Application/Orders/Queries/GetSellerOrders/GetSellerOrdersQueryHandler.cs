using HiveSpace.Application.Shared.Handlers;
using HiveSpace.Core.Contexts;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.OrderService.Application.Orders;
using HiveSpace.OrderService.Domain.Exceptions;

namespace HiveSpace.OrderService.Application.Orders.Queries.GetSellerOrders;

public class GetSellerOrdersQueryHandler(IOrderDataQuery orderDataQuery, IUserContext userContext)
    : IQueryHandler<GetSellerOrdersQuery, GetSellerOrdersResponse>
{
    public Task<GetSellerOrdersResponse> Handle(GetSellerOrdersQuery request, CancellationToken cancellationToken)
    {
        if (userContext.StoreId is null)
            throw new ForbiddenException(OrderDomainErrorCode.SellerStoreRequired, nameof(userContext.StoreId));

        return orderDataQuery.GetSellerOrdersAsync(
            userContext.StoreId.Value,
            request.Page, request.PageSize,
            request.ProcessStatus, request.SearchField, request.SearchValue,
            cancellationToken);
    }
}
