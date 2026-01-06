using Nyxon.Core.DTOs;
using Nyxon.Server.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Server.Controllers
{
    [ApiController]
    [Route("api/conversation/vault")]
    [Authorize]
    public class ConversationVaultController : ControllerBase
    {
        private readonly IConversationVaultService _conversationVaultService;
        public ConversationVaultController(IConversationVaultService conversationVaultService)
        {
            _conversationVaultService = conversationVaultService;
        }

        [HttpGet("{conversationId}")]
        public async Task<ActionResult<ConversationVaultDto>> GetVault(Guid conversationId)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdString == null || !Guid.TryParse(userIdString, out var userId))
                return Unauthorized();

            var vault = await _conversationVaultService.GetConversationVaultAsync(userId, conversationId);
            if (vault == null)
                return NotFound(new { error = "Conversation vault not found" });

            return Ok(vault);
        }

        [HttpPut("{conversationId}")]
        public async Task<IActionResult> UpdateVault(Guid conversationId, [FromBody] ConversationVaultDto request)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdString == null || !Guid.TryParse(userIdString, out var userId))
                return Unauthorized();

            if (request == null)
                return BadRequest(new { error = "Conversation vault can't be null" });

            if (conversationId != request.ConversationId)
                return BadRequest(new { error = "Conversation id mismatch" });

            await _conversationVaultService.UpdateConversationVaultAsync(userId, request);
            return Ok();
        }

        [HttpPost("{conversationId}")]
        public async Task<IActionResult> CreateVault(Guid conversationId, [FromBody] ConversationVaultData vaultData)
        {
            try
            {
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdString == null || !Guid.TryParse(userIdString, out var userId))
                    return Unauthorized();

                if (vaultData == null)
                    return BadRequest(new { error = "Conversation vault can't be null" });

                await _conversationVaultService.CreateVaultAsync(conversationId, userId, vaultData);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}