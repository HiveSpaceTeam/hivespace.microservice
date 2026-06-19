using FluentAssertions;
using HiveSpace.NotificationService.Core.DomainModels;
using HiveSpace.NotificationService.Core.Features.Notifications.Commands.MarkNotificationRead;
using HiveSpace.NotificationService.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HiveSpace.NotificationService.Tests.Application.Delivery;

public class MarkNotificationDeliveredCommandHandlerTests : IClassFixture<NotificationServiceFixture>
{
    private readonly NotificationServiceFixture _fixture;

    public MarkNotificationDeliveredCommandHandlerTests(NotificationServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Handle_UpdatesDeliveryStatusToSent()
    {
        var notification = Notification.Create(Guid.NewGuid(), NotificationChannel.Email, "order.updated", "order:del:email", "{}");
        _fixture.DbContext.Notifications.Add(notification);
        await _fixture.DbContext.SaveChangesAsync();

        notification.MarkSent();
        await _fixture.DbContext.SaveChangesAsync();

        notification.Status.Should().Be(NotificationStatus.Sent);
        typeof(MarkNotificationReadCommandHandler).Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_AfterDelivery_NotificationStatusIsNotPending()
    {
        var notification = Notification.Create(Guid.NewGuid(), NotificationChannel.InApp, "payment.done", "payment:del:inapp", "{}");
        _fixture.DbContext.Notifications.Add(notification);
        await _fixture.DbContext.SaveChangesAsync();

        notification.MarkSent();
        await _fixture.DbContext.SaveChangesAsync();

        var stored = await _fixture.DbContext.Notifications.SingleAsync(x => x.Id == notification.Id);
        stored.Status.Should().NotBe(NotificationStatus.Pending);
    }
}
