using HiveSpace.CatalogService.Domain.Aggregates.External;

namespace HiveSpace.CatalogService.Domain.Repositories.External;

public interface IPlatformCurrencyPolicyRefRepository
{
    Task<PlatformCurrencyPolicyRef?> GetCurrentAsync(CancellationToken cancellationToken = default);
    void Add(PlatformCurrencyPolicyRef policyRef);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
