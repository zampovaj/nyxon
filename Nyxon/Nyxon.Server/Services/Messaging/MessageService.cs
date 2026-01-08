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
        private readonly ILogger<MessageService> _logger;

        public MessageService(AppDbContext context, IMessageCacheService messageCacheService, ILogger<MessageService> logger)
        {
            _context = context;
            _messageCacheService = messageCacheService;
            _logger = logger;
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
                    try
                    {
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
                    catch (Exception ex)
                    {
                        throw new Exception($"Couldn't add snapshot R:{request.Snapshot.RotationIndex}::Id:{request.Snapshot.Id}: {ex.Message}");
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

        public async Task<MessageReceivedStateUpdateResponse> ReadMessageUpdateAsync(Guid userId, MessageReceivedStateUpdateRequest request)
        {
            var user = await _context.Users
                .Where(u => u.Id == userId)
                .FirstOrDefaultAsync();

            var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var convVault = await _context.ConversationVaults
                    .Where(v => v.UserId == userId
                        && v.ConversationId == request.ConversationId)
                    .FirstOrDefaultAsync();

                if (convVault == null)
                    throw new InvalidOperationException("Conversation vault fetch failed.");

                if (convVault.RecvCounter > request.RecvCounter)
                    throw new InvalidOperationException($"Ratchet can't move backwards: {nameof(request.RecvCounter)}");

                // rotation happened
                if (request.EncryptedNewSessionKey != null && request.EncryptedNewSessionKey.Any())
                {
                    if (convVault.VaultData.Receiving.Session.RotationIndex > request.SessionIndex)
                        throw new InvalidOperationException($"Ratchet can't move backwards: {nameof(request.SessionIndex)}");

                    convVault.VaultData.Receiving.Session.EncryptedCurrentSessionKey = request.EncryptedNewSessionKey;

                    // snapshots
                    if (request.Snapshots != null && request.Snapshots.Any())
                    {
                        foreach (var snapshot in request.Snapshots)
                        {
                            try
                            {
                                await _context.RatchetSnapshots.AddAsync(
                                    new RatchetSnapshot(
                                        id: snapshot.Id,
                                        userId: userId,
                                        conversationId: request.ConversationId,
                                        RatchetType.Receiving,
                                        rotationIndex: snapshot.RotationIndex,
                                        encryptedSessionKey: snapshot.EncryptedSessionKey,
                                        createdAt: snapshot.CreatedAt
                                    )
                                );
                            }
                            catch (Exception ex)
                            {
                                _logger.LogInformation($"Couldn't add snapshot R:{snapshot.RotationIndex}::Id:{snapshot.Id}: {ex.Message}");
                            }
                        }
                        convVault.VaultData.Receiving.Session.RotationIndex = request.SessionIndex;

                    }
                }
                else
                {
                    if (convVault.VaultData.Receiving.Session.MessageIndex > request.MessageIndex)
                        throw new InvalidOperationException($"Ratchet can't move backwards: {nameof(request.MessageIndex)}");
                }

                // vault
                convVault.RecvCounter = request.RecvCounter;
                convVault.VaultData.Receiving.Session.MessageIndex = request.MessageIndex;
                await _context.SaveChangesAsync();


                // conversation users
                var conversationUser = await _context.ConversationUsers
                    .Where(cu => cu.ConversationId == request.ConversationId &&
                        cu.UserId == userId)
                    .FirstOrDefaultAsync();

                if (conversationUser == null)
                    throw new InvalidOperationException("User and conversation database join not found");

                var now = DateTime.UtcNow;
                conversationUser.LastRead = now;
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return new MessageReceivedStateUpdateResponse(now);
            }
            catch
            {
                await transaction.RollbackAsync();
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

        public async Task<MessageResponse?> GetMessageAsync(string kvKey)
        {
            try
            {
                var message = await _messageCacheService.GetMessageAsync(kvKey);
                if (message == null)
                    throw new KeyNotFoundException("Messages with this key ddoesn't exist");

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
            catch
            {
                throw;
            }
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