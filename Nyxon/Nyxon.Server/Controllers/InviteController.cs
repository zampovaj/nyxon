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

                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdString == null || !Guid.TryParse(userIdString, out var userId))
                    return Unauthorized();

                var canCreateString = User.FindFirst("CanCreateInvites")?.Value;
                if (canCreateString == null || !bool.TryParse(canCreateString, out var canCreate))
                    return Forbid();

                var invites = await _inviteCodeService.CreateInvitesAsync(userId, request.Count);
                return Ok(new NewInviteCodesResponse(invites));
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}