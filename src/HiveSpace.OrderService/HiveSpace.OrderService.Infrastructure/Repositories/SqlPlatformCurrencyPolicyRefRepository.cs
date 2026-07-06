using HiveSpace.Infrastructure.Persistence.Repositories;
using HiveSpace.OrderService.Domain.External;
using HiveSpace.OrderService.Domain.Repositories;
using HiveSpace.OrderService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HiveSpace.OrderService.Infrastructure.Repositories;

public class SqlPlatformCurrencyPolicyRefRepository(OrderDbContext context)
    : BaseRepository<PlatformCurrencyPolicyRef, Guid>(context), IPlatformCurrencyPolicyRefRepository
{
    public async Task<PlatformCurrencyPolicyRef?> GetCurrentAsync(CancellationToken cancellationToken = default)
        => await context.Set<PlatformCurrencyPolicyRef>()
            .OrderByDescending(x => x.Version)
            .FirstOrDefaultAsync(cancellationToken);
}
