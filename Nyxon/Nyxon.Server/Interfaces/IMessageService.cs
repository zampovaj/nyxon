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
        Task<List<MessageResponse>> GetRecentMessagesAsync(Guid userId, Guid conversationId);
        Task<List<MessageResponse>> GetMessagesBundleAsync(Guid userId, Guid conversationId, int count, int lastSequenceNumber);
        Task<MessageResponse?> GetMessageAsync(Guid userId, Guid conversationId, int sequenceNumber);
        Task<MessageResponse?> GetMessageAsync(Guid userId, string kvKey);
        Task DeleteMessageAsync(Guid userId, Guid messageId);
        Task<MessageReceivedStateUpdateResponse> ReadMessageUpdateAsync(Guid userId, MessageReceivedStateUpdateRequest request);
    }
}