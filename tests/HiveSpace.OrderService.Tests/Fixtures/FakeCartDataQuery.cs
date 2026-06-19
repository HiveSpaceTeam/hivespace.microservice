using HiveSpace.Core.Models.Pagination;
using HiveSpace.OrderService.Application.Cart;
using HiveSpace.OrderService.Application.Cart.Dtos;

namespace HiveSpace.OrderService.Tests.Fixtures;

public sealed class FakeCartDataQuery : ICartDataQuery
{
    public Task<PagedResult<CartItemDto>> GetPagedCartItemsAsync(
        Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
        => Task.FromResult(new PagedResult<CartItemDto>([], page, pageSize, 0));
}
