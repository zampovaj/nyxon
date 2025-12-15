var builder = WebApplication.CreateBuilder(args);

// add controllers
builder.Services.AddControllersWithViews();
// di
builder.Services.AddApplicationServices(builder.Configuration);

// swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument();

var app = builder.Build();

// ... after builder.Build()

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

//cors
app.UseCors();

//routing
app.UseRouting();

// auth
app.UseAuthentication();
app.UseAuthorization();
//csrf
app.UseAntiforgery();

// controllers
app.MapControllers();

// signalr
app.MapHub<ChatHub>("/hubs/chat");

app.Run();
