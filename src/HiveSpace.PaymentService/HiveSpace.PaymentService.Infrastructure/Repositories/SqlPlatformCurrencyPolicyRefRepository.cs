using HiveSpace.Infrastructure.Persistence.Repositories;
using HiveSpace.PaymentService.Domain.Aggregates.External;
using HiveSpace.PaymentService.Domain.Repositories;
using HiveSpace.PaymentService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HiveSpace.PaymentService.Infrastructure.Repositories;

public class SqlPlatformCurrencyPolicyRefRepository(PaymentDbContext context)
    : BaseRepository<PlatformCurrencyPolicyRef, Guid>(context), IPlatformCurrencyPolicyRefRepository
{
    public async Task<PlatformCurrencyPolicyRef?> GetCurrentAsync(CancellationToken cancellationToken = default)
        => await context.Set<PlatformCurrencyPolicyRef>()
            .OrderByDescending(x => x.Version)
            .FirstOrDefaultAsync(cancellationToken);
}
