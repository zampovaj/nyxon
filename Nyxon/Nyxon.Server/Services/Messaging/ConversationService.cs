using Nyxon.Core.Version;
using Nyxon.Server.Data;
using Nyxon.Server.Interfaces;
using Org.BouncyCastle.Ocsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Nyxon.Server.Services.Messaging
{
    public class ConversationService : IConversationService
    {
        private readonly AppDbContext _context;
        private readonly IMessageCacheService _messageCacheService;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly ILogger<ConversationService> _logger;

        public ConversationService(AppDbContext context,
            IMessageCacheService messageCacheService,
            IHubContext<ChatHub> hubContext,
            ILogger<ConversationService> logger)
        {
            _context = context;
            _messageCacheService = messageCacheService;
            _hubContext = hubContext;
            _logger = logger;
        }
        public async Task<CreateConversationResponse> CreateConversationAsync(Guid initiatorId, CreateConversationRequest request)
        {
            if (request.TargetUserId == initiatorId)
                throw new Exception("Cannot create conversation with yourself");

            var targetUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == request.TargetUserId);

            if (targetUser == null)
                throw new Exception("User not found");

            if (targetUser.Username == AccountConstants.DeletedAccount)
                throw new InvalidOperationException("Cannot communicate with a deleted user");

            // sort ids
            var user1Id = initiatorId.CompareTo(request.TargetUserId) < 0 ? initiatorId : request.TargetUserId;
            var user2Id = initiatorId.CompareTo(request.TargetUserId) < 0 ? request.TargetUserId : initiatorId;


            // check for exisitng conversation and return it
            var existingId = await _context.Conversations
                .Where(c => c.User1Id == user1Id && c.User2Id == user2Id)
                .Select(c => (Guid?)c.Id)
                .FirstOrDefaultAsync();

            if (existingId != null)
            {
                return new CreateConversationResponse
                {
                    ConversationId = (Guid)existingId,
                    AlreadyExisted = true
                };
            }

            // create conversation
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                //conversation
                var conversation = new Conversation(request.ConversationId, user1Id, user2Id);

                _context.Conversations.Add(conversation);
                await _context.SaveChangesAsync();

                //conversation users
                var user1 = new ConversationUser(conversation.Id, initiatorId);
                var user2 = new ConversationUser(conversation.Id, targetUser.Id);

                _context.ConversationUsers.Add(user1);
                _context.ConversationUsers.Add(user2);
                await _context.SaveChangesAsync();

                // conversation vault
                var vault = new ConversationVault
                (
                    userId: initiatorId,
                    conversationId: conversation.Id,
                    vaultData: request.VaultData
                );
                _context.ConversationVaults.Add(vault);
                await _context.SaveChangesAsync();

                // snapshots

                var sendingSnapshot = request.VaultData.Sending.Snapshots.FirstOrDefault();
                var sending = new RatchetSnapshot(
                    id: sendingSnapshot.Id,
                    userId: initiatorId,
                    conversationId: conversation.Id,
                    type: RatchetType.Sending,
                    rotationIndex: sendingSnapshot.RotationIndex,
                    encryptedSessionKey: sendingSnapshot.EncryptedSessionKey,
                    createdAt: sendingSnapshot.CreatedAt
                );
                _context.RatchetSnapshots.Add(sending);

                var receivingSnapshot = request.VaultData.Receiving.Snapshots.FirstOrDefault();
                var receiving = new RatchetSnapshot(
                    id: receivingSnapshot.Id,
                    userId: initiatorId,
                    conversationId: conversation.Id,
                    type: RatchetType.Receiving,
                    rotationIndex: receivingSnapshot.RotationIndex,
                    encryptedSessionKey: receivingSnapshot.EncryptedSessionKey,
                    createdAt: receivingSnapshot.CreatedAt
                );
                _context.RatchetSnapshots.Add(receiving);

                await _context.SaveChangesAsync();

                var initiator = await _context.Users
                    .Where(u => u.Id == initiatorId)
                    .FirstOrDefaultAsync();

                // handshake

                var handshake = new Handshake(
                    conversationId: conversation.Id,
                    initiatorId: initiatorId,
                    targetUserId: targetUser.Id,
                    spkId: request.SpkId,
                    opkId: request.OpkId ?? null,
                    publicEphemeralKey: request.PublicEphemeralKey,
                    publicAgreementKey: initiator.PublicAgreementKey
                );
                _context.Handshakes.Add(handshake);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                // signalr
                List<Guid> userIds = new() { initiatorId, targetUser.Id };
                await NotifyClientsAsync(userIds);

                return new CreateConversationResponse()
                {
                    ConversationId = conversation.Id,
                    AlreadyExisted = false
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // i hate this
        // with passion

        // fuck signalr

        private async Task NotifyClientsAsync(List<Guid> userIds)
        {
            foreach (var userId in userIds)
            {
                _logger.LogInformation($"Notify: userId = {userId}");
                await _hubContext.Clients
                    .Group($"user:{userId}")
                    .SendAsync("NewConversationNotification");
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

        public async Task DeleteConversationAsync(Guid conversationId)
        {
            var conversation = await _context.Conversations
                .Where(c => c.Id == conversationId)
                .FirstOrDefaultAsync();

            if (conversation == null)
                throw new Exception("Conversatoin does not exist");

            var messageKeys = await _context.MessageMetadata
                .Where(m => m.ConversationId == conversationId)
                .Select(m => m.KvKey)
                .ToListAsync();

            _context.Remove(conversation);
            await _context.SaveChangesAsync();

            await _messageCacheService.DeleteBatchAsync(messageKeys);
        }

        public async Task UpdateReadConversationAsync(Guid userId, Guid conversationId)
        {
            var convUser = await _context.ConversationUsers
                .Where(cu => cu.UserId == userId &&
                    cu.ConversationId == conversationId)
                .FirstOrDefaultAsync();

            if (convUser == null)
                throw new InvalidOperationException("No link between this user and a conversation found.");

            convUser.LastRead = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
    }
}