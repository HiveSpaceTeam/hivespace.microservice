namespace HiveSpace.NotificationService.Core.Interfaces;

public interface IDeduplicationService
{
    /// <summary>Returns true if already processed. Atomically marks as seen otherwise.</summary>
    Task<bool> IsDuplicateAsync(string idempotencyKey, CancellationToken ct = default);
}
