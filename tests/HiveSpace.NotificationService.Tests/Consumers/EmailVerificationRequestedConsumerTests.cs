using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.Infrastructure.Messaging.Shared.Events.Users;
using HiveSpace.NotificationService.Core.Dispatch.Models;
using HiveSpace.NotificationService.Api.Consumers;
using HiveSpace.NotificationService.Core.DomainModels;
using HiveSpace.NotificationService.Core.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace HiveSpace.NotificationService.Tests.Consumers;

public class EmailVerificationRequestedConsumerTests
{
    private readonly IDispatchPipeline _pipeline = Substitute.For<IDispatchPipeline>();
    private readonly ILogger<EmailVerificationRequestedConsumer> _logger = Substitute.For<ILogger<EmailVerificationRequestedConsumer>>();
    private readonly EmailVerificationRequestedConsumer _consumer;

    public EmailVerificationRequestedConsumerTests()
    {
        _consumer = new EmailVerificationRequestedConsumer(_pipeline, _logger);
    }

    [Fact]
    public async Task Consume_DispatchesEmailVerificationNotification()
    {
        var msg = new UserEmailVerificationRequestedIntegrationEvent
        {
            UserId = Guid.NewGuid(),
            ToEmail = "user@example.com",
            ToName = "Test User",
            VerificationLink = "https://example.com/verify",
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            Locale = Culture.Vi
        };
        var ctx = Substitute.For<ConsumeContext<UserEmailVerificationRequestedIntegrationEvent>>();
        ctx.Message.Returns(msg);
        ctx.CancellationToken.Returns(CancellationToken.None);

        await _consumer.Consume(ctx);

        await _pipeline.Received(1).DispatchAsync(
            Arg.Is<NotificationRequest>(r =>
                r.EventType == NotificationEventType.EmailVerificationRequested &&
                r.UserId == msg.UserId),
            Arg.Any<CancellationToken>());
    }
}
