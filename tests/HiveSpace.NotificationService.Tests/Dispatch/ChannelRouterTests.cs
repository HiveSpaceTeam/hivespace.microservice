using FluentAssertions;
using HiveSpace.NotificationService.Core.Dispatch;
using HiveSpace.NotificationService.Core.Dispatch.Models;
using HiveSpace.NotificationService.Core.DomainModels;
using HiveSpace.NotificationService.Core.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace HiveSpace.NotificationService.Tests.Dispatch;

public class ChannelRouterTests
{
    private readonly IRateLimiter _rateLimiter = Substitute.For<IRateLimiter>();
    private readonly IRetryScheduler _retryScheduler = Substitute.For<IRetryScheduler>();

    private ChannelRouter CreateRouter(params IChannelProvider[] providers)
        => new(providers, _rateLimiter, _retryScheduler, NullLogger<ChannelRouter>.Instance);

    private static Notification MakeNotification(NotificationChannel channel = NotificationChannel.InApp)
        => Notification.Create(Guid.NewGuid(), channel, "order.placed", $"idem-{Guid.NewGuid()}", "{}");

    [Fact]
    public async Task SendAsync_SuccessfulDelivery_MarksNotificationSent()
    {
        var provider = Substitute.For<IChannelProvider>();
        provider.Channel.Returns(NotificationChannel.InApp);
        provider.SendAsync(Arg.Any<Notification>(), Arg.Any<Dictionary<string, object>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(DeliveryResult.Ok()));
        _rateLimiter.AllowAsync(Arg.Any<Guid>(), Arg.Any<NotificationChannel>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true));

        var notification = MakeNotification();
        await CreateRouter(provider).SendAsync(notification, new(), default);

        notification.Status.Should().Be(NotificationStatus.Sent);
        notification.AttemptCount.Should().Be(1);
    }

    [Fact]
    public async Task SendAsync_NoProviderRegistered_MarksNotificationFailed()
    {
        var notification = MakeNotification(NotificationChannel.Email);
        await CreateRouter().SendAsync(notification, new(), default);

        notification.Status.Should().Be(NotificationStatus.Failed);
    }

    [Fact]
    public async Task SendAsync_RateLimited_MarksNotificationThrottled()
    {
        var provider = Substitute.For<IChannelProvider>();
        provider.Channel.Returns(NotificationChannel.InApp);
        _rateLimiter.AllowAsync(Arg.Any<Guid>(), Arg.Any<NotificationChannel>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));

        var notification = MakeNotification();
        await CreateRouter(provider).SendAsync(notification, new(), default);

        notification.Status.Should().Be(NotificationStatus.Throttled);
    }

    [Fact]
    public async Task SendAsync_ProviderReturnsFailure_MarksNotificationFailed()
    {
        var provider = Substitute.For<IChannelProvider>();
        provider.Channel.Returns(NotificationChannel.InApp);
        provider.SendAsync(Arg.Any<Notification>(), Arg.Any<Dictionary<string, object>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(DeliveryResult.Fail("smtp error")));
        _rateLimiter.AllowAsync(Arg.Any<Guid>(), Arg.Any<NotificationChannel>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true));

        var notification = MakeNotification();
        await CreateRouter(provider).SendAsync(notification, new(), default);

        notification.Status.Should().Be(NotificationStatus.Failed);
        notification.ErrorMessage.Should().Be("smtp error");
    }

    [Fact]
    public async Task SendAsync_ProviderThrows_MarksNotificationFailed()
    {
        var provider = Substitute.For<IChannelProvider>();
        provider.Channel.Returns(NotificationChannel.InApp);
        provider.SendAsync(Arg.Any<Notification>(), Arg.Any<Dictionary<string, object>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<DeliveryResult>(new Exception("connection refused")));
        _rateLimiter.AllowAsync(Arg.Any<Guid>(), Arg.Any<NotificationChannel>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true));

        var notification = MakeNotification();
        await CreateRouter(provider).SendAsync(notification, new(), default);

        notification.Status.Should().Be(NotificationStatus.Failed);
    }

    [Fact]
    public async Task SendAsync_FailedAfterMaxAttempts_MarksNotificationDead()
    {
        var provider = Substitute.For<IChannelProvider>();
        provider.Channel.Returns(NotificationChannel.InApp);
        provider.SendAsync(Arg.Any<Notification>(), Arg.Any<Dictionary<string, object>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(DeliveryResult.Fail("persistent error")));
        _rateLimiter.AllowAsync(Arg.Any<Guid>(), Arg.Any<NotificationChannel>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true));

        var notification = MakeNotification();
        // Simulate 4 prior attempts so the 5th crosses the MaxAttempts=5 threshold
        for (var i = 0; i < 4; i++) notification.IncrementAttempt();

        await CreateRouter(provider).SendAsync(notification, new(), default);

        notification.Status.Should().Be(NotificationStatus.Dead);
    }

    [Fact]
    public async Task SendAsync_ProviderReturnsFailureWithNullError_UsesDefaultMessage()
    {
        var provider = Substitute.For<IChannelProvider>();
        provider.Channel.Returns(NotificationChannel.InApp);
        provider.SendAsync(Arg.Any<Notification>(), Arg.Any<Dictionary<string, object>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(DeliveryResult.Fail(null!)));
        _rateLimiter.AllowAsync(Arg.Any<Guid>(), Arg.Any<NotificationChannel>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true));

        var notification = MakeNotification();
        await CreateRouter(provider).SendAsync(notification, new(), default);

        notification.Status.Should().Be(NotificationStatus.Failed);
        notification.ErrorMessage.Should().Be("Provider returned failure");
    }
}
