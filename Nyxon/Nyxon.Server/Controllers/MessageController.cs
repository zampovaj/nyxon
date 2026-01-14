using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.SignalR;
using Nyxon.Core.DTOs;
using Nyxon.Server.Hubs;
using Nyxon.Server.Interfaces;
using YamlDotNet.Core.Tokens;

namespace Nyxon.Server.Controllers
{
    [ApiController]
    [Route("api/message")]
    [Authorize]
    public class MessageController : ControllerBase
    {
        private readonly IMessageService _messageService;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly ISnapshotService _snapshotService;

        public MessageController(IMessageService messageService,
            IHubContext<ChatHub> hubContext,
            ISnapshotService snapshotService)
        {
            _messageService = messageService;
            _hubContext = hubContext;
            _snapshotService = snapshotService;
        }

        [HttpGet("{kvKey}")]
        public async Task<ActionResult<MessageResponse>> GetMessageAsync(string kvKey)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdString == null || !Guid.TryParse(userIdString, out var userId))
                return Unauthorized();

            try
            {
                var message = await _messageService.GetMessageAsync(userId, kvKey);

                return Ok(message);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("send")]
        public async Task<ActionResult<Guid?>> SendMessage([FromBody] SendMessageRequest request)
        {
            var senderIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (senderIdString == null || !Guid.TryParse(senderIdString, out var senderId))
                return Unauthorized();

            try
            {
                // save to postgres and valkey
                var messageId = await _messageService.SendMessageAsync(senderId, request);

                return Ok(messageId);
            }

            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPatch("receive")]
        public async Task<ActionResult<MessageReceivedStateUpdateResponse>> Receive([FromBody] MessageReceivedStateUpdateRequest request)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdString == null || !Guid.TryParse(userIdString, out var userId))
                return Unauthorized();

            try
            {
                // update state
                var response = await _messageService.ReadMessageUpdateAsync(userId, request);

                return Ok(response);
            }

            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("{conversationId}/recent")]
        public async Task<ActionResult<MessagesBundleDto>> GetRecentMessages([FromRoute] Guid conversationId)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdString == null || !Guid.TryParse(userIdString, out var userId))
                return Unauthorized();

            try
            {
                var messages = await _messageService.GetRecentMessagesAsync(userId, conversationId);
                var snapshots = await _snapshotService.GetSnapshotsAsync(userId, conversationId, messages);
                return Ok(new MessagesBundleDto(messages, snapshots));
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("{conversationId}/bundle/{count}/{lastSequenceNumber}")]
        public async Task<ActionResult<MessagesBundleDto>> GetMessagesBundle([FromRoute] Guid conversationId, int count, int lastSequenceNumber)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdString == null || !Guid.TryParse(userIdString, out var userId))
                return Unauthorized();

            try
            {
                var messages = await _messageService.GetMessagesBundleAsync(userId, conversationId, count, lastSequenceNumber);
                var snapshots = await _snapshotService.GetSnapshotsAsync(userId, conversationId, messages);

                return new MessagesBundleDto(messages, snapshots);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("{conversationId}/message/{sequenceNumber}")]
        public async Task<ActionResult<MessageResponse>> GetMessage([FromRoute] Guid conversationId, [FromRoute] int sequenceNumber)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdString == null || !Guid.TryParse(userIdString, out var userId))
                return Unauthorized();

            try
            {
                var message = await _messageService.GetMessageAsync(userId, conversationId, sequenceNumber);
                if (message == null)
                    throw new Exception("Message not found");

                return Ok(message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}