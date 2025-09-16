using Microsoft.EntityFrameworkCore;

namespace SignalRDemo;

public class ChatDbContext : DbContext
{
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();

    public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options) {}
}
