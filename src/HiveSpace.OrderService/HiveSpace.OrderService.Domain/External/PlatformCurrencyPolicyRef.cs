using HiveSpace.Domain.Shared.Entities;

namespace HiveSpace.OrderService.Domain.External;

public class PlatformCurrencyPolicyRef : AggregateRoot<Guid>
{
    public string DefaultCurrencyCode { get; private set; } = "VND";
    public long Version { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public string EnabledCurrencyCodes { get; private set; } = "VND";

    private PlatformCurrencyPolicyRef()
    {
    }

    public PlatformCurrencyPolicyRef(Guid id, string defaultCurrencyCode, long version, DateTimeOffset updatedAt, IEnumerable<string> enabledCurrencyCodes)
    {
        Id = id;
        Update(defaultCurrencyCode, version, updatedAt, enabledCurrencyCodes);
    }

    public void Update(string defaultCurrencyCode, long version, DateTimeOffset updatedAt, IEnumerable<string> enabledCurrencyCodes)
    {
        DefaultCurrencyCode = defaultCurrencyCode.ToUpperInvariant();
        Version = version;
        UpdatedAt = updatedAt;
        EnabledCurrencyCodes = string.Join(",", enabledCurrencyCodes.Select(x => x.ToUpperInvariant()).Distinct().OrderBy(x => x));
    }

    public bool IsCurrencyEnabled(string currencyCode)
        => EnabledCurrencyCodes.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Contains(currencyCode.ToUpperInvariant(), StringComparer.OrdinalIgnoreCase);
}
