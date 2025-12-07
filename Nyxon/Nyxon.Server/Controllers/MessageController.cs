using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Nyxon.Core.DTOs;
using Nyxon.Server.Hubs;
using Nyxon.Server.Interfaces;

namespace Nyxon.Server.Controllers
{
    [ApiController]
    [Route("v1/message/")]
    [Authorize]
    public class MessageController : ControllerBase
    {
        private readonly IMessageService _messageService;
        private readonly IHubContext<ChatHub> _hubContext;

        public MessageController(IMessageService messageService, IHubContext<ChatHub> hubContext)
        {
            _messageService = messageService;
            _hubContext = hubContext;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
        {
            var senderIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var senderUsername = User.FindFirst(ClaimTypes.Name)?.Value;
            if (senderIdString == null || !Guid.TryParse(senderIdString, out var senderId) || senderUsername == null)
                return Unauthorized();

            try
            {
                // save to postgres and valkey
                var messageId = await _messageService.SendMessageAsync(senderId, senderUsername, request);

                // signalr
                await _hubContext.Clients.Group(request.ConversationId.ToString())
                    .SendAsync("ReceiveMEssageNotification", new
                    {
                        ConversationId = request.ConversationId,
                        MessageId = messageId
                    });

                return Ok(new { MessageId = messageId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
        [HttpGet("{conversationId}/recent")]
        public async Task<IActionResult> GetRecentMessages([FromRoute] Guid conversationId)
        {
            try
            {
                var messages = await _messageService.GetRecentMessagesAsync(conversationId);
                return Ok(messages);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
        
        [HttpGet("{conversationId}/message/{sequenceNumber}")]
        public async Task<IActionResult> GetMessage([FromRoute] Guid conversationId, [FromRoute] int sequenceNumber)
        {
            try
            {
                var message = await _messageService.GetMessageAsync(conversationId, sequenceNumber);
                if (message == null)
                    throw new Exception("Message not found");
                
                return Ok(message);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}