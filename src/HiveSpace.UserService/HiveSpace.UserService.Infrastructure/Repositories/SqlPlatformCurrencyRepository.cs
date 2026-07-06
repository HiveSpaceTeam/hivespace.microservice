using HiveSpace.Infrastructure.Persistence.Repositories;
using HiveSpace.UserService.Domain.Aggregates.Configuration;
using HiveSpace.UserService.Domain.Repositories;
using HiveSpace.UserService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HiveSpace.UserService.Infrastructure.Repositories;

public class SqlPlatformCurrencyRepository(UserDbContext context)
    : BaseRepository<PlatformCurrency, Guid>(context), IPlatformCurrencyRepository
{
    public async Task<List<PlatformCurrency>> GetCurrencyItemsAsync(CancellationToken cancellationToken = default)
        => await context.PlatformCurrencies
            .Where(x => x.ConfigType == PlatformConfig.CurrencyConfigType)
            .OrderBy(x => x.SortOrder)
            .ToListAsync(cancellationToken);

    public void AddRange(IEnumerable<PlatformCurrency> currencies)
        => context.PlatformCurrencies.AddRange(currencies);
}
