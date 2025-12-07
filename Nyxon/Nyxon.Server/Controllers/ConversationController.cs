using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Nyxon.Core.DTOs;
using Nyxon.Server.Interfaces;

namespace Nyxon.Server.Controllers
{
    [ApiController]
    [Route("v1/conversation")]
    [Authorize]
    public class ConversationController : ControllerBase
    {
        private readonly IConversationService _conversationService;

        public ConversationController(IConversationService conversationService)
        {
            _conversationService = conversationService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateConversation([FromBody] CreateConversationRequest request)
        {
            var initiatorIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (initiatorIdString == null || !Guid.TryParse(initiatorIdString, out var initiatorId))
                return Unauthorized();

            try
            {
                var conversationId = await _conversationService.CreateConversationAsync(initiatorId, request.Username);
                return Ok(new CreateConversationResponse
                {
                    ConversationId = conversationId
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new {error = ex.Message});
            }     
        }
    }
}