using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Server.Services.Messaging
{
    public class HandshakeService : IHandshakeService
    {
        private readonly AppDbContext _context;

        public HandshakeService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<HandshakeDto>> GetPendingHandshakesAsync(Guid userId)
        {
            await ClearHandshakesAsync();

            return await _context.Handshakes
                .AsNoTracking()
                .Where(h => h.TargetUserId == userId)
                .OrderByDescending(h => h.CreatedAt)
                .Select(h => new HandshakeDto()
                {
                    Id = h.Id,
                    ConversationId = h.ConversationId,
                    PublicEphemeralKey = h.PublicEphemeralKey,
                    PublicIdentityKey = h.PublicIdentityKey,
                    PrivateSpk = h.Spk.EncryptedKey,
                    PrivateOpk = h.Opk != null ? h.Opk.EncryptedKey : null
                })
                .ToListAsync();
        }

        public async Task<bool> DeleteHandshakeAsync(Guid handshakeId)
        {
            try
            {
                await _context.Handshakes
                    .Where(h => h.Id == handshakeId)
                    .ExecuteDeleteAsync();
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                await ClearHandshakesAsync();
            }
        }

        public async Task ClearHandshakesAsync()
        {
            var ids = await _context.Handshakes
                .Where(h => h.ExpiresAt <= DateTime.UtcNow)
                .Select(h => h.ConversationId)
                .ToListAsync();

            foreach (var id in ids)
            {
                await _context.Conversations
                    .Where(c => c.Id == id)
                    .ExecuteDeleteAsync();
            }
        }
    }
}