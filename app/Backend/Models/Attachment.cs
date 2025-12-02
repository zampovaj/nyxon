namespace Backend.Models
{
    public class Attachment
    {
        public Guid Id { get; set; }

        public Guid ConversationId { get; set; }
        public virtual Conversation Conversation { get; set; }

        public Guid MessageId { get; set; }
        public virtual MessageMetadata Message { get; set; }

        public Guid OwnerId { get; set; }
        public virtual User Owner { get; set; }
        
        public string StoragePtr { get; set; }
        public byte[] WrappedDek { get; set; }
        public DateTime CreatedAt { get; set; }
        public short Version { get; set; }

        public Attachment(Guid id, Guid conversationId, Guid messageId, Guid ownerId, string storagePtr, byte[] wrappedDek, DateTime createdAt, short version)
        {
            Id = id;
            ConversationId = conversationId;
            MessageId = messageId;
            OwnerId = ownerId;
            StoragePtr = storagePtr;
            WrappedDek = wrappedDek;
            CreatedAt = createdAt;
            Version = version;
        }

        /// <summary>
        /// Creates a brand new attachment
        /// </summary>
        public Attachment(Guid conversationId, Guid messageId, Guid ownerId, string storagePtr, byte[] wrappedDek)
        {
            Id = Guid.NewGuid();
            ConversationId = conversationId;
            MessageId = messageId;
            OwnerId = ownerId;
            StoragePtr = storagePtr;
            WrappedDek = wrappedDek;
            CreatedAt = DateTime.UtcNow;
            Version = AppVersion.Current;
        }
    }
}