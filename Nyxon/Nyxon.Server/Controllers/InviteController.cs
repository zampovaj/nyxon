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
        public async Task<ActionResult<NewInviteCodesResponse>> CreateInvite([FromBody] NewInviteCodesRequest request)
        {
            try
            {
                var user = HttpContext.User;

                if (user.Identity?.IsAuthenticated != true)
                    return Unauthorized();

                var canCreate = bool.TryParse(user.FindFirst("CanCreateInvites")?.Value, out var val) && val;
                if (!canCreate)
                    return Forbid();

                var invites = await _inviteCodeService.CreateInvitesAsync(request.Count);
                return Ok(new NewInviteCodesResponse(invites));
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}