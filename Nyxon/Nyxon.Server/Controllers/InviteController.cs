using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.HttpResults;
using YamlDotNet.Core.Tokens;

namespace Nyxon.Server.Controllers
{
    [ApiController]
    [Route("api/invites")]
    [AutoValidateAntiforgeryToken]
    [Authorize]
    public class InviteController : ControllerBase
    {
        private IInviteCodeService _inviteCodeService;

        public InviteController(IInviteCodeService inviteCodeService)
        {
            _inviteCodeService = inviteCodeService;
        }

        [HttpPost]
        [Authorize(Policy = "CanCreateInvites")]
        public async Task<ActionResult<InviteCodeDto>> CreateInvite()
        {
            try
            {
                var user = HttpContext.User;

                if (user.Identity?.IsAuthenticated != true)
                    return Unauthorized();

                var canCreate = bool.TryParse(user.FindFirst("CanCreateInvites")?.Value, out var val) && val;
                if (!canCreate)
                    return Forbid();

                var invite = await _inviteCodeService.CreateInviteAsync();
                return Ok(invite);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}