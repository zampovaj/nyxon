using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Client.Interfaces.Messaging
{
    public interface IActiveConversationService : IDisposable
    {
        Guid? ConversationId { get; }

        // used after creating new conversation
        Task InitializeNewAsync(Guid conversationId, ConversationVaultData encryptedVault);
        // used after opening already established conversation
        Task InitializeAsync(Guid conversationId);
        Task<NewMessageObject> SendMessageAsync(string message);
        Task<ChatMessage> ReceiveMessageAsync(string kvKey);
        Task LoadHistoryAsync(int lastSequenceNumber, int count = 50);
        Task LoadRecentMessagesAsync();
        event Action<List<ChatMessage>> MessagesDecrypted;
        void Clear();
    }
}