using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Interfaces
{
    public interface IConversationVaultService
    {
        public Task<ConversationVaultDto?> GetConversationVaultAsync(Guid userId, Guid conversationId);
        public Task UpdateConversationVaultAsync(Guid userId, ConversationVaultDto vaultDto);
    }
}