using Microsoft.Extensions.Caching.Distributed;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConnectionController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IDistributedCache _cache;

    public ConnectionController(AppDbContext context, IDistributedCache cache)
    {
        _context = context;
        _cache = cache;
    }

    [HttpGet("test")]
    public async Task<IActionResult> TestConnection()
    {
        var results = new Dictionary<string, string>();

        // 1. Test PostgreSQL
        try
        {
            // Simple check to see if we can connect
            bool canConnect = await _context.Database.CanConnectAsync();
            results.Add("Postgres", canConnect ? "Success" : "Failed");
        }
        catch (Exception ex)
        {
            results.Add("Postgres", $"Error: {ex.Message}");
        }

        // 2. Test Valkey (Redis)
        try
        {
            await _cache.SetStringAsync("ping", "pong");
            var val = await _cache.GetStringAsync("ping");
            results.Add("Valkey", val == "pong" ? "Success" : "Failed (Value Mismatch)");
        }
        catch (Exception ex)
        {
            results.Add("Valkey", $"Error: {ex.Message}");
        }

        return Ok(results);
    }
}