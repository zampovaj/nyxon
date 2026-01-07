using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Nyxon.Core.Models.Vaults;

namespace Nyxon.Core.DTOs
{
    public class MessageReceivedStateUpdateRequest
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
        [Required]
        [NotNull]
        [Range(0, int.MaxValue)]
        public int RecvCounter { get; set; }
        // ratchet
        public byte[]? EncryptedNewSessionKey { get; set; } = null;
        // snapshots
        public List<Snapshot>? Snapshots = new();

        public MessageReceivedStateUpdateRequest() { }

        public MessageReceivedStateUpdateRequest(Guid conversationId, int sessionIndex, int messageIndex, int recvCounter, byte[] encryptedCurrentSessionKey, List<Snapshot> snapshots)
        {
            ConversationId = conversationId;
            SessionIndex = sessionIndex;
            MessageIndex = messageIndex;
            RecvCounter = recvCounter;
            EncryptedNewSessionKey = encryptedCurrentSessionKey;
            Snapshots = snapshots;
        }

        public MessageReceivedStateUpdateRequest(Guid conversationId, int sessionIndex, int messageIndex, byte[] encryptedCurrentSessionKey)
        {
            ConversationId = conversationId;
            SessionIndex = sessionIndex;
            MessageIndex = messageIndex;
            EncryptedNewSessionKey = encryptedCurrentSessionKey;
        }

        public MessageReceivedStateUpdateRequest(Guid conversationId, int sessionIndex, int messageIndex)
        {
            ConversationId = conversationId;
            SessionIndex = sessionIndex;
            MessageIndex = messageIndex;
        }

        public void AddSnapshot(Snapshot snapshot)
        {
            Snapshots.Add(snapshot);
        }

        public void AddSnapshot(int rotationIndex, byte[] encryptedSessionKey, DateTime createdAt)
        {
            Snapshots.Add(new Snapshot(rotationIndex, encryptedSessionKey, createdAt));
        }
    }
}