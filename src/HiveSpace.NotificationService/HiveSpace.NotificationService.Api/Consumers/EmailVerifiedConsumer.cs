using HiveSpace.Infrastructure.Messaging.Shared.Events.Users;
using HiveSpace.NotificationService.Core.Dispatch.Models;
using HiveSpace.NotificationService.Core.DomainModels;
using HiveSpace.NotificationService.Core.Interfaces;
using MassTransit;

namespace HiveSpace.NotificationService.Api.Consumers;

public class EmailVerifiedConsumer(
    IDispatchPipeline pipeline,
    ILogger<EmailVerifiedConsumer>    logger) : IConsumer<UserEmailVerifiedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<UserEmailVerifiedIntegrationEvent> context)
    {
        var msg = context.Message;

        await pipeline.DispatchAsync(new NotificationRequest
        {
            UserId          = msg.UserId,
            EventType       = NotificationEventType.EmailVerified,
            IdempotencyKey  = $"user.email.verified:{msg.UserId}",
            Locale          = msg.Locale,
            IsTransactional = true,
            TemplateData    = new Dictionary<string, object>
            {
                ["userName"]        = msg.ToName,
                ["_recipientEmail"] = msg.ToEmail,
            }
        }, context.CancellationToken);

        logger.LogInformation("Email verified confirmation dispatched for UserId={UserId}", msg.UserId);
    }
}
