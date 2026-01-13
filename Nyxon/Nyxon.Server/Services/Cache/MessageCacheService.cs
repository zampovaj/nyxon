using StackExchange.Redis;
using System.Security.Cryptography;
using Microsoft.Extensions.Caching.Distributed;
using Nyxon.Server.Models.Valkey;
using Nyxon.Server.Interfaces;
using Namotion.Reflection;

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
            // tran returns task - i dont need that task cause i will await them all at once using tran

            // save message and set expiration
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

        public async Task<Message?> GetMessageAsync(string kvKey)
        {
            var value = await _db.StringGetAsync(kvKey);

            if (value.IsNullOrEmpty) return null;

            return JsonSerializer.Deserialize<Message>(value.ToString());
        }

        public async Task<List<Message>> GetRecentMessagesAsync(Guid conversationId, int count = 50)
        {
            var results = new List<Message>();
            var key = KeyFactory.MessageRecentKey(conversationId);

            // get range 0 to count-1
            var values = await _db.ListRangeAsync(key, 0, count - 1);

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

        public async Task<List<Message>> GetMessagesBundleAsync(Guid conversationId, int lastSequenceNumber, int count = 50)
        {
            var results = new List<Message>();

            // build array of keys in advcacne to fetch all messages in one trip to redis
            int realCount = Math.Min(count, lastSequenceNumber);
            var keys = new RedisKey[realCount];

            for (int i = 0; i < realCount; i++)
            {
                keys[i] = KeyFactory.MessageKey(conversationId, lastSequenceNumber - i);
            }
            var values = await _db.StringGetAsync(keys);

            foreach (var val in values)
            {
                if (val.HasValue)
                {
                    var msg = JsonSerializer.Deserialize<Message>(val.ToString());
                    if (msg != null)
                        results.Add(msg);
                }
            }

            return results;
        }

        public async Task DeleteMessageAsync(string kvKey, Guid conversationId)
        {
            var listKey = KeyFactory.MessageRecentKey(conversationId);
            // using db -> send commands, one by one
            // using transaction -> queue them into one call, execute at once
            var tran = _db.CreateTransaction();

            _ = tran.KeyDeleteAsync(kvKey);
            _ = tran.KeyDeleteAsync(listKey);

            await tran.ExecuteAsync();
        }

        public async Task DeleteBatchAsync(List<string> kvKeys)
        {
            foreach (var kvKey in kvKeys)
            {
                await _db.KeyDeleteAsync(kvKey);
            }
        }
    }
}