using Resend;
using Microsoft.Extensions.Logging;
using HiveSpace.NotificationService.Core.DomainModels;
using HiveSpace.NotificationService.Core.Interfaces;
using HiveSpace.NotificationService.Core.Models;

namespace HiveSpace.NotificationService.Core.Channels.Email;

public class EmailChannelProvider(
    IResend              resend,
    IUserRefRepository   userRefs,
    ITemplateRenderer    renderer,
    ILogger<EmailChannelProvider> logger) : IChannelProvider
{
    public NotificationChannel Channel => NotificationChannel.Email;

    public async Task<DeliveryResult> SendAsync(
        Notification notification,
        Dictionary<string, object> templateData,
        CancellationToken ct = default)
    {
        var user = await userRefs.GetByIdAsync(notification.UserId, ct);
        if (user is null)
        {
            logger.LogWarning("UserRef not found for UserId={UserId}", notification.UserId);
            return DeliveryResult.Fail("UserRef not found — cannot resolve email address");
        }

        var rendered = await renderer.RenderAsync(
            notification.EventType, NotificationChannel.Email,
            user.Locale, templateData, ct);

        try
        {
            var message = new EmailMessage
            {
                From     = "HiveSpace <no-reply@hivespace.site>",
                To       = { user.Email },
                Subject  = rendered.Subject,
                HtmlBody = rendered.Body,
            };

            var response = await resend.EmailSendAsync(message, ct);
            return DeliveryResult.Ok(response.Content.ToString());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Resend failed for UserId={UserId}", notification.UserId);
            return DeliveryResult.Fail(ex.Message);
        }
    }
}
