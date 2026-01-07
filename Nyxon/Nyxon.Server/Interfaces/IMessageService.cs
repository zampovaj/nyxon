using Nyxon.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Server.Interfaces
{
    public interface IMessageService
    {
        Task<SendMessageResponse> SendMessageAsync(Guid senderId, SendMessageRequest request);
        Task<List<MessageResponse>> GetRecentMessagesAsync(Guid conversationId);
        Task<MessageResponse?> GetMessageAsync(Guid conversationId, int sequenceNumber);
        Task<MessageResponse?> GetMessageAsync(string kvKey);
        Task DeleteMessageAsync(Guid messageId);
        Task<ReadMessageStateUpdateResponse> ReadMessageUpdateAsync(Guid userId, ReadMessageStateUpdateRequest request);
    }
}