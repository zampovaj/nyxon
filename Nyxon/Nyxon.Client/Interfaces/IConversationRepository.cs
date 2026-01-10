using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Client.Interfaces
{
    public interface IConversationRepository
    {
        Task<InboxDto?> FetchInboxAsync();
        Task<PrekeyBundleResponse?> GetPrekeyBundle(string username);
        Task<ConversationVaultDto?> FetchVaultAsync(Guid conversationId);
        Task<SendMessageResponse?> SendMessageAsync(SendMessageRequest messageRequest);
        Task<CreateConversationResponse?> CreateConversationAsync(CreateConversationRequest conversationRequest);
        Task<bool> CreateConversationVaultAsync(Guid conversationId, ConversationVaultData vaultData);
        Task<MessageResponse?> GetMessageAsync(string kvKey);
        Task<MessageReceivedStateUpdateResponse?> ReceiveMessageServerUpdateAsync(MessageReceivedStateUpdateRequest request);
    }
}