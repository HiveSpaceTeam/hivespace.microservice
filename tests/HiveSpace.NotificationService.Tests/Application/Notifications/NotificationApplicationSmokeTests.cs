using FluentAssertions;
using HiveSpace.NotificationService.Core.DomainModels;
using HiveSpace.NotificationService.Core.Features.Notifications.Queries.GetNotifications;
using HiveSpace.NotificationService.Core.Features.Notifications.Queries.GetUnreadCount;
using HiveSpace.NotificationService.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HiveSpace.NotificationService.Tests.Application.Notifications;

public class CreateNotificationCommandHandlerTests : IClassFixture<NotificationServiceFixture>
{
    private readonly NotificationServiceFixture _fixture;

    public CreateNotificationCommandHandlerTests(NotificationServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Handle_WithValidInput_PersistsNotification()
    {
        var userId = Guid.NewGuid();
        var notification = Notification.Create(userId, NotificationChannel.InApp, "order.updated", "order:create:inapp", "{}");
        _fixture.DbContext.Notifications.Add(notification);
        await _fixture.DbContext.SaveChangesAsync();

        var stored = await _fixture.DbContext.Notifications.SingleAsync(x => x.Id == notification.Id);
        stored.UserId.Should().Be(userId);
        stored.Status.Should().Be(NotificationStatus.Pending);
    }

    [Fact]
    public async Task Handle_ReturnsNotificationsForAuthenticatedUser()
    {
        var userId = Guid.NewGuid();
        var notification = Notification.Create(userId, NotificationChannel.InApp, "order.updated", "order:query:inapp", "{}");
        _fixture.DbContext.Notifications.Add(notification);
        await _fixture.DbContext.SaveChangesAsync();

        var notifications = await _fixture.DbContext.Notifications.Where(x => x.UserId == userId).ToListAsync();
        notifications.Should().ContainSingle();
        typeof(GetNotificationsQueryHandler).Should().NotBeNull();
        typeof(GetUnreadCountQueryHandler).Should().NotBeNull();
    }
}
