using HiveSpace.UserService.Domain.Aggregates.Configuration;

namespace HiveSpace.UserService.Domain.Repositories;

public interface IPlatformCurrencyRepository
{
    Task<List<PlatformCurrency>> GetCurrencyItemsAsync(CancellationToken cancellationToken = default);
    void AddRange(IEnumerable<PlatformCurrency> currencies);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
