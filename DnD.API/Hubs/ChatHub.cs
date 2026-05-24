using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace DnD.API.Hubs;

public class ChatHub : Hub
{
    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }

    public async Task SendMessageToGroup(string groupName, object message)
    {
        await Clients.Group(groupName).SendAsync("ReceiveMessage", message);
    }
}
