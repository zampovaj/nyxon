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

        public ConversationController(IConversationService conversationService)
        {
            _conversationService = conversationService;
        }

        [HttpPost]
        public async Task<ActionResult<CreateConversationResponse>> CreateConversation([FromBody] CreateConversationRequest request)
        {
            var initiatorIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (initiatorIdString == null || !Guid.TryParse(initiatorIdString, out var initiatorId))
                return Unauthorized();

            try
            {
                var conversation = await _conversationService.CreateConversationAsync(initiatorId, request.Username);
                return Ok(conversation);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("inbox")]
        public async Task<ActionResult<List<ConversationSummaryDto>>> GetInbox()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdString == null || !Guid.TryParse(userIdString, out var userId))
                return Unauthorized();

            try
            {
                var inbox = await _conversationService.GetInboxAsync(userId);
                return Ok(inbox);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
    }
}