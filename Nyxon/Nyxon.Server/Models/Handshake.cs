using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// id : uuid
// initiator_id : uuid
// conversation_id : uuid
// version : short
// created_at : datetime
// expires_at : datetime
// spk : uuid
// opk : uuid nullable
// ek_public : bytes[]

namespace Nyxon.Server.Models
{
    public class Handshake
    {
        public Guid Id { get; set; }

        public Guid InitiatorId { get; set; }
        public virtual User Initiator { get; set; }

        public Guid ConversationId { get; set; }
        public virtual Conversation Conversation { get; set; }

        public short Version { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }

        public Guid SpkId { get; set; }
        public virtual SignedPrekey Spk { get; set; }

        public Guid? OpkId { get; set; }
        public virtual OneTimePrekey? Opk { get; set; }

        public byte[] PublicEphemeralKey { get; set; }

        protected Handshake() { }

        public Handshake(Guid conversationId, Guid initiatorId, Guid spkId, Guid? opkId, byte[] publicEphemeralKey)
        {
            Id = Guid.NewGuid();
            ConversationId = conversationId;
            InitiatorId = initiatorId;
            SpkId = spkId;
            OpkId = opkId;
            PublicEphemeralKey = publicEphemeralKey;

            Version = AppVersion.Current;
            CreatedAt = DateTime.UtcNow;
            ExpiresAt = DateTime.UtcNow.AddDays(14);
        }
    }
}