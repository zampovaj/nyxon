using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Interfaces
{
    public interface IMessageService
    {
        public Task<Guid> SendMessageAsync(Guid senderId, string senderUsername, SendMessageRequest request);
        public Task<List<MessageResponse>> GetRecentMessagesAsync(Guid conversationId);
        public Task<MessageResponse?> GetMessageAsync(Guid conversationId, int sequenceNumber);
    }
}