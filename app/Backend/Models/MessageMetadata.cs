using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shared.Version;

/*id : uuid pk ai
conversation_id : uuid references conversations
kv_key : string (messages:{conversation_id}:{message_sequence})
sender_id : uuid references users
rotation_index : int
kd_index : int
created_at : datetime
version : small int
attachments : {file_id,…}*/

namespace Backend.Models
{
    public class MessageMetadata
    {
        public Guid Id { get; set; }

        public Guid ConversationId { get; set; }
        public virtual Conversation Conversation { get; set; }

        public string KvKey { get; set; }

        public Guid SenderId { get; set; }
        public virtual User Sender { get; set; }

        public int RotationIndex { get; set; }
        public int MessageIndex { get; set; }
        public DateTime CreatedAt { get; set; }
        public short Version { get; set; }

        public virtual ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();

        protected MessageMetadata() { }

        public MessageMetadata(Guid id, Guid conversationId, string kvKey, Guid senderId, int rotationIndex, int messageIndex, DateTime createdAt, short version, ICollection<Attachment> attachments)
        {
            Id = id;
            ConversationId = conversationId;
            KvKey = kvKey;
            SenderId = senderId;
            RotationIndex = rotationIndex;
            MessageIndex = messageIndex;
            CreatedAt = createdAt;
            Version = version;
            if (attachments != null)
            {
                Attachments = attachments;
            }
        }

        /// <summary>
        /// Creates a brand new message metadata
        /// </summary>
        public MessageMetadata(Guid conversationId, string kvKey, Guid senderId, int rotationIndex, int messageIndex, ICollection<Attachment> attachments)
        {
            Id = Guid.NewGuid();
            ConversationId = conversationId;
            KvKey = kvKey;
            SenderId = senderId;
            RotationIndex = rotationIndex;
            MessageIndex = messageIndex;
            CreatedAt = DateTime.UtcNow;
            Version = AppVersion.Current;
            if (attachments != null)
            {
                Attachments = attachments;
            }
        }
    }
}