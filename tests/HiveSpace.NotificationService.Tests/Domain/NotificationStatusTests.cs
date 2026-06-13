using FluentAssertions;
using HiveSpace.NotificationService.Core.DomainModels;
using Xunit;

namespace HiveSpace.NotificationService.Tests.Domain;

public class NotificationStatusTests
{
    [Fact]
    public void NewNotification_StartsInPendingDeliveryStatus()
    {
        var notification = Notification.Create(Guid.NewGuid(), NotificationChannel.InApp, "order.updated", "key-1", "{}");

        notification.Status.Should().Be(NotificationStatus.Pending);
    }

    [Fact]
    public void MarkDelivered_TransitionsToDelivered()
    {
        var notification = Notification.Create(Guid.NewGuid(), NotificationChannel.InApp, "order.updated", "key-1", "{}");

        notification.MarkSent();

        notification.Status.Should().Be(NotificationStatus.Sent);
    }

    [Fact]
    public void MarkDelivered_WhenAlreadyDelivered_IsIdempotent()
    {
        var notification = Notification.Create(Guid.NewGuid(), NotificationChannel.InApp, "order.updated", "key-1", "{}");

        notification.MarkSent();
        notification.MarkSent();

        notification.Status.Should().Be(NotificationStatus.Sent);
    }

    [Fact]
    public void MarkFailed_TransitionsToFailed()
    {
        var notification = Notification.Create(Guid.NewGuid(), NotificationChannel.Email, "order.updated", "key-1", "{}");

        notification.MarkFailed("smtp unavailable");

        notification.Status.Should().Be(NotificationStatus.Failed);
        notification.ErrorMessage.Should().Be("smtp unavailable");
    }
}
