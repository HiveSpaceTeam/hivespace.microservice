using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.NotificationService.Core.DomainModels;
using HiveSpace.NotificationService.Core.Dispatch.Models;

namespace HiveSpace.NotificationService.Core.Interfaces;

public interface ITemplateRenderer
{
    Task<RenderedTemplate> RenderAsync(
        string eventType, NotificationChannel channel,
        Culture locale, Dictionary<string, object> templateData,
        CancellationToken ct = default);
}
