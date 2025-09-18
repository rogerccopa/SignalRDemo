using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace SignalRDemo;

public class ChatDbContext : IdentityDbContext
{
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();

    public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options) {}
}
