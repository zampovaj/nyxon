using System.Net.WebSockets;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

var pgHost = Environment.GetEnvironmentVariable("POSTGRES_HOST");
var pgPort = Environment.GetEnvironmentVariable("POSTGRES_PORT");
var pgDb   = Environment.GetEnvironmentVariable("POSTGRES_DB");
var pgUser = Environment.GetEnvironmentVariable("POSTGRES_USER");
var pgPass = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");

var conn = $"Host={pgHost};Port={pgPort};Database={pgDb};Username={pgUser};Password={pgPass}";
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(conn));

// --- 2. Valkey (Redis) Setup (MISSING IN YOUR CODE) ---
var valkeyHost = Environment.GetEnvironmentVariable("VALKEY_HOST") ?? "valkey";
var valkeyPort = Environment.GetEnvironmentVariable("VALKEY_PORT") ?? "6379";

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = $"{valkeyHost}:{valkeyPort}";
});

builder.Services.AddControllers();

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5286") // frontend URL
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.WebHost.UseUrls("http://0.0.0.0:5000"); //container (inside docker) on port 5000; host (outside) port is mapped in docker-compose.yml

var app = builder.Build();

app.UseWebSockets();


//app.UseHttpsRedirection();

app.UseCors(); // <- enable CORS

app.UseAuthorization();

app.MapControllers();

app.Map("/ws", async context => 
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var ws = await context.WebSockets.AcceptWebSocketAsync();
        var buffer = new byte[1024];
        WebSocketReceiveResult? result;

        // Keep receiving messages until socket is closed
        do
        {
            result = await ws.ReceiveAsync(buffer, CancellationToken.None);
            if (result.MessageType != WebSocketMessageType.Close)
            {
                await ws.SendAsync(buffer[..result.Count], WebSocketMessageType.Text, true, CancellationToken.None);
            }
        } while (!result.CloseStatus.HasValue);

        await ws.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
    }
});

app.Run();
