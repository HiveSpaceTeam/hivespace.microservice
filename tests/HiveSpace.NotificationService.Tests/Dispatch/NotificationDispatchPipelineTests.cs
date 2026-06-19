using FluentAssertions;
using HiveSpace.NotificationService.Core.Dispatch;
using HiveSpace.NotificationService.Core.Dispatch.Models;
using HiveSpace.NotificationService.Core.DomainModels;
using HiveSpace.NotificationService.Core.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace HiveSpace.NotificationService.Tests.Dispatch;

public class NotificationDispatchPipelineTests
{
    private readonly IDeduplicationService _dedup = Substitute.For<IDeduplicationService>();
    private readonly IUserPreferenceRepository _prefs = Substitute.For<IUserPreferenceRepository>();
    private readonly INotificationRepository _repo = Substitute.For<INotificationRepository>();
    private readonly IChannelRouter _router = Substitute.For<IChannelRouter>();

    private NotificationDispatchPipeline CreatePipeline()
        => new(_dedup, _prefs, _repo, _router, NullLogger<NotificationDispatchPipeline>.Instance);

    private static NotificationRequest MakeRequest(bool transactional = false, string? key = null)
        => new()
        {
            UserId          = Guid.NewGuid(),
            EventType       = "order.confirmed",
            IdempotencyKey  = key ?? $"key-{Guid.NewGuid()}",
            TemplateData    = new Dictionary<string, object>(),
            IsTransactional = transactional,
        };

    [Fact]
    public async Task DispatchAsync_DuplicateKey_SkipsDelivery()
    {
        _dedup.IsDuplicateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true));

        await CreatePipeline().DispatchAsync(MakeRequest(), default);

        await _router.DidNotReceive().SendAsync(
            Arg.Any<Notification>(), Arg.Any<Dictionary<string, object>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DispatchAsync_TransactionalRequest_SendsToEmailOnly()
    {
        _dedup.IsDuplicateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));

        await CreatePipeline().DispatchAsync(MakeRequest(transactional: true), default);

        await _router.Received(1).SendAsync(
            Arg.Is<Notification>(n => n.Channel == NotificationChannel.Email),
            Arg.Any<Dictionary<string, object>>(),
            Arg.Any<CancellationToken>());
        await _repo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DispatchAsync_NoEnabledChannels_SkipsDelivery()
    {
        _dedup.IsDuplicateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));
        _prefs.GetEnabledChannelsAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<NotificationChannel>>([]));

        await CreatePipeline().DispatchAsync(MakeRequest(), default);

        await _router.DidNotReceive().SendAsync(
            Arg.Any<Notification>(), Arg.Any<Dictionary<string, object>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DispatchAsync_WithEnabledChannels_SendsPerChannelAndPersists()
    {
        _dedup.IsDuplicateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));
        _prefs.GetEnabledChannelsAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<NotificationChannel>>(
                [NotificationChannel.Email, NotificationChannel.InApp]));

        await CreatePipeline().DispatchAsync(MakeRequest(), default);

        await _router.Received(2).SendAsync(
            Arg.Any<Notification>(), Arg.Any<Dictionary<string, object>>(), Arg.Any<CancellationToken>());
        await _repo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
