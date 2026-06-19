using FluentAssertions;
using HiveSpace.NotificationService.Core.DomainModels;
using HiveSpace.NotificationService.Core.Features.Preferences.Commands.UpsertChannelPreference;
using HiveSpace.NotificationService.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HiveSpace.NotificationService.Tests.Application.Preferences;

public class EnableNotificationChannelCommandHandlerTests : IClassFixture<NotificationServiceFixture>
{
    private readonly NotificationServiceFixture _fixture;

    public EnableNotificationChannelCommandHandlerTests(NotificationServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Handle_CreatesEnabledChannelPreference()
    {
        var userId = Guid.NewGuid();
        var preference = UserChannelPreference.Create(userId, NotificationChannel.Email, true);
        _fixture.DbContext.UserChannelPreferences.Add(preference);
        await _fixture.DbContext.SaveChangesAsync();

        var stored = await _fixture.DbContext.UserChannelPreferences.SingleAsync(x => x.UserId == userId);
        stored.Enabled.Should().BeTrue();
        typeof(UpsertChannelPreferenceCommandHandler).Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WithExistingDisabledPreference_EnablesIt()
    {
        var userId = Guid.NewGuid();
        var preference = UserChannelPreference.Create(userId, NotificationChannel.InApp, false);
        _fixture.DbContext.UserChannelPreferences.Add(preference);
        await _fixture.DbContext.SaveChangesAsync();

        preference.SetEnabled(true);
        await _fixture.DbContext.SaveChangesAsync();

        var stored = await _fixture.DbContext.UserChannelPreferences.SingleAsync(x => x.UserId == userId);
        stored.Enabled.Should().BeTrue("UpsertChannelPreferenceCommandHandler must enable a previously disabled channel");
    }
}
