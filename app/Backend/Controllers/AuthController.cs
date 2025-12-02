using Shared.Interfaces;

namespace Backend.Controllers
{
    [ApiController]
    [Route("v1/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                var userId = await _authService.RegisterUserAsync(request);
                return Ok(new { userId = userId });
            }
            catch (ArgumentException argEx)
            {
                return BadRequest(new { error = argEx.Message });
            }
            catch (InvalidOperationException invOpEx)
            {
                return Conflict(new { error = invOpEx.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            return BadRequest(new { error = "Error 500. An internal error occured." });
        }
    }
}