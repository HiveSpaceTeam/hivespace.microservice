using HiveSpace.NotificationService.Core.Dispatch.Models;

namespace HiveSpace.NotificationService.Core.Interfaces;

public interface IDispatchPipeline
{
    Task DispatchAsync(NotificationRequest request, CancellationToken ct = default);
}
