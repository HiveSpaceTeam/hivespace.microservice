using HiveSpace.Core.Models.Pagination;
using HiveSpace.OrderService.Application.Cart.Dtos;

namespace HiveSpace.OrderService.Application.Cart;

public interface ICartDataQuery
{
    Task<PagedResult<CartItemDto>> GetPagedCartItemsAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);
}
