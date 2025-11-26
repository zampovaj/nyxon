using System.Net.WebSockets;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

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

var app = builder.Build();

app.UseWebSockets();

app.UseHttpsRedirection();

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
