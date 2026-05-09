using HiveSpace.Application.Shared.Handlers;
using HiveSpace.Core.Contexts;
using HiveSpace.OrderService.Domain.Repositories;

namespace HiveSpace.OrderService.Application.Cart.Queries.GetSelectedCartItemsCount;

public class GetSelectedCartItemsCountQueryHandler(ICartRepository cartRepository, IUserContext userContext)
    : IQueryHandler<GetSelectedCartItemsCountQuery, int>
{
    public Task<int> Handle(GetSelectedCartItemsCountQuery request, CancellationToken cancellationToken)
        => cartRepository.CountSelectedItemsAsync(userContext.UserId, cancellationToken);
}
