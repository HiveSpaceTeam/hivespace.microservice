using HiveSpace.NotificationService.Core.DomainModels;
using HiveSpace.NotificationService.Core.Models;

namespace HiveSpace.NotificationService.Core.Interfaces;

public interface ITemplateRenderer
{
    Task<RenderedTemplate> RenderAsync(
        string eventType, NotificationChannel channel,
        string locale, Dictionary<string, object> templateData,
        CancellationToken ct = default);
}
