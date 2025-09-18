using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SignalRDemo;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<ChatDbContext>(options => options.UseSqlite("Data Source=Chat.db"));

// Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ChatDbContext>()
    .AddDefaultTokenProviders();

// Cookie authentication
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/login.html"; // Redirect to this path if not authenticated/authorized
});

// SignalR
builder.Services.AddSignalR();

var app = builder.Build();

app.UseDefaultFiles();  // Serve index.html by default
app.UseStaticFiles();   // Serve static files in wwwroot (JS, HTML, CSS)

app.UseAuthentication(); // Enable authentication
app.UseAuthorization();  // Enable authorization

app.MapHub<ChatHub>("/TheChatHub");    // Map the SignalR hub to this path

app.MapPost(
    "/account/login",
    async ( string username,
            string password,
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager) =>
    {
        IdentityUser? user = await userManager.FindByNameAsync(username);
        if (user is not null)
        {
            var result = await signInManager.PasswordSignInAsync(user, password, true, false);
            if (result.Succeeded)
            {
                return Results.Redirect("/");
            }
        }
        return Results.Redirect("/login.html?error=1");
    });

app.MapPost(
    "/account/register",
    async ( string username,
            string password,
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager) =>
    {
        IdentityUser user = new IdentityUser { UserName = username };
        IdentityResult result = await userManager.CreateAsync(user, password);
        if (result.Succeeded)
        {
            await signInManager.SignInAsync(user, isPersistent: true);
            return Results.Redirect("/");
        }
        return Results.BadRequest("Registration failed");
    });

app.MapPost("/account/logout", async (SignInManager<IdentityUser> signInManager) =>
{
    await signInManager.SignOutAsync();
    return Results.Redirect("/login.html");
});

app.Run();
