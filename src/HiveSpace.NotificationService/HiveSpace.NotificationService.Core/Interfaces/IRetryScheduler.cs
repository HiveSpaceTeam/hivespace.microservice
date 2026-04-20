namespace HiveSpace.NotificationService.Core.Interfaces;

public interface IRetryScheduler
{
    void Schedule(Guid notificationId, TimeSpan delay);
}
