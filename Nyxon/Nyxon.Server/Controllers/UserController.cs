using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YamlDotNet.Core.Tokens;

namespace Nyxon.Server.Controllers
{

    [ApiController]
    [Route("api/user")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("list")]
        public async Task<ActionResult<List<UserListDto>>> GetUserListAsync()
        {
            try
            {
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdString == null || !Guid.TryParse(userIdString, out var userId))
                    return Unauthorized();

                var userList = await _userService.GetAllUsersButMeAsync(userId);
                return Ok(userList);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
    }
}