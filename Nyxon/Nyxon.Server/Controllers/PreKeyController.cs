using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;
using Org.BouncyCastle.Ocsp;
using YamlDotNet.Core.Tokens;

namespace Nyxon.Server.Controllers
{
    [ApiController]
    [Route("api/prekeys")]
    [Authorize]
    public class PreKeyController : ControllerBase
    {
        private readonly IPrekeyService _prekeyService;

        public PreKeyController(IPrekeyService prekeyService)
        {
            _prekeyService = prekeyService;
        }

        [HttpGet("{username}")]
        public async Task<ActionResult<PrekeyBundleResponse>> GetPrekeyBundle([FromRoute] string username)
        {
            try
            {
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdString == null || !Guid.TryParse(userIdString, out var userId))
                    return Unauthorized();

                var response = await _prekeyService.GetPrekeyBundle(username);

                if (response == null)
                    throw new("Prekey bundle generation failed");

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<ActionResult<bool>> CheckSignedPrekey()
        {
            try
            {
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdString == null || !Guid.TryParse(userIdString, out var userId))
                    return Unauthorized();

                return await _prekeyService.IsNewSpkNeededAsync(userId);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RotatePrekey(SignedPrekeyDto request)
        {
            try
            {
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdString == null || !Guid.TryParse(userIdString, out var userId))
                    return Unauthorized();

                await _prekeyService.RotateSignedPrekeyAsync(userId, request.SignedPrekey);
                return Created();
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}