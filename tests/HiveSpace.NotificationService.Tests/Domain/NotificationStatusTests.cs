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

    [Fact]
    public void MarkThrottled_TransitionsToThrottled()
    {
        var notification = Notification.Create(Guid.NewGuid(), NotificationChannel.InApp, "order.updated", "key-1", "{}");

        notification.MarkThrottled();

        notification.Status.Should().Be(NotificationStatus.Throttled);
    }

    [Fact]
    public void MarkDead_TransitionsToDeadWithErrorMessage()
    {
        var notification = Notification.Create(Guid.NewGuid(), NotificationChannel.Email, "order.updated", "key-1", "{}");

        notification.MarkDead("max retries exceeded");

        notification.Status.Should().Be(NotificationStatus.Dead);
        notification.ErrorMessage.Should().Be("max retries exceeded");
    }

    [Fact]
    public void MarkRead_TransitionsToRead()
    {
        var notification = Notification.Create(Guid.NewGuid(), NotificationChannel.InApp, "order.updated", "key-1", "{}");

        notification.MarkRead();

        notification.Status.Should().Be(NotificationStatus.Read);
        notification.ReadAt.Should().NotBeNull();
    }

    [Fact]
    public void IncrementAttempt_IncrementsAttemptCount()
    {
        var notification = Notification.Create(Guid.NewGuid(), NotificationChannel.InApp, "order.updated", "key-1", "{}");

        notification.IncrementAttempt();
        notification.IncrementAttempt();

        notification.AttemptCount.Should().Be(2);
    }

    [Fact]
    public void AddAttempt_AppendsToAttemptsList()
    {
        var notification = Notification.Create(Guid.NewGuid(), NotificationChannel.InApp, "order.updated", "key-1", "{}");
        var attempt = DeliveryAttempt.Create(notification.Id, attemptNumber: 1, success: true, providerResponse: null, errorMessage: null);

        notification.AddAttempt(attempt);

        notification.Attempts.Should().ContainSingle().Which.Should().Be(attempt);
    }
}
