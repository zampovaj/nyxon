using Nyxon.Core.DTOs;
using Nyxon.Server.Data;
using Nyxon.Server.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Server.Services.Vault
{
    public class ConversationVaultService : IConversationVaultService
    {
        private readonly AppDbContext _context;
        public ConversationVaultService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ConversationVaultDto?> GetConversationVaultAsync(Guid userId, Guid conversationId)
        {
            var vault = await _context.ConversationVaults
                .FirstOrDefaultAsync(v => v.UserId == userId && v.ConversationId == conversationId);

            if (vault == null) return null;

            return new ConversationVaultDto
            {
                ConversationId = conversationId,
                UpdatedAt = vault.UpdatedAt,
                RecvCounter = vault.RecvCounter,
                SendCounter = vault.SendCounter,
                VaultBlob = vault.VaultBlob
            };


        }
        public async Task UpdateConversationVaultAsync(Guid userId, ConversationVaultDto vaultDto)
        {
            var vault = await _context.ConversationVaults
                .FirstOrDefaultAsync(v => v.UserId == userId && v.ConversationId == vaultDto.ConversationId);

            // create if not exist
            if (vault == null)
            {
                vault = new ConversationVault
                (
                    userId: userId,
                    conversationId: vaultDto.ConversationId,
                    vaultBlob: vaultDto.VaultBlob
                );
                _context.ConversationVaults.Add(vault);
                await _context.SaveChangesAsync();
            }

            // update
            else
            {
                vault.RecvCounter = vaultDto.RecvCounter;
                vault.SendCounter = vaultDto.SendCounter;
                vault.VaultBlob = vaultDto.VaultBlob;
                vault.UpdatedAt = DateTime.UtcNow;

                _context.ConversationVaults.Update(vault);
                await _context.SaveChangesAsync();
            }

        }
    }
}