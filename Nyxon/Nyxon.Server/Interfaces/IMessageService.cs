using Nyxon.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Server.Interfaces
{
    public interface IMessageService
    {
        public Task<SendMessageResponse> SendMessageAsync(Guid senderId, SendMessageRequest request);
        public Task<List<MessageResponse>> GetRecentMessagesAsync(Guid conversationId);
        public Task<MessageResponse?> GetMessageAsync(Guid conversationId, int sequenceNumber);
        public Task DeleteMessageAsync(Guid messageId);
    }
}