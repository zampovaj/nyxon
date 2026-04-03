using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace Nyxon.Server.Services.Cache
{
    public class InviteCodeCacheService : IInviteCodeCacheService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _db;

        public InviteCodeCacheService(IConnectionMultiplexer redis)
        {
            _redis = redis;
            _db = _redis.GetDatabase();
        }

        public async Task<int> GetInviteCodesCountAsync(Guid userId)
        {
            // returns 0 if key doesnt exist, no need for null checks
            var key = KeyFactory.InviteCodeLimit(userId);
            await CleanupExpiredInvites(key);
            var value = await _db.SortedSetLengthAsync(key);
            return (int)value;
        }

        public async Task SaveInviteAsync(Guid userId, byte[] hash)
        {
            await SaveInvitesAsync(userId, new List<byte[]> { hash });
        }

        public async Task<bool> SaveInvitesAsync(Guid userId, List<byte[]> hashes)
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var twentyFourHoursAgo = now - 24 * 60 * 60;

            string limitKey = KeyFactory.InviteCodeLimit(userId);

            // manually clean up old invites (older than 24 hours)
            await CleanupExpiredInvites(limitKey);

            // transaction - if code already exists, dicards the whole process
            // it might look stupid, but its the safest path
            // if i was to save them one by one when generating, in case of failure mid proccess id end up with ghost invites
            // saving them in collection through transaction also reduces db trips

            var tran = _db.CreateTransaction();

            foreach (var hash in hashes)
            {
                string globalKey = KeyFactory.InviteCode(hash);
                tran.AddCondition(Condition.KeyNotExists(globalKey));
                _ = tran.StringSetAsync(globalKey, userId.ToString(), TimeSpan.FromHours(24));
                _ = tran.SortedSetAddAsync(limitKey, hash, now);
            }

            _ = tran.KeyExpireAsync(limitKey, TimeSpan.FromHours(24));

            return await tran.ExecuteAsync();
        }

        public async Task<Guid?> ValidateInviteCodeAsync(byte[] hash)
        {
            string globalKey = KeyFactory.InviteCode(hash);
            var result = await _db.StringGetAsync(globalKey);
            return result.IsNull ? null : Guid.Parse((string)result);
        }

        public async Task DeleteInviteCodeAsync(Guid userId, byte[] hash)
        {
            // i cant actually delete it from per user set, doing so would defeat the point of the counter

            string globalKey = KeyFactory.InviteCode(hash);
            //string limitKey = KeyFactory.InviteCodeLimit(userId);

            var tran = _db.CreateTransaction();

            _ = tran.KeyDeleteAsync(globalKey);
            //_ = tran.SortedSetRemoveAsync(limitKey, hash);

            await tran.ExecuteAsync();
        }

        public async Task DeleteInvitesForUser(Guid userId)
        {
            var limitKey = KeyFactory.InviteCodeLimit(userId);

            await _db.KeyDeleteAsync(limitKey);
        }


        private async Task CleanupExpiredInvites(string userLimitKey)
        {
            var twentyFourHoursAgo = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - 86400;
            await _db.SortedSetRemoveRangeByScoreAsync(userLimitKey, 0, twentyFourHoursAgo);
        }
    }
}