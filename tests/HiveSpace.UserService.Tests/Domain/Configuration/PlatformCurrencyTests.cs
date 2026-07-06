using FluentAssertions;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.UserService.Domain.Aggregates.Configuration;
using Xunit;

namespace HiveSpace.UserService.Tests.Domain.Configuration;

public class PlatformCurrencyTests
{
    [Fact]
    public void EnableCurrency_UnsupportedCodeRejected()
    {
        var act = () => PlatformCurrency.CreateCurrency("JPY", true, 0);

        act.Should().Throw<InvalidFieldException>();
    }
}
