using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nyxon.Core.DTOs;
using Nyxon.Core.Version;
using Nyxon.Server.Data;
using Nyxon.Server.Interfaces;
using Nyxon.Server.Services.Cache;

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
        public async Task<Guid> SendMessageAsync(Guid senderId, string senderUsername, SendMessageRequest request)
        {
            var messageId = Guid.NewGuid();
            var now = DateTime.UtcNow;

            // save to valkey
            var valkeyMessage = new Nyxon.Server.Models.Valkey.Message(
                id: messageId,
                sequenceNumber: request.MessageSequence,
                senderUsername: senderUsername,
                sessionIndex: request.SessionIndex,
                messageIndex: request.MessageIndex,
                createdAt: now,
                encryptedPayload: request.EncryptedPayload
            );

            await _messageCacheService.SaveMessageAsync(request.ConversationId, valkeyMessage);
            var kvKey = KeyFactory.MessageKey(request.ConversationId, request.MessageSequence);

            // store in db
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

            return messageId;
        }
        public async Task<List<MessageResponse>> GetRecentMessagesAsync(Guid conversationId)
        {
            var messages = await _messageCacheService.GetRecentMessagesAsync(conversationId);
            return messages.Select(m => new MessageResponse
            {
                Id = m.Id,
                SequenceNumber = m.SequenceNumber,
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
                SenderUsername = message.SenderUsername,
                SessionIndex = message.SessionIndex,
                MessageIndex = message.MessageIndex,
                CreatedAt = message.CreatedAt,
                EncryptedPayload = message.EncryptedPayload
            };
        }
    }
}