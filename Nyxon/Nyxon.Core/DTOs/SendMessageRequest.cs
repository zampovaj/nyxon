using System.Diagnostics.CodeAnalysis;
using Nyxon.Core.Models.Vaults;

namespace Nyxon.Core.DTOs
{
    public class SendMessageRequest
    {
        [Required]
        [NotNull]
        public Guid ConversationId { get; set; }
        [Required]
        [NotNull]
        [Range(0, int.MaxValue)]
        public int SessionIndex { get; set; }
        [Required]
        [NotNull]
        [Range(0, int.MaxValue)]
        public int MessageIndex { get; set; }
        public byte[] EncryptedPayload { get; set; } = Array.Empty<byte>();
        // ratchet
        public byte[]? EncryptedCurrentSessionKey { get; set; } = null;

        // snapshot
        public Snapshot? Snapshot { get; set; } = null;

        public SendMessageRequest(Guid conversationId, int sessionIndex, int messageIndex, byte[] encryptedPayload, byte[] encryptedCurrentSessionKey, Snapshot snapshot)
        {
            ConversationId = conversationId;
            SessionIndex = sessionIndex;
            MessageIndex = messageIndex;
            EncryptedPayload = encryptedPayload;
            EncryptedCurrentSessionKey = encryptedCurrentSessionKey;
            Snapshot = snapshot;
        }
        
        public SendMessageRequest(Guid conversationId, int sessionIndex, int messageIndex, byte[] encryptedPayload, byte[] encryptedCurrentSessionKey)
        {
            ConversationId = conversationId;
            SessionIndex = sessionIndex;
            MessageIndex = messageIndex;
            EncryptedPayload = encryptedPayload;
            EncryptedCurrentSessionKey = encryptedCurrentSessionKey;
        }
        
        public SendMessageRequest(Guid conversationId, int sessionIndex, int messageIndex, byte[] encryptedPayload)
        {
            ConversationId = conversationId;
            SessionIndex = sessionIndex;
            MessageIndex = messageIndex;
            EncryptedPayload = encryptedPayload;
        }

        public void AddSnapshot(Snapshot snapshot)
        {
            Snapshot = snapshot;
        }
        
        public void AddSnapshot(int rotationIndex, byte[] encryptedSessionKey, DateTime createdAt)
        {
            Snapshot = new Snapshot(rotationIndex, encryptedSessionKey, createdAt);
        }
    }
}