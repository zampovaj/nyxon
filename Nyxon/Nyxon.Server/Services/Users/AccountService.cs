using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Server.Services.Users
{
    public class AccountService : IAccountService
    {

        private readonly AppDbContext _context;
        private readonly IPasswordService _passwordService;

        public AccountService(AppDbContext context, IPasswordService passwordService)
        {
            _context = context;
            _passwordService = passwordService;
        }

        public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
        {
            var user = await _context.Users
                .Where(u => u.Id == userId)
                .FirstOrDefaultAsync();

            if (user == null)
                throw new InvalidOperationException("User not found");

            if(!_passwordService.VerifyPassword(
                password: request.CurrentPasswordHash,
                salt: user.PasswordSalt,
                expectedHash: user.PasswordHash
            ))
            {
                throw new Exception("Current password does not match");
            }

            var newPasswordHash = _passwordService.HashPassword(request.NewPasswordHash, request.NewPasswordSalt);

            user.PasswordHash = newPasswordHash;
            user.PasswordSalt = request.NewPasswordSalt;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAccountAsync(Guid userId)
        {
            var user = await _context.Users
                .Where(u => u.Id == userId)
                .FirstOrDefaultAsync();

            if (user == null)
                throw new Exception("User not found");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // user vault
                var userVault = await _context.UserVaults
                    .Where(v => v.UserId == userId)
                    .FirstOrDefaultAsync();
                _context.UserVaults.Remove(userVault);
                await _context.SaveChangesAsync();

                // conversation vaults
                var convVaults = await _context.ConversationVaults
                    .Where(v => v.UserId == userId)
                    .ToListAsync();
                _context.ConversationVaults.RemoveRange(convVaults);
                await _context.SaveChangesAsync();

                // handshakes
                var handshakes = await _context.Handshakes
                    .Where(h => h.TargetUserId == userId)
                    .ToListAsync();
                _context.Handshakes.RemoveRange(handshakes);
                await _context.SaveChangesAsync();

                // opk
                var opks = await _context.OneTimePrekeys
                    .Where(o => o.UserId == userId)
                    .ToListAsync();
                _context.OneTimePrekeys.RemoveRange(opks);
                await _context.SaveChangesAsync();

                // spk
                var spk = await _context.SignedPrekeys
                    .Where(s => s.UserId == userId)
                    .FirstOrDefaultAsync();
                _context.SignedPrekeys.Remove(spk);
                await _context.SaveChangesAsync();

                // snapshots
                var snapshots = await _context.RatchetSnapshots
                    .Where(s => s.UserId == userId)
                    .ToListAsync();
                _context.RatchetSnapshots.RemoveRange(snapshots);
                await _context.SaveChangesAsync();

                // null out user
                user.Username = "Deleted user";
                user.PasswordHash = new byte[32];
                user.PasswordSalt = new byte[16];

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<DateTime> GetJoinDateAsync(Guid userId)
        {
            var user = await _context.Users
                .Where(u => u.Id == userId)
                .FirstOrDefaultAsync();

            if (user == null)
                throw new InvalidOperationException("User not found");

            return user.CreatedAt;
        }
    }
}