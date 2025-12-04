using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Controllers
{
    [ApiController]
    [Route("v1/message/")]
    [Authorize]
    public class MessageController : ControllerBase
    {
        private readonly IMessageService _messageService;

        public MessageController(IMessageService messageService)
        {
            _messageService = messageService;
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
        {
            var senderIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var senderUsername = User.FindFirst(ClaimTypes.Name)?.Value;
            if (senderIdString == null || !Guid.TryParse(senderIdString, out var senderId) || senderUsername == null)
                return Unauthorized();

            try
            {
                var messageId = await _messageService.SendMessageAsync(senderId, senderUsername, request);
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