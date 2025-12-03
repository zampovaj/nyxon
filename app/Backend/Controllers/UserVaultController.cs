using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Backend.Controllers
{
    [ApiController]
    [Route("v1/user/vault")]
    [Authorize]
    public class UserVaultController : ControllerBase
    {
        private readonly IUserVaultService _userVaultService;
        public UserVaultController(IUserVaultService userVaultService)
        {
            _userVaultService = userVaultService;
        }

        [HttpGet]
        public async Task<IActionResult> GetVault()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdString == null || !Guid.TryParse(userIdString, out var userId))
                return Unauthorized();

            var vault = await _userVaultService.GetVaultAsync(userId);
            if (vault == null)
                return NotFound("User vault not found.");
            
            return Ok(vault);
        }
    }
}