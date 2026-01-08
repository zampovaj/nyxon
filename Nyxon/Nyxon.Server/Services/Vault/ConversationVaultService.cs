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
        private readonly ILogger<ConversationVaultService> _logger;
        public ConversationVaultService(AppDbContext context, ILogger<ConversationVaultService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ConversationVaultDto?> GetConversationVaultAsync(Guid userId, Guid conversationId)
        {
            var vault = await _context.ConversationVaults
                .FirstOrDefaultAsync(v => v.UserId == userId && v.ConversationId == conversationId);

            if (vault == null) return null;

            _logger.LogInformation($"sending msgindex: {vault.VaultData.Sending.Session.MessageIndex}");
            _logger.LogInformation($"sending rotationindex: {vault.VaultData.Sending.Session.RotationIndex}");
            _logger.LogInformation($"sending counter: {vault.SendCounter}");

            return new ConversationVaultDto
            {
                ConversationId = conversationId,
                UpdatedAt = vault.UpdatedAt,
                RecvCounter = vault.RecvCounter,
                SendCounter = vault.SendCounter,
                VaultData = vault.VaultData
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
                    vaultData: vaultDto.VaultData
                );
                _context.ConversationVaults.Add(vault);
                await _context.SaveChangesAsync();
            }

            // update
            else
            {
                vault.RecvCounter = vaultDto.RecvCounter;
                vault.SendCounter = vaultDto.SendCounter;
                vault.VaultData = vaultDto.VaultData;
                vault.UpdatedAt = DateTime.UtcNow;

                _context.ConversationVaults.Update(vault);

                // jsonb thing, need to mark it as modified
                _context.Entry(vault).Property(v => v.VaultData).IsModified = true;

                await _context.SaveChangesAsync();
            }

        }
        public async Task CreateVaultAsync(Guid conversationId, Guid userId, ConversationVaultData vaultData)
        {
            var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // conversation vault
                var vault = new ConversationVault
                (
                    userId: userId,
                    conversationId: conversationId,
                    vaultData: vaultData
                );
                _context.ConversationVaults.Add(vault);
                await _context.SaveChangesAsync();

                // snapshots

                var sendingSnapshot = vaultData.Sending.Snapshots.FirstOrDefault();
                var sending = new RatchetSnapshot(
                    id: sendingSnapshot.Id,
                    userId: userId,
                    conversationId: conversationId,
                    type: RatchetType.Sending,
                    rotationIndex: sendingSnapshot.RotationIndex,
                    encryptedSessionKey: sendingSnapshot.EncryptedSessionKey,
                    createdAt: sendingSnapshot.CreatedAt
                );
                _context.RatchetSnapshots.Add(sending);

                var receivingSnapshot = vaultData.Receiving.Snapshots.FirstOrDefault();
                var receiving = new RatchetSnapshot(
                    id: receivingSnapshot.Id,
                    userId: userId,
                    conversationId: conversationId,
                    type: RatchetType.Receiving,
                    rotationIndex: receivingSnapshot.RotationIndex,
                    encryptedSessionKey: receivingSnapshot.EncryptedSessionKey,
                    createdAt: receivingSnapshot.CreatedAt
                );
                _context.RatchetSnapshots.Add(receiving);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateVaultAsync failed for ConversationId={ConversationId}, UserId={UserId}",
                    conversationId, userId);
                await transaction.RollbackAsync();
                throw;
            }
        }

    }
}