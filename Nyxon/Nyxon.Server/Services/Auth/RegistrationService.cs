using Microsoft.EntityFrameworkCore;
using Nyxon.Core.DTOs;
using Nyxon.Core.Interfaces;
using Nyxon.Server.Data;
using Nyxon.Server.Interfaces;

namespace Nyxon.Server.Services.Auth
{
    public class RegistrationService : IRegistrationService
    {
        private readonly AppDbContext _context;
        private readonly IHashService _hashService;
        private readonly IInviteCodeService _inviteCodeService;
        private readonly bool _enforceInvites;

        public RegistrationService(AppDbContext context, IHashService hashService, IConfiguration config, IInviteCodeService inviteCodeService)
        {
            _context = context;
            _hashService = hashService;
            _enforceInvites = config.GetValue<bool>("Security:EnforceInvites", false);
            _inviteCodeService = inviteCodeService;
        }
        public async Task<Guid> RegisterUserAsync(RegisterRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // check username uniqueness
                var usernameExists = await _context.Users
                    .AnyAsync(u => u.Username == request.Username);

                if (usernameExists)
                    throw new InvalidOperationException("Username taken");

                // check invite code
                if (_enforceInvites)
                {
                    var inviteId = await _inviteCodeService.ValidateAsync(request.InviteCode);
                    await _inviteCodeService.MarkUsedAsync(inviteId);
                }

                // create user
                var newUser = new User(
                    request.Username,
                    request.PasswordHash,
                    request.PublicKey,
                    false,
                    false);

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync(); // wait for db to save changes

                // create uservault
                var newUserVault = new UserVault(
                    newUser.Id,
                    request.EncryptedVaultKey,
                    request.EncryptedIdentityKey);

                _context.UserVaults.Add(newUserVault);
                await _context.SaveChangesAsync();

                newUser.UserVault = newUserVault;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return newUser.Id;
            }

            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}