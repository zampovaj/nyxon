var builder = WebApplication.CreateBuilder(args);

// add controllers
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

// di
builder.Services.AddApplicationServices(builder.Configuration); 

// swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument();

var app = builder.Build();

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
