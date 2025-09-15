using System;
using Microsoft.AspNetCore.SignalR;

namespace SignalRDemo;

// A Hub is the central piece where clients and server send messages.
public class ChatHub : Hub
{
    // Method clients can call
    public async Task SendMessage(string user, string message)
    {
        // Broadcasts to all connected clients
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }
}
