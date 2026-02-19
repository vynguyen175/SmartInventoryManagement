using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;
using SmartInventoryManagement.Data;
using SmartInventoryManagement.Models;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog for logging
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
builder.Host.UseSerilog();

// Get connection string - support Railway's DATABASE_URL or fallback to appsettings
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
string? connectionString;
if (!string.IsNullOrEmpty(databaseUrl))
{
    // Convert Railway's postgresql:// URL to Npgsql format
    // Example: postgresql://user:pass@host:5432/dbname
    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':');
    var host = uri.Host;
    var port = uri.Port > 0 ? uri.Port : 5432;
    var database = uri.AbsolutePath.TrimStart('/');
    // URL-decode username and password in case they contain special characters
    var username = Uri.UnescapeDataString(userInfo[0]);
    var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";

    // Internal Railway connections (.railway.internal) don't need SSL
    // External/public connections require SSL
    if (host.EndsWith(".railway.internal"))
    {
        connectionString = $"Host={host};Port={port};Database={database};Username={username};Password={password}";
    }
    else
    {
        connectionString = $"Host={host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true";
    }
}
else
{
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
}

// Configure database context with PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Add Identity services with custom ApplicationUser
builder.Services.AddIdentity<ApplicationUser, IdentityRole>() // Use ApplicationUser here
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Add controllers, views, and Razor Pages
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddTransient<IEmailSender, EmailSender>();


var app = builder.Build();

// Ensure database is migrated and roles/users exist on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Auto-migrate database in production
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        string[] roles = { "Admin", "User" };

        // Create roles if they don't exist
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Create a default admin user
        string adminEmail = "admin@inventory.com";
        string adminPassword = "Admin@123";

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "Admin User",
                ContactInformation = "N/A",
                SecurityQuestion = "What is the name of your best friend?",
                SecurityAnswer = "AdminFriend",
                Address = "N/A",
                Pronouns = ""
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        // Create a regular user
        string userEmail = "user@inventory.com";
        string userPassword = "User@123";

        var regularUser = await userManager.FindByEmailAsync(userEmail);
        if (regularUser == null)
        {
            regularUser = new ApplicationUser
            {
                UserName = userEmail,
                Email = userEmail,
                FullName = "Regular User",
                ContactInformation = "N/A",
                SecurityQuestion = "What is your favourite movie or book?",
                SecurityAnswer = "RegularAnswer",
                Address = "N/A",
                Pronouns = ""
            };

            var result = await userManager.CreateAsync(regularUser, userPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(regularUser, "User");
            }
        }
    }
    catch (Exception ex)
    {
        Log.Error("An error occurred while seeding roles and users: {Error}", ex);
    }
}


// Global exception handling
app.UseExceptionHandler("/Error/500"); // Redirects to custom 500 error page
app.UseStatusCodePagesWithReExecute("/Error/{0}"); // Handles 404 & other status codes

// Only use HTTPS redirection in development (Railway handles SSL at edge)
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();
app.Run();
