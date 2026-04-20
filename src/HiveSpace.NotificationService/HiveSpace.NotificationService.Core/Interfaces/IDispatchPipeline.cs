using HiveSpace.NotificationService.Core.Models;

namespace HiveSpace.NotificationService.Core.Interfaces;

public interface IDispatchPipeline
{
    Task DispatchAsync(NotificationRequest request, CancellationToken ct = default);
}
