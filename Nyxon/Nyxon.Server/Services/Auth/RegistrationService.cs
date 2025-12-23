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
        private readonly IHasher _hasher;
        private readonly IPasswordService _passwordService;
        private readonly IInviteCodeService _inviteCodeService;
        private readonly bool _enforceInvites;

        public RegistrationService(AppDbContext context,
            IHasher hasher,
            IConfiguration config,
            IPasswordService passwordService,
            IInviteCodeService inviteCodeService)
        {
            _context = context;
            _hasher = hasher;
            _enforceInvites = config.GetValue<bool>("Security:EnforceInvites", false);
            _passwordService = passwordService;
            _inviteCodeService = inviteCodeService;
        }
        public async Task<Guid> RegisterUserAsync(RegisterRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // check username uniqueness
                var usernameExists = await _context.Users
                    // invariant is the correct choise for string comparisons
                    .AnyAsync(u => u.Username.ToLowerInvariant() == request.Username.ToLowerInvariant());

                if (usernameExists)
                    throw new InvalidOperationException("Username taken");

                // check invite code
                if (_enforceInvites)
                {
                    var inviteId = await _inviteCodeService.ValidateAsync(request.InviteCode);
                    await _inviteCodeService.MarkUsedAsync(inviteId);
                }

                var passwordHash = _passwordService.HashPassword(request.PasswordHash, request.PasswordSalt);

                // create user
                var newUser = new User(
                    username: request.Username,
                    passwordHash: passwordHash,
                    passwordSalt: request.PasswordSalt,
                    publicKey: request.PublicIdentityKey,
                    admin: false,
                    canCreateInvites: false);

                //spk
                var spk = new Nyxon.Server.Models.SignedPrekey
                (
                    id: request.PrekeyBundle.SPK.Id,
                    userId: newUser.Id,
                    publicKey: request.PrekeyBundle.SPK.PublicKey,
                    encryptedKey: request.PrekeyBundle.SPK.PrivateKey,
                    signature: request.PrekeyBundle.SPK.Signature
                );
                newUser.SignedPrekeys.Add(spk);

                //opk
                foreach (var opk in request.PrekeyBundle.OPKs)
                {
                    var oneTimePrekey = new Nyxon.Server.Models.OneTimePrekey
                    (
                        id: opk.Id,
                        userId: newUser.Id,
                        publicKey: opk.PublicKey,
                        encryptedKey: opk.PrivateKey
                    );
                    newUser.OneTimePrekeys.Add(oneTimePrekey);
                }

                //save user to db
                _context.Users.Add(newUser);
                await _context.SaveChangesAsync(); // wait for db to save changes

                // create uservault
                var newUserVault = new UserVault(
                    userId: newUser.Id,
                    passphraseSalt: request.PassphraseSalt,
                    vaultKey: request.EncryptedVaultKey,
                    privateIdentityKey: request.EncryptedPrivateIdentityKey);

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