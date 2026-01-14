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
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Nyxon.Server.Services.Messaging
{
    public class MessageService : IMessageService
    {
        private readonly AppDbContext _context;
        private readonly IMessageCacheService _messageCacheService;
        private readonly ILogger<MessageService> _logger;
        private readonly IHubContext<ChatHub> _hubContext;

        public MessageService(AppDbContext context,
            IMessageCacheService messageCacheService,
            ILogger<MessageService> logger,
            IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _messageCacheService = messageCacheService;
            _logger = logger;
            _hubContext = hubContext;
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
                    sequenceNumber: messageSequence,
                    encryptedPayload: request.EncryptedPayload,
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

                if (request.EncryptedCurrentSessionKey == null || !request.EncryptedCurrentSessionKey.Any())
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

                if (convsersationUser == null)
                    throw new UnauthorizedAccessException("User is not part of this conversation");

                convsersationUser.LastRead = now;

                // advance index in conversation
                ++conversation.LastSequenceNumber;
                conversation.LastMessageAt = now;

                // jsonb thing, need to mark it as modified
                _context.Entry(convVault).Property(v => v.VaultData).IsModified = true;

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                // signalr
                await NotifyClientsAsync(kvKey, request.ConversationId, senderId);

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
                await _messageCacheService.DeleteMessageAsync(kvKey, request.ConversationId);

                throw;
            }
        }
        private async Task NotifyClientsAsync(string kvKey, Guid conversationId, Guid userId)
        {
            if (ChatHub.TryGetConnection(userId.ToString(), out var senderConnectionId))
            {
                await _hubContext.Clients
                    .GroupExcept(conversationId.ToString(), senderConnectionId)
                    .SendAsync("ReceiveMessageNotification", kvKey);
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
                    convVault.VaultData.Receiving.Session.RotationIndex = request.SessionIndex;

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

                // conversation users
                var conversationUser = await _context.ConversationUsers
                    .Where(cu => cu.ConversationId == request.ConversationId &&
                        cu.UserId == userId)
                    .FirstOrDefaultAsync();

                if (conversationUser == null)
                    throw new UnauthorizedAccessException("User is not part of this conversation");

                var now = DateTime.UtcNow;
                conversationUser.LastRead = now;

                // jsonb thing, need to mark it as modified
                _context.Entry(convVault).Property(v => v.VaultData).IsModified = true;

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

        public async Task<List<MessageResponse>> GetRecentMessagesAsync(Guid userId, Guid conversationId)
        {
            try
            {
                List<Message> messages;
                messages = await _messageCacheService.GetRecentMessagesAsync(conversationId);

                if (!messages.Any()) return new List<MessageResponse>();

                return messages
                    .Select(m => MapMessage(m))
                    .OrderBy(m => m.SequenceNumber)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Loading recent messages failed: {ex.Message}");
                throw;
            }
        }

        public async Task<List<MessageResponse>> GetMessagesBundleAsync(Guid userId, Guid conversationId, int count, int lastSequenceNumber)
        {
            try
            {
                var conversation = await _context.Conversations
                    .Where(c => c.Id == conversationId &&
                        c.ConversationUsers.Any(cu => cu.UserId == userId))
                    .FirstOrDefaultAsync();

                if (conversation == null)
                    throw new InvalidOperationException("Combination of conversation and user not found");

                if (conversation.LastSequenceNumber < lastSequenceNumber)
                    throw new InvalidOperationException("Last sequence doesn't match database value");

                var messages = await _messageCacheService.GetMessagesBundleAsync(conversationId, lastSequenceNumber, count);

                if (messages.Count == count)
                {
                    return messages
                        .Select(m => MapMessage(m))
                        .ToList();
                }

                //50, 101
                //45 -> 100 - 56
                //minseq = 56
                //countleft = 5
                //floor = 51
                //where(m < 56 && m >= 51)

                //minseq = 101
                //countleft = 50
                //floor = 51
                //where(m < 101 && m >= 51)

                int minSequence = lastSequenceNumber + 1;
                if (messages.Count > 0) minSequence = messages
                    .Min(m => m.SequenceNumber);
                int countLeft = count - messages.Count;
                int floor = minSequence - countLeft;
                if (floor < 1) floor = 1;

                var dbMessages = await _context.MessageMetadata
                    .Where(m => m.ConversationId == conversationId &&
                        m.SequenceNumber < minSequence &&
                        m.SequenceNumber >= floor)
                    .AsNoTracking() // less memory
                    .ToListAsync();

                List<MessageResponse> result = new();
                result.AddRange(dbMessages
                        .Select(m => MapMessage(m)));
                result.AddRange(messages
                        .Select(m => MapMessage(m)));

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Loading messages bundle failed: {ex.Message}");
                throw;
            }
        }

        public async Task<MessageResponse?> GetMessageAsync(Guid userId, Guid conversationId, int sequenceNumber)
        {
            try
            {
                var conversationUser = await _context.ConversationUsers
                    .Where(c => c.UserId == userId &&
                        c.ConversationId == conversationId)
                    .FirstOrDefaultAsync();
                if (conversationUser == null)
                    throw new UnauthorizedAccessException("User must be part of this conversation to get a message");

                var key = KeyFactory.MessageKey(conversationId, sequenceNumber);
                var message = await _messageCacheService.GetMessageAsync(key);

                // if not in valkey -> try postgres
                if (message == null)
                {
                    return await GetMessageFromDbAsync(key);
                }

                return MapMessage(message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Fetching message: {conversationId}:{sequenceNumber} failed: {ex.Message}");
                throw;
            }
        }

        public async Task<MessageResponse?> GetMessageAsync(Guid userId, string kvKey)
        {
            try
            {
                var message = await _messageCacheService.GetMessageAsync(kvKey);

                // if not in valkey -> try postgres
                if (message == null)
                {
                    return await GetMessageFromDbAsync(kvKey);
                }

                // TODO: add check for unauthorized access (user not form this conversation)

                return MapMessage(message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Fetching message: {kvKey} failed: {ex.Message}");
                throw;
            }
        }

        private async Task<MessageResponse?> GetMessageFromDbAsync(string kvKey)
        {
            try
            {
                var message = await _context.MessageMetadata
                    .Include(m => m.Sender)
                    .Where(m => m.KvKey == kvKey)
                    .FirstOrDefaultAsync();

                if (message == null) return null;

                return MapMessage(message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Fetching message: {kvKey} failed: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteMessageAsync(Guid userId, Guid messageId)
        {
            var message = await _context.MessageMetadata
                .Where(m => m.Id == messageId)
                .FirstOrDefaultAsync();

            if (message == null)
                throw new Exception("Message does not exist");

            var conversationUser = await _context.ConversationUsers
                .Where(c => c.UserId == userId &&
                    c.ConversationId == message.ConversationId)
                .FirstOrDefaultAsync();
            if (conversationUser == null)
                throw new UnauthorizedAccessException("User must be part of this conversation to deleet a message");

            await _messageCacheService.DeleteMessageAsync(message.KvKey, message.ConversationId);

            _context.Remove(message);
            await _context.SaveChangesAsync();
        }

        private MessageResponse MapMessage(Message message)
        {
            return new MessageResponse
            {
                Id = message.Id,
                SequenceNumber = message.SequenceNumber,
                SenderId = message.SenderId,
                SenderUsername = message.SenderUsername,
                SessionIndex = message.SessionIndex,
                MessageIndex = message.MessageIndex,
                CreatedAt = message.CreatedAt,
                EncryptedPayload = message.EncryptedPayload
            };
        }

        private MessageResponse MapMessage(MessageMetadata message)
        {
            return new MessageResponse
            {
                Id = message.Id,
                SequenceNumber = message.SequenceNumber,
                SenderId = message.SenderId,
                SenderUsername = message.Sender.Username,
                SessionIndex = message.RotationIndex,
                MessageIndex = message.MessageIndex,
                CreatedAt = message.CreatedAt,
                EncryptedPayload = message.EncryptedPayload
            };
        }
    }
}