using Microsoft.AspNetCore.SignalR;
using HiveSpace.NotificationService.Core.Infrastructure.Channels.InApp;
using Microsoft.AspNetCore.Hosting;

namespace HiveSpace.NotificationService.Api.Hubs;

public class NotificationHub(IWebHostEnvironment hostEnvironment) : Hub<INotificationClient>
{
    public override async Task OnConnectedAsync()
    {
        var groupName = ResolveGroupName();
        if (groupName is not null)
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var groupName = ResolveGroupName();
        if (groupName is not null)
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// In production: uses the authenticated user's sub claim.
    /// In development: falls back to the ?userId= query string so the test page works without a JWT.
    /// </summary>
    private string? ResolveGroupName()
        => Context.UserIdentifier
        ?? (hostEnvironment.IsDevelopment()
            ? Context.GetHttpContext()?.Request.Query["userId"].FirstOrDefault()
            : null);
}
