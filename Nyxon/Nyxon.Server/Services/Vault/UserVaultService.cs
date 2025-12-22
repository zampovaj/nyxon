using Nyxon.Core.DTOs;
using Nyxon.Server.Data;
using Nyxon.Server.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Server.Services.Vault
{
    public class UserVaultService : IUserVaultService
    {
        private readonly AppDbContext _context;

        public UserVaultService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<UserVaultRequest?> GetVaultAsync(Guid userId)
        {
            var vault = await _context.UserVaults
                .FirstOrDefaultAsync(v => v.UserId == userId);
            
            if (vault == null) return null;

            return new UserVaultRequest
            {
                EncryptedVaultKey = vault.VaultKey,
                EncryptedIdentityKey = vault.IdentityKey,
                Salt = vault.Salt
            };
        }
    }
}