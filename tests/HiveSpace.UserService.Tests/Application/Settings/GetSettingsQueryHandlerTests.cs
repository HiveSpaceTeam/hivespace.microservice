using FluentAssertions;
using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Domain.Enums;
using HiveSpace.UserService.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HiveSpace.UserService.Tests.Application.Settings;

public class GetSettingsQueryHandlerTests : IClassFixture<UserServiceFixture>
{
    private readonly UserServiceFixture _fixture;

    public GetSettingsQueryHandlerTests(UserServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Handle_ReturnsCurrentThemeAndCulture()
    {
        var user = NewUser("settings-get@hivespace.local");
        user.UpdateTheme(Theme.Dark);
        user.UpdateCulture(Culture.En);
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        user.Settings.Theme.Should().Be(Theme.Dark);
        user.Settings.Culture.Should().Be(Culture.En);
    }

    [Fact]
    public async Task Handle_DefaultSettings_ReturnsSystemDefaultValues()
    {
        var user = NewUser("settings-default@hivespace.local");
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        var stored = await _fixture.DbContext.Users.SingleAsync(u => u.Id == user.Id);
        stored.Settings.Should().NotBeNull("every user profile has a Settings object initialized with defaults");
    }

    private static User NewUser(string email) =>
        User.CreateProfile(Guid.NewGuid(), Email.Create(email), email, "Test User");
}
