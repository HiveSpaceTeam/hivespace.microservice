using FluentAssertions;
using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Domain.Enums;
using HiveSpace.UserService.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HiveSpace.UserService.Tests.Application.Settings;

public class UpdateNotificationPreferenceCommandHandlerTests : IClassFixture<UserServiceFixture>
{
    private readonly UserServiceFixture _fixture;

    public UpdateNotificationPreferenceCommandHandlerTests(UserServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Handle_UpdateTheme_PersistsNewTheme()
    {
        var user = NewUser("theme-pref@hivespace.local");
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        user.UpdateTheme(Theme.Dark);
        await _fixture.DbContext.SaveChangesAsync();

        user.Settings.Theme.Should().Be(Theme.Dark);
    }

    [Fact]
    public async Task Handle_UpdateCulture_PersistsNewCulture()
    {
        var user = NewUser("culture-pref@hivespace.local");
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        user.UpdateCulture(Culture.En);
        await _fixture.DbContext.SaveChangesAsync();

        var stored = await _fixture.DbContext.Users.SingleAsync(u => u.Id == user.Id);
        stored.Settings.Culture.Should().Be(Culture.En);
    }

    private static User NewUser(string email) =>
        User.CreateProfile(Guid.NewGuid(), Email.Create(email), email, "Test User");
}
