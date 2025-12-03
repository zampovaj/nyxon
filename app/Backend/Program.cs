using Backend.Data;
using System.Net.WebSockets;
using Microsoft.EntityFrameworkCore;
// Backend/Program.cs
using Backend.Extensions; // Ensure you have this using

var builder = WebApplication.CreateBuilder(args);

// DELETE ALL manual Env Variable lines for Postgres/Valkey here.
// The DependencyInjection class handles it now.

// Add Services
builder.Services.AddControllers();

// cors
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

// This ONE LINE does everything now:
builder.Services.AddApplicationServices(builder.Configuration); 

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(); // NSwag

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUI();
}

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
