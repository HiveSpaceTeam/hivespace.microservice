using HiveSpace.Infrastructure.Persistence.Repositories;
using HiveSpace.UserService.Domain.Aggregates.Configuration;
using HiveSpace.UserService.Domain.Repositories;
using HiveSpace.UserService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HiveSpace.UserService.Infrastructure.Repositories;

public class SqlPlatformConfigRepository(UserDbContext context)
    : BaseRepository<PlatformConfig, Guid>(context), IPlatformConfigRepository
{
    public async Task<PlatformConfig?> GetCurrencyPolicyAsync(CancellationToken cancellationToken = default)
        => await context.PlatformConfigs
            .FirstOrDefaultAsync(x => x.ConfigType == PlatformConfig.CurrencyConfigType, cancellationToken);
}
