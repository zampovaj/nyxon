using Nyxon.Core.Version;

namespace Nyxon.Server.Models
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
        public int SequenceNumber { get; set; }
        public byte[] EncryptedPayload { get; set; }
        public DateTime CreatedAt { get; set; }
        public short Version { get; set; }

        public virtual ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();

        protected MessageMetadata() { }

        public MessageMetadata(Guid id, Guid conversationId, string kvKey, Guid senderId, int rotationIndex, int messageIndex, byte[] encryptedPayload, int sequenceNumber, DateTime createdAt, short version, ICollection<Attachment> attachments)
        {
            Id = id;
            ConversationId = conversationId;
            KvKey = kvKey;
            SenderId = senderId;
            RotationIndex = rotationIndex;
            MessageIndex = messageIndex;
            SequenceNumber = sequenceNumber;
            EncryptedPayload = encryptedPayload;
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
        public MessageMetadata(Guid conversationId, string kvKey, Guid senderId, int rotationIndex, int messageIndex, int sequenceNumber, byte[] encryptedPayload, ICollection<Attachment> attachments)
        {
            Id = Guid.NewGuid();
            ConversationId = conversationId;
            KvKey = kvKey;
            SenderId = senderId;
            RotationIndex = rotationIndex;
            MessageIndex = messageIndex;
            SequenceNumber = sequenceNumber;
            EncryptedPayload = encryptedPayload;
            CreatedAt = DateTime.UtcNow;
            Version = AppVersion.Current;
            if (attachments != null)
            {
                Attachments = attachments;
            }
        }
    }
}