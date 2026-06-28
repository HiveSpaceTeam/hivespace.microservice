using System.Security.Cryptography;
using System.Text;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Infrastructure.Messaging.Shared.Events.Users;
using HiveSpace.NotificationService.Core.Dispatch.Models;
using HiveSpace.NotificationService.Core.DomainModels;
using HiveSpace.NotificationService.Core.Exceptions;
using HiveSpace.NotificationService.Core.Interfaces;
using MassTransit;

namespace HiveSpace.NotificationService.Api.Consumers;

public class OtpChallengeRequestedConsumer(
    IDispatchPipeline pipeline,
    ILogger<OtpChallengeRequestedConsumer> logger) : IConsumer<UserOtpChallengeRequestedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<UserOtpChallengeRequestedIntegrationEvent> context)
        => await ConsumeMessageAsync(context.Message, context.CancellationToken);

    public async Task ConsumeMessageAsync(
        UserOtpChallengeRequestedIntegrationEvent message,
        CancellationToken cancellationToken = default)
    {
        if (!string.Equals(message.Purpose, "SignIn", StringComparison.Ordinal))
            throw new InvalidFieldException(NotificationDomainErrorCode.UnsupportedOtpPurpose, nameof(message.Purpose));

        var recipientKey = DeriveRecipientKey(message.RecipientEmail);
        await pipeline.DispatchAsync(new NotificationRequest
        {
            UserId = recipientKey,
            EventType = NotificationEventType.OtpSignInRequested,
            IdempotencyKey = $"user.otp.signin:{message.RecipientEmail}:{message.ExpiresAt:yyyyMMddHHmmss}",
            IsTransactional = true,
            TemplateData = new Dictionary<string, object>
            {
                ["otpCode"] = message.OtpCode,
                ["expiresAt"] = message.ExpiresAt.ToString("f"),
                ["_recipientEmail"] = message.RecipientEmail
            }
        }, cancellationToken);

        logger.LogInformation("OTP sign-in email dispatched for RecipientEmail={RecipientEmail}", message.RecipientEmail);
    }

    private static Guid DeriveRecipientKey(string recipientEmail)
    {
        var normalized = recipientEmail.Trim().ToUpperInvariant();
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(normalized));
        var bytes = hash[..16];
        return new Guid(bytes);
    }
}
