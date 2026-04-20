using Hangfire;
using HiveSpace.NotificationService.Core.Interfaces;

namespace HiveSpace.NotificationService.Core.BackgroundJobs;

public class HangfireRetryScheduler(IBackgroundJobClient jobClient) : IRetryScheduler
{
    public void Schedule(Guid notificationId, TimeSpan delay)
    {
        jobClient.Schedule<RetryNotificationJob>(
            j => j.ExecuteAsync(notificationId),
            delay);
    }
}
