using Microsoft.EntityFrameworkCore;
using SignalRDemo;

var builder = WebApplication.CreateBuilder(args);

// Add EF Core with SQLite
builder.Services.AddDbContext<ChatDbContext>(options => options.UseSqlite("Data Source=Chat.db"));

// Add SignalR
builder.Services.AddSignalR();

var app = builder.Build();

app.UseDefaultFiles();  // Serve index.html by default
app.UseStaticFiles();   // Serve static files in wwwroot (JS, HTML, CSS)

app.MapHub<ChatHub>("/TheChatHub");    // Map the SignalR hub to this path

app.Run();
