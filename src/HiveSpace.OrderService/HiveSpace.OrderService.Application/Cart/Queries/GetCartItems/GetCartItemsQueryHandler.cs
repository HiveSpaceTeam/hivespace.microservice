using HiveSpace.Application.Shared.Handlers;
using HiveSpace.Core.Contexts;

namespace HiveSpace.OrderService.Application.Cart.Queries.GetCartItems;

public class GetCartItemsQueryHandler(ICartDataQuery cartDataQuery, IUserContext userContext)
    : IQueryHandler<GetCartItemsQuery, GetCartItemsResponse>
{
    public async Task<GetCartItemsResponse> Handle(GetCartItemsQuery request, CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;
        var pagedResult = await cartDataQuery.GetPagedCartItemsAsync(userId, request.Page, request.PageSize, cancellationToken);

        return new GetCartItemsResponse(pagedResult.Items.ToList(), pagedResult.Pagination);
    }
}
