using HiveSpace.PaymentService.Domain.Aggregates.External;

namespace HiveSpace.PaymentService.Domain.Repositories;

public interface IPlatformCurrencyPolicyRefRepository
{
    Task<PlatformCurrencyPolicyRef?> GetCurrentAsync(CancellationToken cancellationToken = default);
    void Add(PlatformCurrencyPolicyRef policyRef);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
