using Nyxon.Core.Version;

namespace Nyxon.Server.Models
{
    [Index(nameof(LastMessageAt))]
    public class Conversation
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public short Version { get; set; }
        public DateTime LastMessageAt { get; set; }

        public Guid User1Id { get; set; }
        public Guid User2Id { get; set; }

        public int LastSequenceNumber { get; set; }

        public virtual ICollection<ConversationUser> ConversationUsers { get; set; } = new List<ConversationUser>();

        protected Conversation() { }

        public Conversation(Guid id, DateTime createdAt, short version, ICollection<ConversationUser> conversationUsers, int lastSequenceNumber)
        {
            Id = id;
            CreatedAt = createdAt;
            Version = version;
            if (conversationUsers != null)
            {
                ConversationUsers = conversationUsers;
            }
            LastSequenceNumber = lastSequenceNumber;
        }

        public Conversation(Guid id, Guid user1Id, Guid user2Id)
        {
            Id = id;
            CreatedAt = DateTime.UtcNow;
            Version = AppVersion.Current;
            LastMessageAt = DateTime.UtcNow;
            ConversationUsers = new List<ConversationUser>();
            User1Id = user1Id;
            User2Id = user2Id;
            LastSequenceNumber = 0;
        }
    }
}