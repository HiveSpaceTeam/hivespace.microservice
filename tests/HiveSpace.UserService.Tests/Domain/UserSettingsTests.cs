using FluentAssertions;
using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Domain.Enums;
using Xunit;

namespace HiveSpace.UserService.Tests.Domain;

public class UserSettingsTests
{
    [Fact]
    public void Default_ReturnsLightThemeAndViCulture()
    {
        var defaults = UserSettings.Default;
        defaults.Theme.Should().Be(Theme.Light);
        defaults.Culture.Should().Be(Culture.Vi);
    }

    [Fact]
    public void TwoDefaultSettings_AreEqual()
    {
        var a = UserSettings.Default;
        var b = UserSettings.Default;
        a.Should().Be(b);
    }

    [Fact]
    public void SettingsWithDifferentTheme_AreNotEqual()
    {
        var light = new UserSettings(Theme.Light, Culture.Vi);
        var dark = new UserSettings(Theme.Dark, Culture.Vi);
        light.Should().NotBe(dark);
    }

    [Fact]
    public void WithTheme_ReturnsNewInstanceWithUpdatedTheme()
    {
        var original = UserSettings.Default;
        var updated = original.WithTheme(Theme.Dark);
        updated.Theme.Should().Be(Theme.Dark);
        updated.Culture.Should().Be(original.Culture);
        original.Theme.Should().Be(Theme.Light);
    }

    [Fact]
    public void WithCulture_ReturnsNewInstanceWithUpdatedCulture()
    {
        var original = UserSettings.Default;
        var updated = original.WithCulture(Culture.En);
        updated.Culture.Should().Be(Culture.En);
        updated.Theme.Should().Be(original.Theme);
        original.Culture.Should().Be(Culture.Vi);
    }
}
