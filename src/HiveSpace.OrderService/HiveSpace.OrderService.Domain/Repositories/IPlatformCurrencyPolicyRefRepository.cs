using HiveSpace.OrderService.Domain.External;

namespace HiveSpace.OrderService.Domain.Repositories;

public interface IPlatformCurrencyPolicyRefRepository
{
    Task<PlatformCurrencyPolicyRef?> GetCurrentAsync(CancellationToken cancellationToken = default);
    void Add(PlatformCurrencyPolicyRef policyRef);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
