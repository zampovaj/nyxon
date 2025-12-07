using Nyxon.Server.Models.Valkey;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Server.Interfaces
{
    public interface IMessageCacheService
    {

        // save new message to recent cache
        Task SaveMessageAsync(Guid conversationId, Message message);

        // fetch specific message data needed for decryption
        Task<Message?> GetMessageAsync(Guid conversationId, int sequenceNumber);

        // fetch last n messages for conversation
        Task<List<Message>> GetRecentMessagesAsync(Guid conversationId, int count = 50);
    }
}