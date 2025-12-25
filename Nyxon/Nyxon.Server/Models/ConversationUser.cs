namespace Nyxon.Server.Models
{
    public class ConversationUser
    {
        public Guid ConversationId { get; set; }
        public Conversation Conversation { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; }

        public DateTime JoinedAt { get; set; }
        public bool IsAdmin { get; set; }
        public DateTime LastRead { get; set; }
        public short Version { get; set; }

        // Empty constructor for EF Core
        protected ConversationUser() { }

        public ConversationUser(Guid conversationId, Guid userId, bool isAdmin, DateTime joinedAt, DateTime lastRead, short version)
        {
            ConversationId = conversationId;
            UserId = userId;
            IsAdmin = isAdmin;
            JoinedAt = joinedAt;
            LastRead = lastRead;
            Version = version;
        }

        public ConversationUser(Guid conversationId, Guid userId)
        {
            ConversationId = conversationId;
            UserId = userId;
            IsAdmin = false;
            JoinedAt = DateTime.UtcNow;
            LastRead = DateTime.UtcNow;
            Version = AppVersion.Current;
        }
    }
}