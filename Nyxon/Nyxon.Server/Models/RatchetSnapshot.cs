using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nyxon.Core.DTOs;

namespace Nyxon.Server.Models
{
    public class RatchetSnapshot
    {

        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public virtual User User { get; set; }

        public Guid ConversationId { get; set; }
        public virtual Conversation Conversation { get; set; }

        public RatchetType Type { get; set; }
        public int RotationIndex { get; set; }
        public byte[] EncryptedSessionKey { get; set; }
        public DateTime CreatedAt { get; set; }

        protected RatchetSnapshot() { }

        public RatchetSnapshot(Guid id, Guid userId, Guid conversationId, RatchetType type, int rotationIndex, byte[] encryptedSessionKey, DateTime createdAt)
        {
            Id = id;
            UserId = userId;
            ConversationId = conversationId;
            Type = type;
            RotationIndex = rotationIndex;
            EncryptedSessionKey = encryptedSessionKey;
            CreatedAt = createdAt;
        }
    }
}