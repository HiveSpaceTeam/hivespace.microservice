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

public class EmailVerifiedConsumerTests
{
    private readonly IDispatchPipeline _pipeline = Substitute.For<IDispatchPipeline>();
    private readonly ILogger<EmailVerifiedConsumer> _logger = Substitute.For<ILogger<EmailVerifiedConsumer>>();
    private readonly EmailVerifiedConsumer _consumer;

    public EmailVerifiedConsumerTests()
    {
        _consumer = new EmailVerifiedConsumer(_pipeline, _logger);
    }

    [Fact]
    public async Task Consume_DispatchesEmailVerifiedNotification()
    {
        var msg = new UserEmailVerifiedIntegrationEvent
        {
            UserId = Guid.NewGuid(),
            ToEmail = "user@example.com",
            ToName = "Test User",
            Locale = Culture.Vi
        };
        var ctx = Substitute.For<ConsumeContext<UserEmailVerifiedIntegrationEvent>>();
        ctx.Message.Returns(msg);
        ctx.CancellationToken.Returns(CancellationToken.None);

        await _consumer.Consume(ctx);

        await _pipeline.Received(1).DispatchAsync(
            Arg.Is<NotificationRequest>(r =>
                r.EventType == NotificationEventType.EmailVerified &&
                r.UserId == msg.UserId),
            Arg.Any<CancellationToken>());
    }
}
