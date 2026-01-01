using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Client.Services.Messaging
{
    public class ActiveConversationService : IActiveConversationService
    {
        private readonly IConversationRepository _conversationRepository;

        public Guid? ConversationId { get; private set; }
        public ConversationVaultData? EncryptedVault { get; private set; }
        public int SendingCounter { get; private set; }
        public int ReceivingCounter { get; private set; }
        public DateTime? UpdatedAt { get; private set; }

        public event Action<ChatMessage>? MessageDecrypted;

        public ActiveConversationService(IConversationRepository conversationRepository)
        {
            _conversationRepository = conversationRepository;
        }

        public async Task InitializeNewAsync(Guid conversationId, ConversationVaultData encryptedVault)
        {
            if (encryptedVault == null)
                throw new InvalidOperationException("Conversation vault cant be null");

            ConversationId = conversationId;
            EncryptedVault = encryptedVault;
            SendingCounter = 0;
            ReceivingCounter = 0;
        }

        public async Task InitializeAsync(Guid conversationId)
        {
            var conversationVault = await _conversationRepository.FetchVaultAsync(conversationId);
            if (conversationVault == null)
                throw new InvalidOperationException("Failed to fetch conversatoin vault");

            EncryptedVault = conversationVault.VaultData;
            ConversationId = conversationId;
            SendingCounter = conversationVault.SendCounter;
            ReceivingCounter = conversationVault.RecvCounter;
        }

        public Task<List<ChatMessage>> LoadHistoryAsync(int count = 50, int skip = 0)
        {
            throw new NotImplementedException();
        }

        public Task SendMessageAsync(string message)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }
    }
}