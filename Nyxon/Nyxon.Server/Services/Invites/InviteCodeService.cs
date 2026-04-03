using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Linq.Expressions;
using Org.BouncyCastle.Bcpg;

namespace Nyxon.Server.Services.Invites
{
    public class InviteCodeService : IInviteCodeService
    {
        private readonly AppDbContext _context;
        private readonly IHasher _hasher;
        private readonly IInviteCodeCacheService _inviteCodeCache;
        private const int InvitesLimit = 20;


        public InviteCodeService(AppDbContext context, IHasher hasher, IInviteCodeCacheService inviteCodeCache)
        {
            _context = context;
            _hasher = hasher;
            _inviteCodeCache = inviteCodeCache;
        }

        public async Task MarkUsedAsync(Guid userId, byte[] hash)
        {
            await _inviteCodeCache.DeleteInviteCodeAsync(userId, hash);
        }

        public async Task<Guid?> ValidateAsync(byte[] hash)
        {
            return await _inviteCodeCache.ValidateInviteCodeAsync(hash);
        }

        public async Task<List<string>> CreateInvitesAsync(Guid userId, int count = 1)
        {
            List<string> inviteCodes = new();
            List<byte[]> hashes = new();

            int todayCount = await _inviteCodeCache.GetInviteCodesCountAsync(userId);
            int invitesLeftToday = InvitesLimit - todayCount;

            if (count > invitesLeftToday) count = invitesLeftToday;

            int attempts = 0;
            bool success = false;

            while (!success && attempts < 5)
            {
                inviteCodes.Clear();
                hashes.Clear();

                for (int j = 0; j < count; j++)
                {
                    var code = GenerateInvite(12);

                    if (!inviteCodes.Contains(code))
                    {
                        inviteCodes.Add(code);
                        hashes.Add(_hasher.HashInvite(code));
                    }
                }

                success = await _inviteCodeCache.SaveInvitesAsync(userId, hashes);
                attempts++;
            }

            return inviteCodes;
        }

        public async Task<int> GetInviteCodesCountAsync(Guid userId)
        {
            var user = await _context.Users
                .Where(u => u.Id == userId)
                .FirstOrDefaultAsync();
            
            if(user == null)
                throw new InvalidOperationException("This user id doesnt exist - cannot retrieve invites count");
            
            return await _inviteCodeCache.GetInviteCodesCountAsync(userId);
        }


        private string GenerateInvite(int length)
        {
            string charset = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return RandomNumberGenerator.GetString(charset, length);
        }
    }
}