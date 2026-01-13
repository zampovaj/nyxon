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
        Task<Message?> GetMessageAsync(string kvKey);
        Task DeleteMessageAsync(string kvKey, Guid conversationId);
        Task DeleteBatchAsync(List<string> kvKeys);

        // fetch last n messages for conversation
        Task<List<Message>> GetRecentMessagesAsync(Guid conversationId, int count = 50);
        Task<List<Message>> GetMessagesBundleAsync(Guid conversationId, int lastSequenceNumber, int count = 50);
    }
}