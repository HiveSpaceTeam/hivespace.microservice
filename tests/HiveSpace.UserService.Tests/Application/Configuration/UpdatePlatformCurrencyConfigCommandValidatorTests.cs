using FluentAssertions;
using HiveSpace.UserService.Application.Configuration.Commands.UpdatePlatformCurrencyConfig;
using Xunit;

namespace HiveSpace.UserService.Tests.Application.Configuration;

public class UpdatePlatformCurrencyConfigCommandValidatorTests
{
    private readonly UpdatePlatformCurrencyConfigCommandValidator _validator = new();

    [Fact]
    public void Validate_WhenCurrencySetIsIncomplete_ReturnsInvalid()
    {
        var command = new UpdatePlatformCurrencyConfigCommand(
            [
                new PlatformCurrencyConfigItemRequest("VND", true),
                new PlatformCurrencyConfigItemRequest("USD", false)
            ],
            "VND",
            1);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_WhenCurrencyCodesContainDuplicates_ReturnsInvalid()
    {
        var command = new UpdatePlatformCurrencyConfigCommand(
            [
                new PlatformCurrencyConfigItemRequest("VND", true),
                new PlatformCurrencyConfigItemRequest("USD", true),
                new PlatformCurrencyConfigItemRequest("USD", false)
            ],
            "VND",
            1);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_WhenDefaultCurrencyIsDisabled_ReturnsInvalid()
    {
        var command = new UpdatePlatformCurrencyConfigCommand(
            [
                new PlatformCurrencyConfigItemRequest("VND", true),
                new PlatformCurrencyConfigItemRequest("USD", false),
                new PlatformCurrencyConfigItemRequest("EUR", false)
            ],
            "USD",
            1);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_WhenCurrencySetIsCompleteAndValid_ReturnsValid()
    {
        var command = new UpdatePlatformCurrencyConfigCommand(
            [
                new PlatformCurrencyConfigItemRequest("VND", true),
                new PlatformCurrencyConfigItemRequest("USD", true),
                new PlatformCurrencyConfigItemRequest("EUR", false)
            ],
            "USD",
            1);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }
}
