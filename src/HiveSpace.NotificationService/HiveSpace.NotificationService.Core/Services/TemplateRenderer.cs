using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Scriban;
using HiveSpace.NotificationService.Core.DomainModels;
using HiveSpace.NotificationService.Core.Interfaces;
using HiveSpace.NotificationService.Core.Models;

namespace HiveSpace.NotificationService.Core.Services;

public class TemplateRenderer(
    INotificationTemplateRepository templates,
    IMemoryCache                     cache,
    ILogger<TemplateRenderer>        logger) : ITemplateRenderer
{
    public async Task<RenderedTemplate> RenderAsync(
        string eventType, NotificationChannel channel,
        string locale, Dictionary<string, object> templateData,
        CancellationToken ct = default)
    {
        var template = await templates.GetAsync(eventType, channel, locale, ct)
                    ?? await templates.GetAsync(eventType, channel, "vi", ct);

        if (template is null)
        {
            logger.LogWarning(
                "Template missing. EventType={EventType} Channel={Channel} Locale={Locale}",
                eventType, channel, locale);

            return new RenderedTemplate(
                Subject: "Thông báo từ HiveSpace",
                Body:    "Bạn có một thông báo mới từ HiveSpace.");
        }

        var body    = RenderScriban(template.BodyTemplate, templateData, $"{eventType}:{channel}:body");
        var subject = RenderScriban(template.Subject,      templateData, $"{eventType}:{channel}:subject");

        return new RenderedTemplate(subject, body);
    }

    private string RenderScriban(string templateText, Dictionary<string, object> data, string cacheKey)
    {
        var parsed = cache.GetOrCreate(cacheKey, _ => Template.Parse(templateText));
        return parsed!.Render(data);
    }
}
