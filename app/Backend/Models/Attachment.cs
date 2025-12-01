using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shared.Version;

/*file_id : uuid pk ai
conversation_id : conversations_metadata[conversation_id]
message_id : messages_metadata[message_id]
owner_id : users[user_id]
storage_ptr : string // pointer to file address
wrapped_dek : key // encrypted with vault key
created_at : datetime
version : small int*/

namespace Backend.Models
{
    public class Attachment
    {
        public Guid Id { get; set; }
        public Guid ConversationId { get; set; }
        public Guid MessageId { get; set; }
        public Guid OwnerId { get; set; }
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