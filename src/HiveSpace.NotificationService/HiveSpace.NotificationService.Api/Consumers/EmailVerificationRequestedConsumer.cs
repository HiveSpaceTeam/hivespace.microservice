using HiveSpace.Infrastructure.Messaging.Shared.Events.Users;
using HiveSpace.NotificationService.Core.Dispatch.Models;
using HiveSpace.NotificationService.Core.DomainModels;
using HiveSpace.NotificationService.Core.Interfaces;
using MassTransit;

namespace HiveSpace.NotificationService.Api.Consumers;

public class EmailVerificationRequestedConsumer(
    IDispatchPipeline pipeline,
    ILogger<EmailVerificationRequestedConsumer> logger) : IConsumer<UserEmailVerificationRequestedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<UserEmailVerificationRequestedIntegrationEvent> context)
    {
        var msg = context.Message;

        await pipeline.DispatchAsync(new NotificationRequest
        {
            UserId          = msg.UserId,
            EventType       = NotificationEventType.EmailVerificationRequested,
            IdempotencyKey  = $"user.email.verification:{msg.UserId}:{msg.ExpiresAt:yyyyMMddHHmmss}",
            Locale          = msg.Locale,
            IsTransactional = true,
            TemplateData    = new Dictionary<string, object>
            {
                ["userName"]          = msg.ToName,
                ["verificationLink"]  = msg.VerificationLink,
                ["expiresAt"]         = msg.ExpiresAt.ToString("f"),
                ["_recipientEmail"]   = msg.ToEmail,
            }
        }, context.CancellationToken);

        logger.LogInformation("Email verification dispatched for UserId={UserId}", msg.UserId);
    }
}
