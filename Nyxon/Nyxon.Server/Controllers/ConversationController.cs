using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Nyxon.Core.DTOs;
using Nyxon.Server.Interfaces;
using YamlDotNet.Core.Tokens;

namespace Nyxon.Server.Controllers
{
    [ApiController]
    [Route("api/conversation")]
    [Authorize]
    public class ConversationController : ControllerBase
    {
        private readonly IConversationService _conversationService;
        private readonly IHandshakeService _handshakeService;

        public ConversationController(IConversationService conversationService, IHandshakeService handshakeService)
        {
            _conversationService = conversationService;
            _handshakeService = handshakeService;
        }

        [HttpPost]
        public async Task<ActionResult<CreateConversationResponse>> CreateConversation([FromBody] CreateConversationRequest request)
        {

            var initiatorIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (initiatorIdString == null || !Guid.TryParse(initiatorIdString, out var initiatorId))
                return Unauthorized();

            try
            {
                var conversation = await _conversationService.CreateConversationAsync(initiatorId, request);
                return Ok(conversation);
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return BadRequest(new { error = innerMessage, details = ex.ToString() });
            }
        }

        [HttpGet("inbox")]
        public async Task<ActionResult<List<InboxDto>>> GetInbox()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdString == null || !Guid.TryParse(userIdString, out var userId))
                return Unauthorized();

            try
            {
                var conversations = await _conversationService.GetInboxAsync(userId);
                var handshakes = await _handshakeService.GetPendingHandshakesAsync(userId);

                return Ok(new InboxDto(conversations, handshakes));
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
    }
}