using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Server.Services.Messaging
{
    public class PrekeyService : IPrekeyService
    {
        private readonly AppDbContext _context;

        public PrekeyService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PrekeyBundleResponse> GetPrekeyBundle(string username)
        {
            var user = await _context.Users
                .Where(u => EF.Functions.ILike(u.Username, username))
                .FirstOrDefaultAsync();

            if (user == null)
                throw new ArgumentException("Target username not found");

            var spk = await _context.SignedPrekeys
                .Where(s => s.UserId == user.Id)
                .FirstOrDefaultAsync();

            if (spk == null)
                throw new InvalidOperationException("SignedPrekey cannot be null");

            // need to use transaction for opk to force opk to really be one time always
            Nyxon.Server.Models.OneTimePrekey? opk = null;
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // postgres row level locking ->
                // upon first contact locks the affected row
                // no other request can touch this row until it gets released
                opk = await _context.OneTimePrekeys
                    .FromSqlRaw(@"
                        SELECT * FROM ""OneTimePrekeys""
                        WHERE ""UserId"" = {0} AND ""Used"" = false
                        LIMIT 1
                        FOR UPDATE
                    ", user.Id)
                    .FirstOrDefaultAsync();

                if (opk != null)
                {
                    opk.Use();
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                return new PrekeyBundleResponse()
                {
                    UserId = user.Id,
                    SpkId = spk.Id,
                    SpkPublic = spk.PublicKey,
                    SpkSignature = spk.Signature,
                    OpkId = opk?.Id,
                    OpkPublic = opk?.PublicKey,
                    PublicIdentityKey = user.PublicIdentityKey,
                    PublicAgreementKey = user.PublicAgreementKey
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}