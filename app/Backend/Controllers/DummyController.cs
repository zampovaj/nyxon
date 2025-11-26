using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class DummyController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok("OK");
}
