using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Client.Interfaces
{
    public interface IActiveConversationService
    {
        Guid? ConversationId { get; }

        // used after creating new conversation
        Task InitializeNewAsync(Guid conversationId, ConversationVaultData encryptedVault);
        // used after opening already established conversation
        Task InitializeAsync(Guid conversationId);
        Task SendMessageAsync(string message);
        Task<List<ChatMessage>> LoadHistoryAsync(int count = 50, int skip = 0);
        event Action<ChatMessage> MessageDecrypted;
        void Clear();
    }
}