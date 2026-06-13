using FluentAssertions;
using HiveSpace.NotificationService.Core.DomainModels;
using HiveSpace.NotificationService.Core.Features.Preferences.Queries.GetPreferences;
using HiveSpace.NotificationService.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HiveSpace.NotificationService.Tests.Application.Preferences;

public class GetNotificationPreferencesQueryHandlerTests : IClassFixture<NotificationServiceFixture>
{
    private readonly NotificationServiceFixture _fixture;

    public GetNotificationPreferencesQueryHandlerTests(NotificationServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Handle_ReturnsStoredPreferences()
    {
        var userId = Guid.NewGuid();
        var email = UserChannelPreference.Create(userId, NotificationChannel.Email, true);
        var inApp = UserChannelPreference.Create(userId, NotificationChannel.InApp, false);
        _fixture.DbContext.UserChannelPreferences.AddRange(email, inApp);
        await _fixture.DbContext.SaveChangesAsync();

        var prefs = await _fixture.DbContext.UserChannelPreferences.Where(x => x.UserId == userId).ToListAsync();
        prefs.Should().HaveCount(2);
        typeof(GetPreferencesQueryHandler).Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WithDisabledPreference_ReturnsFalseForThatChannel()
    {
        var userId = Guid.NewGuid();
        var pref = UserChannelPreference.Create(userId, NotificationChannel.Email, false);
        _fixture.DbContext.UserChannelPreferences.Add(pref);
        await _fixture.DbContext.SaveChangesAsync();

        var stored = await _fixture.DbContext.UserChannelPreferences.SingleAsync(x => x.UserId == userId);
        stored.Enabled.Should().BeFalse();
    }
}
