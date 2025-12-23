using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Nyxon.Server.Services.Invites
{
    public class InviteCodeService : IInviteCodeService
    {
        private readonly AppDbContext _context;
        private readonly IHasher _hasher;

        public InviteCodeService(AppDbContext context, IHasher hasher)
        {
            _context = context;
            _hasher = hasher;
        }

        public async Task MarkUsedAsync(Guid id)
        {
            var invite = await _context.InviteCodes
                .Where(c => c.Id == id)
                .FirstOrDefaultAsync();

            if (invite == null)
                throw new("Invalid invite: id does not exist.");

            invite.Use();
            await _context.SaveChangesAsync();
        }

        public async Task<Guid> ValidateAsync(string code)
        {
            var hash = _hasher.HashInvite(code);

            var invite = await _context.InviteCodes
                .Where(c => c.CodeHash == hash && !c.Used)
                .FirstOrDefaultAsync();

            if (invite == null)
                throw new("Invalid or already used invide code");

            return invite.Id;
        }

        public async Task<InviteCodeDto> CreateInviteAsync()
        {
            var code = GenerateInvite(12);

            var invite = new InviteCode(_hasher.HashInvite(code));

            _context.InviteCodes.Add(invite);
            await _context.SaveChangesAsync();
            
            return new InviteCodeDto()
            {
                Id = invite.Id,
                Code = code
            };
        }

        private string GenerateInvite(int length)
        {
            string charset = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var bytes = new byte[length];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }

            var result = new char[length];

            for (int i = 0; i < length; i++)
            {
                //maps bytes to charset
                result[i] = charset[bytes[i] % charset.Length];
            }

            return new string(result);
        }
    }
}