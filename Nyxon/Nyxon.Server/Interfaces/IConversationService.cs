using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Server.Interfaces
{
    public interface IConversationService
    {
        Task<CreateConversationResponse> CreateConversationAsync(Guid initiatorId, CreateConversationRequest request);
        Task<List<ConversationSummaryDto>> GetInboxAsync(Guid userId);
        Task DeleteConversationAsync(Guid conversationId);
        Task UpdateReadConversationAsync(Guid userId, Guid conversationId);
    }
}