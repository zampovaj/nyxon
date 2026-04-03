using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Server.Services.Messaging
{
    public class PrekeyService : IPrekeyService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<PrekeyService> _logger;

        public PrekeyService(AppDbContext context, ILogger<PrekeyService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PrekeyBundleResponse> GetPrekeyBundle(string username)
        {
            var user = await _context.Users
                .Where(u => EF.Functions.ILike(u.Username, username))
                .FirstOrDefaultAsync();

            if (user == null)
                throw new ArgumentException("Target username not found");

                
            if (user.Username == AccountConstants.DeletedAccount)
                throw new InvalidOperationException("Cannot communicate with a deleted user");

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
        public async Task<bool> IsNewSpkNeededAsync(Guid userId)
        {
            try
            {
                var spk = await _context.SignedPrekeys
                    .Where(s => s.UserId == userId)
                    .FirstOrDefaultAsync();

                if (spk == null) throw new InvalidOperationException("SignedPrekey for this user doesn't exist");

                return DateTime.UtcNow >= spk.CreatedAt.AddDays(2);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to fetch Spk information for user Id: {userId}, error: {ex.Message}");
                return false;
            }
        }

        public async Task RotateSignedPrekeyAsync(Guid userId, Nyxon.Core.Models.SignedPrekey signedPrekey)
        {
            var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var user = await _context.Users
                    .Where(u => u.Id == userId)
                    .FirstOrDefaultAsync();
                if (user == null) throw new InvalidOperationException("User doesn't exist");

                //check handshake
                var countHandhskakes = await _context.Handshakes
                    .Where(h => h.SpkId == signedPrekey.Id)
                    .CountAsync();
                if (countHandhskakes > 0) return;

                // delete old
                var spk = await _context.SignedPrekeys
                    .Where(s => s.UserId == userId)
                    .FirstOrDefaultAsync();

                if (spk != null)
                    _context.SignedPrekeys.Remove(spk);
                
                // add new spk
                var newSpk = new Nyxon.Server.Models.SignedPrekey
                (
                    id: signedPrekey.Id,
                    userId: userId,
                    publicKey: signedPrekey.PublicKey,
                    encryptedKey: signedPrekey.PrivateKey,
                    signature: signedPrekey.Signature
                );
                _context.SignedPrekeys.Add(newSpk);

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}