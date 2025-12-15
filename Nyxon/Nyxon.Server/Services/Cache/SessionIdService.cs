using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace Nyxon.Server.Services.Cache
{
    public class SessionIdService : ISessionIdService
    {
        private readonly IDistributedCache _cache;
        public SessionIdService(IDistributedCache cache)
        {
            _cache = cache;
        }
        public async Task<string> SaveSessionIdAsync(Guid userId)
        {
            var sessionId = Guid.NewGuid().ToString();
            var sessionIdKey = KeyFactory.SessionId(userId);

            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(7)
            };

            await _cache.SetStringAsync(sessionIdKey, sessionId, cacheOptions);
            return sessionId;
        }
        public async Task<string?> GetSessionIdAsync(string userId)
        {
            var key = KeyFactory.SessionId(userId);
            return await _cache.GetStringAsync(key);
        }
    }
}