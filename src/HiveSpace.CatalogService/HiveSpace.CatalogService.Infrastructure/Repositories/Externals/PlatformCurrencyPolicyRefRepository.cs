using HiveSpace.CatalogService.Domain.Aggregates.External;
using HiveSpace.CatalogService.Domain.Repositories.External;
using HiveSpace.CatalogService.Infrastructure.Data;
using HiveSpace.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HiveSpace.CatalogService.Infrastructure.Repositories.Externals;

public class PlatformCurrencyPolicyRefRepository(CatalogDbContext context)
    : BaseRepository<PlatformCurrencyPolicyRef, Guid>(context), IPlatformCurrencyPolicyRefRepository
{
    public async Task<PlatformCurrencyPolicyRef?> GetCurrentAsync(CancellationToken cancellationToken = default)
        => await context.Set<PlatformCurrencyPolicyRef>()
            .OrderByDescending(x => x.Version)
            .FirstOrDefaultAsync(cancellationToken);
}
