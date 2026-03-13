using HiveSpace.Infrastructure.Persistence.Repositories;
using HiveSpace.OrderService.Domain.Aggregates.Carts;
using HiveSpace.OrderService.Domain.Repositories;
using HiveSpace.OrderService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HiveSpace.OrderService.Infrastructure.Repositories;

public class SqlCartRepository : BaseRepository<Cart, Guid>, ICartRepository
{
    private readonly OrderDbContext _orderDbContext;

    public SqlCartRepository(OrderDbContext context) : base(context)
    {
        _orderDbContext = context;
    }

    public async Task<Cart?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _orderDbContext.Set<Cart>()
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);
    }
}
