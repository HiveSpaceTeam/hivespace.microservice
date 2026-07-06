using HiveSpace.Infrastructure.Messaging.Events;

namespace HiveSpace.Infrastructure.Messaging.Shared.Events.Users;

public record PlatformCurrencyPolicyUpdatedIntegrationEvent : IntegrationEvent
{
    public Guid PolicyId { get; init; }
    public string DefaultCurrencyCode { get; init; } = string.Empty;
    public long Version { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
    public IReadOnlyCollection<CurrencyPolicyItem> Currencies { get; init; } = [];

    public record CurrencyPolicyItem
    {
        public string CurrencyCode { get; init; } = string.Empty;
        public bool IsEnabled { get; init; }
    }
}
