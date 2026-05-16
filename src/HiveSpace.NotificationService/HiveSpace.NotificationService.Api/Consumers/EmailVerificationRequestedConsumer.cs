using HiveSpace.Infrastructure.Messaging.Shared.Events.Users;
using HiveSpace.NotificationService.Core.DomainModels;
using HiveSpace.NotificationService.Core.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;
using Resend;

namespace HiveSpace.NotificationService.Api.Consumers;

public class EmailVerificationRequestedConsumer(
    IResend                                       resend,
    ITemplateRenderer                             renderer,
    ILogger<EmailVerificationRequestedConsumer>   logger) : IConsumer<UserEmailVerificationRequestedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<UserEmailVerificationRequestedIntegrationEvent> context)
    {
        var msg = context.Message;
        var ct  = context.CancellationToken;

        var templateData = new Dictionary<string, object>
        {
            ["userName"]         = msg.ToName,
            ["verificationLink"] = msg.VerificationLink,
            ["expiresAt"]        = msg.ExpiresAt.ToString("f"),
        };

        var rendered = await renderer.RenderAsync(
            NotificationEventType.EmailVerificationRequested,
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
            "Email verification sent to {Email} for UserId={UserId}",
            msg.ToEmail, msg.UserId);
    }
}
