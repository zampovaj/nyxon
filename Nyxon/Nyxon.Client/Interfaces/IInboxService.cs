using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Client.Interfaces
{
    public interface IInboxService : IDisposable
    {
        List<Conversation> Conversations { get; }
        event Action OnChange;
        Task SyncInboxAsync();
        void Clear();
        Task<Guid?> GetConversationAsync(string username);
        Task UpdateConversationAsync(DateTime lastMessageAt, bool isRead, Guid conversationId);
        Task ReadConversationAsync(Guid conversationId);
    }
}