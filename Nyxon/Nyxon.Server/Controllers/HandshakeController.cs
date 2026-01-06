using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Server.Controllers
{
    [ApiController]
    [Route("api/handshake")]
    [Authorize]
    public class HandshakeController : ControllerBase
    {
        private readonly IHandshakeService _handshakeService;

        public HandshakeController(IHandshakeService handshakeService)
        {
            _handshakeService = handshakeService;
        }

        [HttpDelete("{handshakeId}")]
        public async Task<IActionResult> DeleteHandshake([FromRoute] Guid handshakeId)
        {
            try
            {
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdString == null || !Guid.TryParse(userIdString, out var userId))
                    return Unauthorized();

                await _handshakeService.DeleteHandshakeAsync(handshakeId);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}