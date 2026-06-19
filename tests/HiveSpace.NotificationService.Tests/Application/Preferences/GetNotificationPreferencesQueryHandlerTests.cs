using FluentAssertions;
using HiveSpace.NotificationService.Core.DomainModels;
using HiveSpace.NotificationService.Core.Features.Preferences.Queries.GetPreferences;
using HiveSpace.NotificationService.Core.Persistence.Repositories;
using HiveSpace.NotificationService.Tests.Fixtures;
using HiveSpace.Testing.Shared.Doubles;
using Xunit;

namespace HiveSpace.NotificationService.Tests.Application.Preferences;

public class GetNotificationPreferencesQueryHandlerTests : IClassFixture<NotificationServiceFixture>
{
    private readonly NotificationServiceFixture _fixture;

    public GetNotificationPreferencesQueryHandlerTests(NotificationServiceFixture fixture)
        => _fixture = fixture;

    private GetPreferencesQueryHandler CreateHandler(Guid userId, string role = "Buyer")
        => new(new UserPreferenceRepository(_fixture.DbContext),
               new FakeUserContext { UserId = userId, Roles = [role] });

    [Fact]
    public async Task Handle_ReturnsEntryForEveryChannel()
    {
        var userId = Guid.NewGuid();
        var result = await CreateHandler(userId).Handle(new GetPreferencesQuery(), default);

        result.Should().HaveCount(Enum.GetValues<NotificationChannel>().Length);
    }

    [Fact]
    public async Task Handle_InAppDefaultsToEnabled_WhenNoPreferenceStored()
    {
        var userId = Guid.NewGuid();
        var result = await CreateHandler(userId).Handle(new GetPreferencesQuery(), default);

        result.Single(r => r.Channel == NotificationChannel.InApp).Enabled.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ReflectsStoredChannelPreference()
    {
        var userId = Guid.NewGuid();
        _fixture.DbContext.UserChannelPreferences.Add(
            UserChannelPreference.Create(userId, NotificationChannel.Email, true));
        await _fixture.DbContext.SaveChangesAsync();

        var result = await CreateHandler(userId).Handle(new GetPreferencesQuery(), default);

        result.Single(r => r.Channel == NotificationChannel.Email).Enabled.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_GroupsMatchAllowedGroupsForBuyer()
    {
        var userId = Guid.NewGuid();
        var result = await CreateHandler(userId, "Buyer").Handle(new GetPreferencesQuery(), default);

        var inApp = result.Single(r => r.Channel == NotificationChannel.InApp);
        inApp.Groups.Select(g => g.EventGroup).Should().BeEquivalentTo(NotificationEventGroup.BuyerGroups);
    }

    [Fact]
    public async Task Handle_GroupsMatchAllowedGroupsForSeller()
    {
        var userId = Guid.NewGuid();
        var result = await CreateHandler(userId, "Seller").Handle(new GetPreferencesQuery(), default);

        var inApp = result.Single(r => r.Channel == NotificationChannel.InApp);
        inApp.Groups.Select(g => g.EventGroup).Should().BeEquivalentTo(NotificationEventGroup.SellerGroups);
    }

    [Fact]
    public async Task Handle_WhenGroupPrefStored_ReflectsEnabledState()
    {
        var userId = Guid.NewGuid();
        _fixture.DbContext.UserGroupPreferences.Add(
            UserGroupPreference.Create(userId, NotificationChannel.InApp, NotificationEventGroup.OrderUpdates, enabled: true));
        await _fixture.DbContext.SaveChangesAsync();

        var result = await CreateHandler(userId, "Buyer").Handle(new GetPreferencesQuery(), default);

        var inApp = result.Single(r => r.Channel == NotificationChannel.InApp);
        inApp.Groups.Single(g => g.EventGroup == NotificationEventGroup.OrderUpdates).Enabled.Should().BeTrue();
    }
}
