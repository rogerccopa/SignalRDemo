using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;

namespace SignalRDemo;

// A Hub is the central piece where clients and server send messages.
public class ChatHub : Hub
{
    // Thread-safe dictionary: username -> connectionId
    private static readonly ConcurrentDictionary<string, string> _users = new();
    private readonly ChatDbContext _dbContext;

    public ChatHub(ChatDbContext dbContext)
    {
        _dbContext = dbContext;
    }

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

    /*
    public Task RegisterUser(string userName)
    {
        _users[userName] = Context.ConnectionId;

        // Send last 10 messages as history
        var recentMessages = _dbContext.ChatMessages
            .OrderByDescending(m => m.SentAt)
            .Take(10)
            .OrderBy(m => m.SentAt)
            .ToList();

        return Clients.Caller.SendAsync("ReceiveHistory", recentMessages);
    }
    */
    
    public async Task SendMessage(string message)
    {
        // Get the username from the connection's identity; fallback to "Anonymous" if not authenticated
        var userName = Context.User?.Identity?.Name ?? "Anonymous";
        
        var chatMessage = new ChatMessage { User = userName, Message = message };
        _dbContext.ChatMessages.Add(chatMessage);
        await _dbContext.SaveChangesAsync();

        await Clients.All.SendAsync("ReceiveMessage", userName, message);
    }

    public async Task SendPrivateMessage(string fromUser, string toUser, string message)
    {
        if (_users.TryGetValue(toUser, out var connectionId))
        {
            var chatMessage = new ChatMessage { User = fromUser, Message = message, ToUser = toUser };
            _dbContext.ChatMessages.Add(chatMessage);
            await _dbContext.SaveChangesAsync();

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

    public async Task SendGroupMessage(string groupName, string message)
    {
        // Get the username from the connection's identity; fallback to "Anonymous" if not authenticated
        var userName = Context.User?.Identity?.Name ?? "Anonymous";

        var chatMessage = new ChatMessage { User = userName, Message = message, Group = groupName };
        _dbContext.ChatMessages.Add(chatMessage);
        await _dbContext.SaveChangesAsync();
        
        await Clients.Group(groupName).SendAsync("ReceiveGroupMessage", userName, groupName, message);
    }
}
