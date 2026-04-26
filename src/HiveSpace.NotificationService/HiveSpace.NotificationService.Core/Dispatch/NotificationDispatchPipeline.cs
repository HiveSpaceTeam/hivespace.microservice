using System.Text.Json;
using Microsoft.Extensions.Logging;
using HiveSpace.NotificationService.Core.DomainModels;
using HiveSpace.NotificationService.Core.Interfaces;
using HiveSpace.NotificationService.Core.Dispatch.Models;

namespace HiveSpace.NotificationService.Core.Dispatch;

public class NotificationDispatchPipeline(
    IDeduplicationService     dedup,
    IUserPreferenceRepository prefs,
    INotificationRepository   repo,
    IChannelRouter            router,
    ILogger<NotificationDispatchPipeline> logger) : IDispatchPipeline
{
    public async Task DispatchAsync(NotificationRequest request, CancellationToken ct = default)
    {
        if (await dedup.IsDuplicateAsync(request.IdempotencyKey, ct))
        {
            logger.LogDebug("Duplicate skipped. Key={Key}", request.IdempotencyKey);
            return;
        }

        var eventGroup      = NotificationEventGroup.FromEventType(request.EventType);
        var enabledChannels = await prefs.GetEnabledChannelsAsync(request.UserId, eventGroup, ct);
        if (enabledChannels.Count == 0)
        {
            logger.LogDebug("No enabled channels. UserId={UserId} EventType={EventType}",
                request.UserId, request.EventType);
            return;
        }

        var payload = JsonSerializer.Serialize(request.TemplateData);
        var notifications = enabledChannels
            .Select(channel => Notification.Create(
                request.UserId,
                channel,
                request.EventType,
                $"{request.IdempotencyKey}:{channel}",
                payload))
            .ToList();

        repo.AddAll(notifications);

        logger.LogInformation(
            "Dispatching {Count} notifications. UserId={UserId} EventType={EventType}",
            notifications.Count, request.UserId, request.EventType);

        foreach (var n in notifications)
            await router.SendAsync(n, request.TemplateData, ct);

        await repo.SaveChangesAsync(ct);
    }
}
