var builder = WebApplication.CreateBuilder(args);

// add controllers
builder.Services.AddControllers();
// di
builder.Services.AddApplicationServices(builder.Configuration);

// swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try 
    {
        dbContext.Database.Migrate();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Migration Failed: {ex.Message}");
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
