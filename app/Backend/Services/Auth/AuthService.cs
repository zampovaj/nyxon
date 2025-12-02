using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shared.DTOs;
using Backend.Data;
using Backend.Models;
using System.Transactions;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Shared.Interfaces;
using Backend.Services.Crypto;

namespace Backend.Services.Auth
{
    public class AuthService
    {
        private readonly AppDbContext _context;
        private readonly IHashInterface _hashService;
        private readonly bool _enforceInvites;

        public AuthService(AppDbContext context, IHashInterface hashService, IConfiguration config)
        {
            _context = context;
            _hashService = hashService;
            _enforceInvites = config.GetValue<bool>("Security:EnforceInvites", false);
        }
        public async Task<User> RegisterUserAsync(RegisterRequest request)
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
                    if (string.IsNullOrWhiteSpace(request.InviteCode))
                        throw new ArgumentException("Invite code is required");

                    request.InviteCode = _hashService.HashInviteCode(request.InviteCode);

                    var invite = await _context.InviteCodes
                        .FirstOrDefaultAsync(i => i.CodeHash == request.InviteCode && !i.Used);
                    if (invite == null)
                        throw new("Invalid or already used invide code");

                    invite.Use();
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

                return newUser;
            }

            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        private string ComputeHash(string input)
        {
            return input;
        }
    }
}