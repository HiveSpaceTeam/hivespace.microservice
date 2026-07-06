using HiveSpace.UserService.Domain.Aggregates.Configuration;

namespace HiveSpace.UserService.Domain.Repositories;

public interface IPlatformConfigRepository
{
    Task<PlatformConfig?> GetCurrencyPolicyAsync(CancellationToken cancellationToken = default);
    void Add(PlatformConfig config);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
