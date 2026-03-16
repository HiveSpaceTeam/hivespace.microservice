using HiveSpace.Infrastructure.Persistence.Repositories;
using HiveSpace.OrderService.Domain.Aggregates.Coupons;
using HiveSpace.OrderService.Domain.Repositories;
using HiveSpace.OrderService.Infrastructure.Data;

namespace HiveSpace.OrderService.Infrastructure.Repositories;

public class SqlCouponRepository : BaseRepository<Coupon, Guid>, ICouponRepository
{
    public SqlCouponRepository(OrderDbContext context) : base(context)
    {
    }
}
