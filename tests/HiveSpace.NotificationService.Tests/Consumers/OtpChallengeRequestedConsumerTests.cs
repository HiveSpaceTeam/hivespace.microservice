using FluentAssertions;
using HiveSpace.Infrastructure.Messaging.Shared.Events.Users;
using HiveSpace.NotificationService.Api.Consumers;
using HiveSpace.NotificationService.Core.Dispatch.Models;
using HiveSpace.NotificationService.Core.Interfaces;
using HiveSpace.Domain.Shared.Exceptions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace HiveSpace.NotificationService.Tests.Consumers;

public class OtpChallengeRequestedConsumerTests
{
    [Fact]
    public async Task Consume_SignInPurpose_SendsOtpEmail()
    {
        var pipeline = new RecordingDispatchPipeline();
        var consumer = new OtpChallengeRequestedConsumer(
            pipeline,
            NullLogger<OtpChallengeRequestedConsumer>.Instance);

        await consumer.ConsumeMessageAsync(new UserOtpChallengeRequestedIntegrationEvent
        {
            RecipientEmail = "otp@hivespace.local",
            OtpCode = "123456",
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(10),
            Purpose = "SignIn"
        });

        pipeline.Requests.Should().ContainSingle();
        pipeline.Requests.Single().EventType.Should().Be("user.otp.signin");
        pipeline.Requests.Single().IsTransactional.Should().BeTrue();
        pipeline.Requests.Single().TemplateData["_recipientEmail"].Should().Be("otp@hivespace.local");
        pipeline.Requests.Single().TemplateData["otpCode"].Should().Be("123456");
    }

    [Fact]
    public async Task Consume_DuplicateMessage_RemainsIdempotent()
    {
        var pipeline = new RecordingDispatchPipeline();
        var consumer = new OtpChallengeRequestedConsumer(
            pipeline,
            NullLogger<OtpChallengeRequestedConsumer>.Instance);
        var message = new UserOtpChallengeRequestedIntegrationEvent
        {
            RecipientEmail = "dup-otp@hivespace.local",
            OtpCode = "123456",
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(10),
            Purpose = "SignIn"
        };

        await consumer.ConsumeMessageAsync(message);
        await consumer.ConsumeMessageAsync(message);

        pipeline.Requests.Should().HaveCount(2);
        pipeline.Requests[0].IdempotencyKey.Should().Be(pipeline.Requests[1].IdempotencyKey);
    }

    [Fact]
    public async Task Consume_UnknownPurpose_FailsObservably()
    {
        var consumer = new OtpChallengeRequestedConsumer(
            new RecordingDispatchPipeline(),
            NullLogger<OtpChallengeRequestedConsumer>.Instance);
        var message = new UserOtpChallengeRequestedIntegrationEvent
        {
            RecipientEmail = "unsupported@hivespace.local",
            OtpCode = "999999",
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(10),
            Purpose = "PasswordReset"
        };

        var act = () => consumer.ConsumeMessageAsync(message);

        await act.Should().ThrowAsync<InvalidFieldException>();
    }

    private sealed class RecordingDispatchPipeline : IDispatchPipeline
    {
        public List<NotificationRequest> Requests { get; } = [];

        public Task DispatchAsync(NotificationRequest request, CancellationToken ct = default)
        {
            Requests.Add(request);
            return Task.CompletedTask;
        }
    }
}
