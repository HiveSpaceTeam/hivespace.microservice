using FluentAssertions;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.NotificationService.Core.DomainModels;
using HiveSpace.NotificationService.Core.Features.Preferences.Commands.UpsertGroupPreference;
using HiveSpace.NotificationService.Core.Persistence.Repositories;
using HiveSpace.NotificationService.Tests.Fixtures;
using HiveSpace.Testing.Shared.Doubles;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HiveSpace.NotificationService.Tests.Application.Preferences;

public class UpsertGroupPreferenceCommandHandlerTests : IClassFixture<NotificationServiceFixture>
{
    private readonly NotificationServiceFixture _fixture;

    public UpsertGroupPreferenceCommandHandlerTests(NotificationServiceFixture fixture)
        => _fixture = fixture;

    private UpsertGroupPreferenceCommandHandler CreateHandler(Guid userId, string role = "Buyer")
        => new(new UserPreferenceRepository(_fixture.DbContext),
               new FakeUserContext { UserId = userId, Roles = [role] });

    [Fact]
    public async Task Handle_CreatesGroupPreference_ForAllowedGroup()
    {
        var userId = Guid.NewGuid();
        await CreateHandler(userId, "Buyer").Handle(
            new UpsertGroupPreferenceCommand(NotificationChannel.InApp, NotificationEventGroup.OrderUpdates, true), default);

        var stored = await _fixture.DbContext.UserGroupPreferences
            .SingleAsync(p => p.UserId == userId
                            && p.Channel == NotificationChannel.InApp
                            && p.EventGroup == NotificationEventGroup.OrderUpdates);
        stored.Enabled.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_DisablesExistingGroupPreference()
    {
        var userId = Guid.NewGuid();
        _fixture.DbContext.UserGroupPreferences.Add(
            UserGroupPreference.Create(userId, NotificationChannel.InApp, NotificationEventGroup.Payment, true));
        await _fixture.DbContext.SaveChangesAsync();

        await CreateHandler(userId, "Buyer").Handle(
            new UpsertGroupPreferenceCommand(NotificationChannel.InApp, NotificationEventGroup.Payment, false), default);

        var stored = await _fixture.DbContext.UserGroupPreferences
            .SingleAsync(p => p.UserId == userId
                            && p.Channel == NotificationChannel.InApp
                            && p.EventGroup == NotificationEventGroup.Payment);
        stored.Enabled.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_GroupNotAllowedForRole_ThrowsInvalidFieldException()
    {
        var userId = Guid.NewGuid();
        var act = () => CreateHandler(userId, "Buyer").Handle(
            new UpsertGroupPreferenceCommand(NotificationChannel.InApp, NotificationEventGroup.SellerOrders, true), default);

        await act.Should().ThrowAsync<InvalidFieldException>();
    }
}
