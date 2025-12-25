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

        public virtual ICollection<ConversationUser> ConversationUsers { get; set; } = new List<ConversationUser>();

        protected Conversation() { }

        public Conversation(Guid id, DateTime createdAt, short version, ICollection<ConversationUser> conversationUsers)
        {
            Id = id;
            CreatedAt = createdAt;
            Version = version;
            if (conversationUsers != null)
            {
                ConversationUsers = conversationUsers;
            }
        }

        public Conversation(bool initialize = true)
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
            Version = AppVersion.Current;
            LastMessageAt = DateTime.UtcNow;
            ConversationUsers = new List<ConversationUser>();
        }
    }
}