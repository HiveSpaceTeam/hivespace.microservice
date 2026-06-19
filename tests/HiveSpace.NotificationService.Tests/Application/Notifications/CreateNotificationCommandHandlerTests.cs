using FluentAssertions;
using HiveSpace.NotificationService.Core.DomainModels;
using HiveSpace.NotificationService.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HiveSpace.NotificationService.Tests.Application.Notifications;

public class CreateNotificationCommandHandlerTests : IClassFixture<NotificationServiceFixture>
{
    private readonly NotificationServiceFixture _fixture;

    public CreateNotificationCommandHandlerTests(NotificationServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Handle_StoresNotificationWithPendingDeliveryStatus()
    {
        var userId = Guid.NewGuid();
        var notification = Notification.Create(userId, NotificationChannel.InApp,
            "order.confirmed", $"idem-{Guid.NewGuid()}", "{\"orderId\":\"123\"}");
        _fixture.DbContext.Notifications.Add(notification);
        await _fixture.DbContext.SaveChangesAsync();

        var stored = await _fixture.DbContext.Notifications.SingleAsync(n => n.UserId == userId);
        stored.Status.Should().Be(NotificationStatus.Pending, "notifications start in Pending delivery status");
        stored.Channel.Should().Be(NotificationChannel.InApp);
    }
}
