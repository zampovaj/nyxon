using Nyxon.Core.DTOs;
using Nyxon.Server.Interfaces;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

namespace Nyxon.Server.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IRegistrationService _registrationService;
        private readonly ILoginService _loginService;
        private readonly IAntiforgery _antiforgery;

        public AuthController(IRegistrationService registrationService, ILoginService loginService, IAntiforgery antiforgery)
        {
            _registrationService = registrationService;
            _loginService = loginService;
            _antiforgery = antiforgery;
        }

        [HttpGet("me")]
        [AllowAnonymous]
        public async Task<UserSessionDto> GetCurrentUser()
        {
            var user = HttpContext.User;

            if (user.Identity?.IsAuthenticated == true)
            {
                return new UserSessionDto()
                {
                    UserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "",
                    Username = user.Identity.Name ?? "unknown",
                    IsAuthenticated = true
                };
            }

            return new UserSessionDto() { IsAuthenticated = false };
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                var userId = await _registrationService.RegisterUserAsync(request);

                //login
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, request.Username)
        };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTime.UtcNow.AddDays(7)
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);
                
                //success
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
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var user = await _loginService.LoginAsync(request);

                if (user == null)
                    return Unauthorized(new { error = "Invalid credentials" });

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTime.UtcNow.AddDays(7)
                };

                // encrypt cookie and add it to the response header
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                // success
                return Ok(new UserSessionDto
                {
                    IsAuthenticated = true,
                    Username = user.Username,
                    UserId = user.Id.ToString()
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult?> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok(new { message = "Logged out" });
        }

        [HttpGet("csrf")]
        public async Task<IActionResult?> GetCsrfToken()
        {
            var tokens = _antiforgery.GetAndStoreTokens(HttpContext);
            return Ok(new { token = tokens.RequestToken });
        }
    }
}