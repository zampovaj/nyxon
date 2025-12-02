using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StackExchange.Redis;
using Shared.Models.Valkey;

namespace Backend.Services.Cache
{
    public interface IMessageCacheService
    {

        // Save message and update the "Recent" list
        Task SaveMessageAsync(Guid conversationId, Message message);
        
        // Fetch a specific message (for SignalR flow)
        Task<Message?> GetMessageAsync(Guid conversationId, int sequenceNumber);
        
        // Fetch last N messages (for First Load flow)
        Task<List<Message>> GetRecentMessagesAsync(Guid conversationId, int count = 50);
    }
}