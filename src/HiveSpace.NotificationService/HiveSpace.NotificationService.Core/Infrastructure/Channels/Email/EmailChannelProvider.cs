using HiveSpace.Domain.Shared.Enumerations;
using Resend;
using Microsoft.Extensions.Logging;
using HiveSpace.NotificationService.Core.DomainModels;
using HiveSpace.NotificationService.Core.Interfaces;
using HiveSpace.NotificationService.Core.Dispatch.Models;

namespace HiveSpace.NotificationService.Core.Infrastructure.Channels.Email;

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

        string toEmail;
        Culture locale;
        if (user is not null)
        {
            toEmail = user.Email;
            locale  = user.Locale;
        }
        else if (templateData.TryGetValue("_recipientEmail", out var emailOverride))
        {
            toEmail = emailOverride.ToString()!;
            locale  = templateData.TryGetValue("_locale", out var localeOverride)
                      ? ParseLocaleOverride(localeOverride)
                      : Culture.Vi;
            logger.LogDebug("UserRef not found for UserId={UserId} — using override recipient email", notification.UserId);
        }
        else
        {
            logger.LogWarning("UserRef not found for UserId={UserId}", notification.UserId);
            return DeliveryResult.Fail("UserRef not found — cannot resolve email address");
        }

        var rendered = await renderer.RenderAsync(
            notification.EventType, NotificationChannel.Email,
            locale, templateData, ct);

        try
        {
            var message = new EmailMessage
            {
                From     = "HiveSpace <no-reply@hivespace.site>",
                To       = { toEmail },
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

    private static Culture ParseLocaleOverride(object localeOverride)
    {
        return localeOverride switch
        {
            Culture culture => culture,
            string code => CultureExtensions.FromCode(code),
            _ => CultureExtensions.FromCode(localeOverride.ToString()!)
        };
    }
}
