using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Server.Controllers
{
    [ApiController]
    [Route("api/account")]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly IInviteCodeService _inviteCodeService;
        private readonly IUserService _userService;

        public AccountController(IInviteCodeService inviteCodeService, IUserService userService)
        {
            _inviteCodeService = inviteCodeService;
            _userService = userService;
        }



        [HttpGet("data")]
        public async Task<ActionResult<AccountMetadataDto>> GetAccountData()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdString == null || !Guid.TryParse(userIdString, out var userId))
                return Unauthorized();
            try
            {
                int invitesCount = await _inviteCodeService.GetInviteCodesCountAsync(userId);
                DateTime joinedAt = await _userService.GetJoinDate(userId);

                return new AccountMetadataDto
                {
                    JoinedAt = joinedAt,
                    InvitesCount = invitesCount
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdString == null || !Guid.TryParse(userIdString, out var userId))
                return Unauthorized();
            try
            {
                // chnage password
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("delete")]
        public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountRequest request)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdString == null || !Guid.TryParse(userIdString, out var userId))
                return Unauthorized();
            try
            {
                // delete account
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}