using Nyxon.Core.DTOs;
using Nyxon.Server.Interfaces;

namespace Nyxon.Server.Controllers
{
    [ApiController]
    [Route("v1/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IRegistrationService _registrationService;
        private readonly ILoginService _loginService;
        public AuthController(IRegistrationService registrationService, ILoginService loginService)
        {
            _registrationService = registrationService;
            _loginService = loginService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                var userId = await _registrationService.RegisterUserAsync(request);
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

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try {
            var response = await _loginService.LoginAsync(request);

            if (response == null)
                return Unauthorized(new { error = "Invalid credentials" });

            return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}