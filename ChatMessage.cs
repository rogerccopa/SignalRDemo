using System;

namespace SignalRDemo;

public class ChatMessage
{
    public int Id { get; set; } // primary key
    public string User { get; set; } = string.Empty;    // who sent the message
    public string Message { get; set; } = string.Empty;
    public string? Group { get; set; }  // Null = public, otherwise group name
    public string? ToUser { get; set; } // Null = public, otherwise private message to this user
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}
