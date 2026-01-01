using Nyxon.Core.Version;
using Nyxon.Server.Data;
using Nyxon.Server.Interfaces;
using Org.BouncyCastle.Ocsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Server.Services.Messaging
{
    public class ConversationService : IConversationService
    {
        private readonly AppDbContext _context;

        public ConversationService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<CreateConversationResponse> CreateConversationAsync(Guid initiatorId, CreateConversationRequest request)
        {
            if (request.TargetUserId == initiatorId)
                throw new Exception("Cannot create conversation with yourself");

            var targetUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == request.TargetUserId);

            if (targetUser == null)
                throw new Exception("User not found");

            // sort ids
            var user1Id = initiatorId.CompareTo(request.TargetUserId) < 0 ? initiatorId : request.TargetUserId;
            var user2Id = initiatorId.CompareTo(request.TargetUserId) < 0 ? request.TargetUserId : initiatorId;

            // create conversation
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var conversation = new Conversation(request.ConversationId, user1Id, user2Id);

                _context.Conversations.Add(conversation);
                await _context.SaveChangesAsync();

                var user1 = new ConversationUser(conversation.Id, initiatorId);
                var user2 = new ConversationUser(conversation.Id, targetUser.Id);

                _context.ConversationUsers.Add(user1);
                _context.ConversationUsers.Add(user2);
                await _context.SaveChangesAsync();

                // user vault
                var vault = new ConversationVault
                (
                    userId: initiatorId,
                    conversationId: conversation.Id,
                    vaultData: request.VaultData
                );
                _context.ConversationVaults.Add(vault);
                await _context.SaveChangesAsync();

                // handshake
                var handshake = new Handshake(
                    conversationId: conversation.Id,
                    initiatorId: initiatorId,
                    targetUserId: targetUser.Id, 
                    spkId: request.SpkPublicId,
                    opkId: request.OpkPublicId ?? null,
                    publicEphemeralKey: request.PublicEphemeralKey,
                    publicIdentityKey: targetUser.PublicKey
                );
                _context.Handshakes.Add(handshake);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return new CreateConversationResponse()
                {
                    ConversationId = conversation.Id,
                    AlreadyExisted = false
                };
            }
            catch (DbUpdateException)
            {
                // postgres blocked the request because for these users a conversation already exists
                await transaction.RollbackAsync();

                // find it using index and return it
                var existingId = await _context.Conversations
                    .Where(c => c.User1Id == user1Id && c.User2Id == user2Id)
                    .Select(c => c.Id)
                    .FirstAsync();

                return new CreateConversationResponse
                {
                    ConversationId = existingId,
                    AlreadyExisted = true
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<ConversationSummaryDto>> GetInboxAsync(Guid userId)
        {
            return await _context.ConversationUsers
                .AsNoTracking()
                .Where(cu => cu.UserId == userId)
                .OrderByDescending(cu => cu.Conversation.LastMessageAt) // using index
                .Select(cu => new ConversationSummaryDto
                {
                    Id = cu.ConversationId,
                    LastMessageAt = cu.Conversation.LastMessageAt,

                    // unread if the chat was updated and the user hasnt seen it yet
                    HasUnreadMessages = cu.Conversation.LastMessageAt > cu.LastRead,

                    // find the other user
                    Username = cu.Conversation.ConversationUsers
                        .Where(other => other.UserId != userId)
                        .Select(other => other.User.Username)
                        .FirstOrDefault() ?? "Unknown"
                })
                .ToListAsync();
        }
    }
}