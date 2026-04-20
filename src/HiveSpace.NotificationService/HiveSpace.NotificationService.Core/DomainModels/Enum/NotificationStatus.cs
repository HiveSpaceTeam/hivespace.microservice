namespace HiveSpace.NotificationService.Core.DomainModels;

public enum NotificationStatus
{
    Pending   = 0,
    Sent      = 1,
    Failed    = 2,
    Throttled = 3,
    Dead      = 4,
    Read      = 5,
}
