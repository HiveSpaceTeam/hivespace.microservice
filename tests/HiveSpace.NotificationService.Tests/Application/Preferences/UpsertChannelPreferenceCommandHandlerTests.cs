using FluentAssertions;
using HiveSpace.NotificationService.Core.DomainModels;
using HiveSpace.NotificationService.Core.Features.Preferences.Commands.UpsertChannelPreference;
using HiveSpace.NotificationService.Core.Persistence.Repositories;
using HiveSpace.NotificationService.Tests.Fixtures;
using HiveSpace.Testing.Shared.Doubles;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HiveSpace.NotificationService.Tests.Application.Preferences;

public class UpsertChannelPreferenceCommandHandlerTests : IClassFixture<NotificationServiceFixture>
{
    private readonly NotificationServiceFixture _fixture;

    public UpsertChannelPreferenceCommandHandlerTests(NotificationServiceFixture fixture)
        => _fixture = fixture;

    private UpsertChannelPreferenceCommandHandler CreateHandler(Guid userId)
        => new(new UserPreferenceRepository(_fixture.DbContext),
               new FakeUserContext { UserId = userId });

    [Fact]
    public async Task Handle_CreatesNewChannelPreference()
    {
        var userId = Guid.NewGuid();
        await CreateHandler(userId).Handle(
            new UpsertChannelPreferenceCommand(NotificationChannel.Email, true), default);

        var stored = await _fixture.DbContext.UserChannelPreferences
            .SingleAsync(p => p.UserId == userId && p.Channel == NotificationChannel.Email);
        stored.Enabled.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_UpdatesExistingChannelPreference()
    {
        var userId = Guid.NewGuid();
        _fixture.DbContext.UserChannelPreferences.Add(
            UserChannelPreference.Create(userId, NotificationChannel.InApp, true));
        await _fixture.DbContext.SaveChangesAsync();

        await CreateHandler(userId).Handle(
            new UpsertChannelPreferenceCommand(NotificationChannel.InApp, false), default);

        var stored = await _fixture.DbContext.UserChannelPreferences
            .SingleAsync(p => p.UserId == userId && p.Channel == NotificationChannel.InApp);
        stored.Enabled.Should().BeFalse();
    }
}
