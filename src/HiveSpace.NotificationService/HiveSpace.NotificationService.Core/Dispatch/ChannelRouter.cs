using Microsoft.Extensions.Logging;
using HiveSpace.NotificationService.Core.DomainModels;
using HiveSpace.NotificationService.Core.Interfaces;

namespace HiveSpace.NotificationService.Core.Dispatch;

public class ChannelRouter(
    IEnumerable<IChannelProvider> providers,
    IRateLimiter                  rateLimiter,
    IRetryScheduler               retryScheduler,
    ILogger<ChannelRouter>        logger) : IChannelRouter
{
    private const int MaxAttempts = 5;

    public async Task SendAsync(
        Notification notification,
        Dictionary<string, object> templateData,
        CancellationToken ct = default)
    {
        var provider = providers.SingleOrDefault(p => p.Channel == notification.Channel);

        if (provider is null)
        {
            logger.LogWarning("No provider registered for Channel={Channel}", notification.Channel);
            notification.MarkFailed("No provider registered");
            return;
        }

        if (!await rateLimiter.AllowAsync(notification.UserId, notification.Channel, ct))
        {
            logger.LogInformation("Rate limited. UserId={UserId} Channel={Channel}",
                notification.UserId, notification.Channel);
            notification.MarkThrottled();
            return;
        }

        try
        {
            var result = await provider.SendAsync(notification, templateData, ct);

            notification.IncrementAttempt();
            var attempt = DeliveryAttempt.Create(
                notification.Id,
                notification.AttemptCount,
                result.Success,
                result.ProviderResponse,
                result.ErrorMessage);

            notification.AddAttempt(attempt);

            if (result.Success)
                notification.MarkSent();
            else
                HandleFailure(notification, result.ErrorMessage ?? "Provider returned failure");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception sending NotificationId={Id} Channel={Channel}",
                notification.Id, notification.Channel);

            notification.IncrementAttempt();
            HandleFailure(notification, ex.Message);
        }
    }

    private void HandleFailure(Notification notification, string errorMessage)
    {
        if (notification.AttemptCount >= MaxAttempts)
        {
            logger.LogError("Notification dead after {Attempts} attempts. Id={Id}",
                notification.AttemptCount, notification.Id);
            notification.MarkDead(errorMessage);
            return;
        }

        notification.MarkFailed(errorMessage);

        var delay = TimeSpan.FromMinutes(Math.Pow(2, notification.AttemptCount));
        retryScheduler.Schedule(notification.Id, delay);
    }
}
