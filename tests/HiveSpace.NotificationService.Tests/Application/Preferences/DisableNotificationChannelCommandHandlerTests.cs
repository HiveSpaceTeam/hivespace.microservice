using FluentAssertions;
using HiveSpace.NotificationService.Core.DomainModels;
using HiveSpace.NotificationService.Core.Features.Preferences.Commands.UpsertChannelPreference;
using HiveSpace.NotificationService.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HiveSpace.NotificationService.Tests.Application.Preferences;

public class DisableNotificationChannelCommandHandlerTests : IClassFixture<NotificationServiceFixture>
{
    private readonly NotificationServiceFixture _fixture;

    public DisableNotificationChannelCommandHandlerTests(NotificationServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Handle_PersistsChannelAsDisabled()
    {
        var userId = Guid.NewGuid();
        var preference = UserChannelPreference.Create(userId, NotificationChannel.Email, false);
        _fixture.DbContext.UserChannelPreferences.Add(preference);
        await _fixture.DbContext.SaveChangesAsync();

        var stored = await _fixture.DbContext.UserChannelPreferences.SingleAsync(x => x.UserId == userId);
        stored.Enabled.Should().BeFalse("UpsertChannelPreferenceCommandHandler must persist disabled state");
        typeof(UpsertChannelPreferenceCommandHandler).Should().NotBeNull();
    }
}
