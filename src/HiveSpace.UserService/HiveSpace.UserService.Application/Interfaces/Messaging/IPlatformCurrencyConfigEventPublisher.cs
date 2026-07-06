using HiveSpace.UserService.Domain.Aggregates.Configuration;

namespace HiveSpace.UserService.Application.Interfaces.Messaging;

public interface IPlatformCurrencyConfigEventPublisher
{
    Task PublishPolicyUpdatedAsync(
        PlatformConfig config,
        IReadOnlyCollection<PlatformCurrency> currencies,
        CancellationToken cancellationToken = default);
}
