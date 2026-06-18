using HiveSpace.NotificationService.Core.BackgroundJobs;
using HiveSpace.NotificationService.Core.DomainModels;
using HiveSpace.NotificationService.Core.Interfaces;
using NSubstitute;
using Xunit;

namespace HiveSpace.NotificationService.Tests.BackgroundJobs;

public class RetryNotificationJobTests
{
    private readonly INotificationRepository _repo = Substitute.For<INotificationRepository>();
    private readonly IChannelRouter _router = Substitute.For<IChannelRouter>();

    private RetryNotificationJob CreateJob() => new(_repo, _router);

    private static Notification MakeFailed()
    {
        var n = Notification.Create(Guid.NewGuid(), NotificationChannel.Email, "order.placed", $"k-{Guid.NewGuid()}", "{}");
        n.MarkFailed("smtp error");
        return n;
    }

    [Fact]
    public async Task ExecuteAsync_NotificationNotFound_DoesNotRoute()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Notification?>(null));

        await CreateJob().ExecuteAsync(Guid.NewGuid());

        await _router.DidNotReceive().SendAsync(
            Arg.Any<Notification>(), Arg.Any<Dictionary<string, object>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_FailedNotification_RetriesViaRouter()
    {
        var notification = MakeFailed();
        _repo.GetByIdAsync(notification.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Notification?>(notification));

        await CreateJob().ExecuteAsync(notification.Id);

        await _router.Received(1).SendAsync(
            notification, Arg.Any<Dictionary<string, object>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ThrottledNotification_RetriesViaRouter()
    {
        var notification = Notification.Create(
            Guid.NewGuid(), NotificationChannel.InApp, "order.placed", $"k-{Guid.NewGuid()}", "{}");
        notification.MarkThrottled();
        _repo.GetByIdAsync(notification.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Notification?>(notification));

        await CreateJob().ExecuteAsync(notification.Id);

        await _router.Received(1).SendAsync(
            notification, Arg.Any<Dictionary<string, object>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_SentNotification_SkipsRetry()
    {
        var notification = Notification.Create(
            Guid.NewGuid(), NotificationChannel.InApp, "order.placed", $"k-{Guid.NewGuid()}", "{}");
        notification.MarkSent();
        _repo.GetByIdAsync(notification.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Notification?>(notification));

        await CreateJob().ExecuteAsync(notification.Id);

        await _router.DidNotReceive().SendAsync(
            Arg.Any<Notification>(), Arg.Any<Dictionary<string, object>>(), Arg.Any<CancellationToken>());
    }
}
