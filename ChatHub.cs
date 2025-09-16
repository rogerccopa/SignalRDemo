using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;

namespace SignalRDemo;

// A Hub is the central piece where clients and server send messages.
public class ChatHub : Hub
{
    // Thread-safe dictionary: username -> connectionId
    private static readonly ConcurrentDictionary<string, string> _users = new();

    public override Task OnConnectedAsync()
    {
        Console.WriteLine($"Client connected: {Context.ConnectionId}");

        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        // Remove user if they disconnect
        var user = _users.FirstOrDefault(x => x.Value == Context.ConnectionId).Key;
        if (user is not null)
        {
            _users.TryRemove(user, out _);
        }

        Console.WriteLine($"Client disconnected: {Context.ConnectionId}");

        return base.OnDisconnectedAsync(exception);
    }

    // Called by client to register a username
    public Task RegisterUser(string userName)
    {
        _users[userName] = Context.ConnectionId;

        return Task.CompletedTask;
    }

    // Send a message to all clients
    public async Task SendMessage(string user, string message)
    {
        // Broadcasts to all connected clients
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }

    // Send private message
    public async Task SendPrivateMessage(string fromUser, string toUser, string message)
    {
        if (_users.TryGetValue(toUser, out var connectionId))
        {
            await Clients.Client(connectionId).SendAsync("ReceivePrivateMessage", fromUser, message);
        }
    }

    // --- GROUPS ---
    public async Task JoinGroup(string groupName, string userName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        await Clients.Group(groupName).SendAsync(
            "ReceiveMessage",
            "System",
            $"{Context.ConnectionId}={userName} joined {groupName}");
    }

    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        await Clients.Group(groupName).SendAsync(
            "ReceiveMessage",
            "System",
            $"{Context.ConnectionId} left {groupName}");
    }

    public async Task SendGroupMessage(string user, string groupName, string message)
    {
        await Clients.Group(groupName).SendAsync("ReceiveGroupMessage", user, groupName, message);
    }
}
