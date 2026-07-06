using FluentAssertions;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.UserService.Domain.Aggregates.Configuration;
using Xunit;

namespace HiveSpace.UserService.Tests.Domain.Configuration;

public class PlatformConfigTests
{
    [Fact]
    public void DisableCurrentDefault_Rejected()
    {
        var config = PlatformConfig.CreateCurrencyPolicy("VND");
        var currencies = new[]
        {
            PlatformCurrency.CreateCurrency("VND", false, 0),
            PlatformCurrency.CreateCurrency("USD", true, 1),
            PlatformCurrency.CreateCurrency("EUR", false, 2)
        };

        var act = () => config.UpdateCurrencyPolicy("VND", currencies);

        act.Should().Throw<ConflictException>();
    }

    [Fact]
    public void ChangeDefault_DisabledValueRejected()
    {
        var config = PlatformConfig.CreateCurrencyPolicy("VND");
        var currencies = new[]
        {
            PlatformCurrency.CreateCurrency("VND", true, 0),
            PlatformCurrency.CreateCurrency("USD", false, 1),
            PlatformCurrency.CreateCurrency("EUR", false, 2)
        };

        var act = () => config.UpdateCurrencyPolicy("USD", currencies);

        act.Should().Throw<ConflictException>();
    }
}
