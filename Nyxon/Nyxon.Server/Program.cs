using Microsoft.AspNetCore.RateLimiting;
using Nyxon.Core.Extensions;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

// add controllers
builder.Services.AddControllersWithViews();

// di
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddCoreServices();

// swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument();

//logger
builder.Logging.AddConsole();

// rate limitting
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("global", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
    });
});

var app = builder.Build();

// migrations -> didnt work, now it should work even if postgres takes too long
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var dbContext = services.GetRequiredService<AppDbContext>();

    try
    {
        // Simple Retry Loop
        int retries = 0;
        while (retries < 5)
        {
            try
            {
                logger.LogInformation("Attempting to connect to database...");
                if (dbContext.Database.CanConnect())
                {
                    logger.LogInformation("Database connected. Applying migrations...");
                    dbContext.Database.Migrate();
                    logger.LogInformation("Migrations applied successfully!");
                    break;
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning($"Database not ready yet (Attempt {retries + 1}/5). Waiting...");
            }

            retries++;
            Thread.Sleep(2000);
        }

        if (retries >= 5)
        {
            throw new Exception("Could not connect to Database after 5 attempts.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "CRITICAL: Database migration failed. The app will likely crash.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUI();
}

// rate limitter
app.UseRateLimiter();

// blazor
app.UseBlazorFrameworkFiles();
app.UseDefaultFiles();
app.UseStaticFiles();

//routing
app.UseRouting();

//cors
if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
    app.UseCors();

// auth
app.UseAuthentication();
app.UseAuthorization();

//csrf
app.UseAntiforgery();

// controllers
app.MapControllers();

// signalr
app.MapHub<ChatHub>("/hubs/chat");

// blazor fallback
app.MapFallbackToFile("index.html");

app.Run();
