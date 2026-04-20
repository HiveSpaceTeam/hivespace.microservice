namespace HiveSpace.NotificationService.Core.Interfaces;

/// <summary>
/// Abstracts SignalR delivery so Core does not depend on the Hub class defined in the Api project.
/// Implemented in Api via IHubContext&lt;NotificationHub, INotificationClient&gt;.
/// </summary>
public interface INotificationHubContext
{
    Task SendToUserAsync(string userId, object payload, CancellationToken ct = default);
}
