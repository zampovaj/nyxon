using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Nyxon.Server.Controllers
{
    [ApiController]
    [Route("api/account")]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly IInviteCodeService _inviteCodeService;
        private readonly IAccountService _accountService;

        public AccountController(IInviteCodeService inviteCodeService, IAccountService accountService)
        {
            _inviteCodeService = inviteCodeService;
            _accountService = accountService;
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
                DateTime joinedAt = await _accountService.GetJoinDateAsync(userId);

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
                await _accountService.ChangePasswordAsync(userId, request);
                return NoContent();
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
                await _accountService.DeleteAccountAsync(userId, request.PasswordHash);
                // delete account
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return Unauthorized();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}