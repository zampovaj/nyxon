using StackExchange.Redis;
using System.Security.Cryptography;
using Microsoft.Extensions.Caching.Distributed;
using Nyxon.Server.Models.Valkey;
using Nyxon.Server.Interfaces;

namespace Nyxon.Server.Services.Cache
{
    public class MessageCacheService : IMessageCacheService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _db;

        public MessageCacheService(IConnectionMultiplexer redis)
        {
            _redis = redis;
            _db = _redis.GetDatabase();
        }

        public async Task SaveMessageAsync(Guid conversationId, Message message)
        {
            var json = JsonSerializer.Serialize(message);
            var messageKey = KeyFactory.MessageKey(conversationId, message.SequenceNumber);
            var listKey = KeyFactory.MessageRecentKey(conversationId);

            // using db -> send commands, one by one
            // using transaction -> queue them into one call, execute at once
            var tran = _db.CreateTransaction();
            
            // _ discard variable
            // trand returns task - i dont need that task cause i will await them all at once using tran
            
            // set direct key -> expires after 30 days
            _ = tran.StringSetAsync(messageKey, json, TimeSpan.FromDays(30));

            // lpush to list -> lpush means newest on top
            _ = tran.ListLeftPushAsync(listKey, json);

            // trim to keep only latest 50 messages
            _ = tran.ListTrimAsync(listKey, 0, 49);

            // executes async all three operations
            await tran.ExecuteAsync();
        }

        
        public async Task<Message?> GetMessageAsync(Guid conversationId, int sequenceNumber)
        {
            var key = KeyFactory.MessageKey(conversationId, sequenceNumber);
            var value = await _db.StringGetAsync(key);

            if (value.IsNullOrEmpty) return null;

            return JsonSerializer.Deserialize<Message>(value.ToString());
        }

        public async Task<List<Message>> GetRecentMessagesAsync(Guid conversationId, int count = 50)
        {
            var key = KeyFactory.MessageRecentKey(conversationId);
            
            // get range 0 to count-1
            var values = await _db.ListRangeAsync(key, 0, count - 1);

            var results = new List<Message>();
            foreach (var val in values)
            {
                if (val.HasValue)
                {
                    // msg is json
                    var msg = JsonSerializer.Deserialize<Message>(val.ToString());
                    if (msg != null) results.Add(msg);
                }
            }

            return results;
        }
    }
}