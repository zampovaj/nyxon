using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Server.Interfaces
{
    public interface IConversationService
    {
        public Task<Guid> CreateConversationAsync(Guid initiatorId, string targetUsername);
        public Task<List<ConversationSummaryDto>> GetInboxAsync(Guid userId);
    }
}