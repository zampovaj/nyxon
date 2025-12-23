using Microsoft.EntityFrameworkCore;
using Nyxon.Core.DTOs;
using Nyxon.Core.Interfaces;
using Nyxon.Server.Data;
using Nyxon.Server.Interfaces;
using Microsoft.Extensions.Logging;

namespace Nyxon.Server.Services.Auth
{
    public class RegistrationService : IRegistrationService
    {
        private readonly AppDbContext _context;
        private readonly IHasher _hasher;
        private readonly IPasswordService _passwordService;
        private readonly IInviteCodeService _inviteCodeService;
        private readonly ILogger<RegistrationService> _logger;
        private readonly bool _enforceInvites;

        public RegistrationService(AppDbContext context,
            IHasher hasher,
            IConfiguration config,
            IPasswordService passwordService,
            IInviteCodeService inviteCodeService,
            ILogger<RegistrationService> logger)
        {
            _context = context;
            _hasher = hasher;
            _enforceInvites = config.GetValue<bool>("Security:EnforceInvites", false);
            _passwordService = passwordService;
            _inviteCodeService = inviteCodeService;
            _logger = logger;
        }
        public async Task<Guid> RegisterUserAsync(RegisterRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            _logger.LogInformation("Starting registration for user {Username}", request.Username);

            try
            {
                // check username uniqueness
                var usernameExists = await _context.Users
                    .AnyAsync(u => EF.Functions.ILike(u.Username, request.Username));


                if (usernameExists)
                {
                    _logger.LogWarning("Registration failed: Username {Username} is already taken", request.Username);
                    throw new InvalidOperationException("Username taken");
                }

                // check invite code
                if (_enforceInvites)
                {
                    var inviteId = await _inviteCodeService.ValidateAsync(request.InviteCode);
                    await _inviteCodeService.MarkUsedAsync(inviteId);
                    _logger.LogDebug("Invite code validated and marked used for {Username}", request.Username);
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

                _logger.LogDebug("User entity created for {Username}", request.Username);
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

                _logger.LogDebug("User {Username} saved to database. Creating vault...", request.Username);

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

                _logger.LogInformation("Registration completed successfully for user {Username} (ID: {UserId})", request.Username, newUser.Id);

                return newUser.Id;
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while registering user {Username}", request.Username);
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}