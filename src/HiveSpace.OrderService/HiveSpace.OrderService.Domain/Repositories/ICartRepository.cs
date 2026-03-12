using HiveSpace.Domain.Shared.Interfaces;
using HiveSpace.OrderService.Domain.Aggregates.Carts;

namespace HiveSpace.OrderService.Domain.Repositories;

public interface ICartRepository : IRepository<Cart>
{
    Task<Cart?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
