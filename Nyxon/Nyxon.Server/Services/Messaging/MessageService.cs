using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nyxon.Core.DTOs;
using Nyxon.Core.Version;
using Nyxon.Server.Data;
using Nyxon.Server.Interfaces;
using Nyxon.Server.Services.Cache;
using Org.BouncyCastle.Ocsp;

namespace Nyxon.Server.Services.Messaging
{
    public class MessageService : IMessageService
    {
        private readonly AppDbContext _context;
        private readonly IMessageCacheService _messageCacheService;

        public MessageService(AppDbContext context, IMessageCacheService messageCacheService)
        {
            _context = context;
            _messageCacheService = messageCacheService;
        }
        public async Task<SendMessageResponse> SendMessageAsync(Guid senderId, SendMessageRequest request)
        {
            var sender = await _context.Users
                .Where(u => u.Id == senderId)
                .FirstOrDefaultAsync();

            var conversation = await _context.Conversations
                .Where(c => c.Id == request.ConversationId)
                .FirstOrDefaultAsync();

            int messageSequence = conversation.LastSequenceNumber + 1;

            var kvKey = KeyFactory.MessageKey(request.ConversationId, messageSequence);
            var now = DateTime.UtcNow;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var messageId = Guid.NewGuid();

                // save to valkey
                var valkeyMessage = new Nyxon.Server.Models.Valkey.Message(
                    id: messageId,
                    sequenceNumber: messageSequence,
                    senderId: senderId,
                    senderUsername: sender.Username,
                    sessionIndex: request.SessionIndex,
                    messageIndex: request.MessageIndex,
                    createdAt: now,
                    encryptedPayload: request.EncryptedPayload
                );

                await _messageCacheService.SaveMessageAsync(request.ConversationId, valkeyMessage);

                // store in postgres

                // message
                var messageMetadata = new MessageMetadata
                (
                    id: messageId,
                    conversationId: request.ConversationId,
                    kvKey: kvKey,
                    senderId: senderId,
                    rotationIndex: request.SessionIndex,
                    messageIndex: request.MessageIndex,
                    createdAt: now,
                    version: AppVersion.Current,
                    attachments: null
                );
                _context.MessageMetadata.Add(messageMetadata);
                await _context.SaveChangesAsync();

                // ratchet
                var convVault = await _context.ConversationVaults
                    .Where(v => v.UserId == senderId
                        && v.ConversationId == request.ConversationId)
                    .FirstOrDefaultAsync();

                if (convVault == null)
                    throw new InvalidOperationException("Conversation vault fetch failed.");

                ++convVault.SendCounter;

                if (request.EncryptedCurrentSessionKey == null)
                {
                    ++convVault.VaultData.Sending.Session.MessageIndex;
                }
                else
                {
                    ++convVault.VaultData.Sending.Session.RotationIndex;
                    convVault.VaultData.Sending.Session.MessageIndex = 1;
                    convVault.VaultData.Sending.Session.EncryptedCurrentSessionKey = request.EncryptedCurrentSessionKey;

                    // snapshot
                    if (request.Snapshot != null)
                    {
                        var snapshot = new RatchetSnapshot(
                            id: request.Snapshot.Id,
                            userId: senderId,
                            conversationId: request.ConversationId,
                            type: RatchetType.Sending,
                            rotationIndex: request.SessionIndex,
                            encryptedSessionKey: request.EncryptedCurrentSessionKey,
                            createdAt: request.Snapshot.CreatedAt
                        );
                        _context.RatchetSnapshots.Add(snapshot);
                    }
                }
                // update time
                convVault.UpdatedAt = now;

                // update last read
                var convsersationUser = await _context.ConversationUsers
                    .Where(c => c.UserId == senderId)
                    .FirstOrDefaultAsync();
                convsersationUser.LastRead = now;

                // advance index in conversation
                ++conversation.LastSequenceNumber;
                conversation.LastMessageAt = now;

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return new SendMessageResponse()
                {
                    Id = messageId,
                    MessageSequence = messageSequence,
                    CreatedAt = now
                };
            }
            catch
            {
                await transaction.RollbackAsync();

                // delete valkey message
                await _messageCacheService.DeleteMessageAsync(kvKey);

                throw;
            }
        }
        public async Task<List<MessageResponse>> GetRecentMessagesAsync(Guid conversationId)
        {
            var messages = await _messageCacheService.GetRecentMessagesAsync(conversationId);
            return messages
                .Select(m => new MessageResponse
                {
                    Id = m.Id,
                    SequenceNumber = m.SequenceNumber,
                    SenderId = m.SenderId,
                    SenderUsername = m.SenderUsername,
                    SessionIndex = m.SessionIndex,
                    MessageIndex = m.MessageIndex,
                    CreatedAt = m.CreatedAt,
                    EncryptedPayload = m.EncryptedPayload
                })
                .OrderBy(m => m.SequenceNumber)
                .ToList();
        }
        public async Task<MessageResponse?> GetMessageAsync(Guid conversationId, int sequenceNumber)
        {
            var message = await _messageCacheService.GetMessageAsync(conversationId, sequenceNumber);
            if (message == null)
                return null;

            return new MessageResponse
            {
                Id = message.Id,
                SequenceNumber = message.SequenceNumber,
                SenderId = message.SenderId,
                SessionIndex = message.SessionIndex,
                MessageIndex = message.MessageIndex,
                CreatedAt = message.CreatedAt,
                EncryptedPayload = message.EncryptedPayload
            };
        }

        public async Task DeleteMessageAsync(Guid messageId)
        {
            var message = await _context.MessageMetadata
                .Where(m => m.Id == messageId)
                .FirstOrDefaultAsync();

            if (message == null)
                throw new Exception("Message does not exist");

            await _messageCacheService.DeleteMessageAsync(message.KvKey);

            _context.Remove(message);
            await _context.SaveChangesAsync();
        }
    }
}