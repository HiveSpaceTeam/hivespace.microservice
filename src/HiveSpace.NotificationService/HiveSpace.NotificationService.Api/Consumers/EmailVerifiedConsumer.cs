using HiveSpace.Infrastructure.Messaging.Shared.Events.Users;
using HiveSpace.NotificationService.Core.DomainModels;
using HiveSpace.NotificationService.Core.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;
using Resend;

namespace HiveSpace.NotificationService.Api.Consumers;

public class EmailVerifiedConsumer(
    IResend                          resend,
    ITemplateRenderer                renderer,
    ILogger<EmailVerifiedConsumer>   logger) : IConsumer<UserEmailVerifiedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<UserEmailVerifiedIntegrationEvent> context)
    {
        var msg = context.Message;
        var ct  = context.CancellationToken;

        var templateData = new Dictionary<string, object>
        {
            ["userName"] = msg.ToName,
        };

        var rendered = await renderer.RenderAsync(
            NotificationEventType.EmailVerified,
            NotificationChannel.Email,
            msg.Locale,
            templateData,
            ct);

        var message = new EmailMessage
        {
            From     = "HiveSpace <no-reply@hivespace.site>",
            To       = { msg.ToEmail },
            Subject  = rendered.Subject,
            HtmlBody = rendered.Body,
        };

        await resend.EmailSendAsync(message, ct);

        logger.LogInformation(
            "Email verified confirmation sent to {Email} for UserId={UserId}",
            msg.ToEmail, msg.UserId);
    }
}
