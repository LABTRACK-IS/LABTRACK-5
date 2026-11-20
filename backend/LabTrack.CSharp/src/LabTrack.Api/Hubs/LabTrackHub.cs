using LabTrack.Api.Models;
using Microsoft.AspNetCore.SignalR;

namespace LabTrack.Api.Hubs;

public interface ILabTrackClient
{
    Task ReceiveNotification(NotificationDto notification);
    Task WorkstationStatusChanged(string workstationCode, string newStatus);
    Task LabAlertReceived(string laboratoryId, string message);
}

public sealed class LabTrackHub : Hub<ILabTrackClient>
{
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinLabGroup(string laboratoryId)
    {
        if (!string.IsNullOrWhiteSpace(laboratoryId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"lab-{laboratoryId}");
        }
    }

    public async Task LeaveLabGroup(string laboratoryId)
    {
        if (!string.IsNullOrWhiteSpace(laboratoryId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"lab-{laboratoryId}");
        }
    }

    public async Task BroadcastNotification(NotificationDto notification)
    {
        await Clients.All.ReceiveNotification(notification);
    }
}
