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

app.UseCors(); // <- enable CORS

// auth
app.UseAuthentication();
app.UseAuthorization();

// controllers
app.MapControllers();

// signalr
app.MapHub<ChatHub>("/hubs/chat");

app.Run();
