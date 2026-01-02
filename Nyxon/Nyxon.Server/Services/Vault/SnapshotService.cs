using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Server.Services.Vault
{
    public class SnapshotService : ISnapshotService
    {
        private readonly AppDbContext _context;

        public SnapshotService(AppDbContext context)
        {
            _context = context;
        }

        public async Task SaveNewAsync(Guid userId, CreateSnapshotDto snapshotDto)
        {
            var user = await _context.Users
                .Where(u => u.Id == userId)
                .FirstOrDefaultAsync();

            var conversation = await _context.Conversations
                .Where(c => c.Id == snapshotDto.ConversationId)
                .FirstOrDefaultAsync();

            if (user == null || conversation == null)
                throw new Exception("User or conversation does not exist");

            var snapshot = new RatchetSnapshot(
                userId: userId,
                conversationId: snapshotDto.ConversationId,
                type: snapshotDto.Type,
                rotationIndex: snapshotDto.RotationIndex,
                encryptedSessionKey: snapshotDto.EncryptedSessionKey,
                createdAt: snapshotDto.CreatedAt
            );
            _context.RatchetSnapshots.Add(snapshot);

            await _context.SaveChangesAsync();
        }

        public async Task<SnapshotsDto> GetSnapshotsAsync(Guid userId, Guid conversationId, List<Message> messages)
        {
            SnapshotsDto result = new SnapshotsDto();

            if (messages == null || !messages.Any())
                return result;

            var sendingIndices = messages
                .Where(m => m.SenderId == userId)
                .Select(m => m.SessionIndex)
                .Distinct()
                .ToList();

            var receivingIndices = messages
                .Where(m => m.SenderId != userId)
                .Select(m => m.SessionIndex)
                .Distinct()
                .ToList();

            var query = _context.RatchetSnapshots
                .Where(s => s.UserId == userId &&
                    s.ConversationId == conversationId);

            var sendingSnapshots = await GetSnapshotsHelperAsync(query, sendingIndices, RatchetType.Sending);
            var receivngSnapshots = await GetSnapshotsHelperAsync(query, receivingIndices, RatchetType.Receiving);

            result = new(sendingSnapshots, receivngSnapshots);

            return result;
        }

        private async Task<List<Snapshot>> GetSnapshotsHelperAsync(IQueryable<RatchetSnapshot>? query, List<int> indicies, RatchetType type)
        {
            if (indicies == null || !indicies.Any())
                return new List<Snapshot>();

            var min = indicies.Min();
            var max = indicies.Max();

            var minSnapshot = await query
                .Where(q => q.Type == type &&
                    q.RotationIndex <= min)
                .Select(q => new Snapshot()
                {
                    RotationIndex = q.RotationIndex,
                    EncryptedSessionKey = q.EncryptedSessionKey,
                    CreatedAt = q.CreatedAt
                })
                .OrderByDescending(q => q.RotationIndex)
                .FirstOrDefaultAsync();

            var interSnapshots = await query
                .Where(q => q.Type == type &&
                    q.RotationIndex > min &&
                    q.RotationIndex <= max)
                .Select(q => new Snapshot()
                {
                    RotationIndex = q.RotationIndex,
                    EncryptedSessionKey = q.EncryptedSessionKey,
                    CreatedAt = q.CreatedAt
                })
                .ToListAsync();

            var combined = new List<Snapshot>();

            if (minSnapshot != null) combined.Add(minSnapshot);
            if (interSnapshots != null) combined.AddRange(interSnapshots);

            return combined;
        }
    }
}