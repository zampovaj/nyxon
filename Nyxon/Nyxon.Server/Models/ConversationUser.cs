namespace Nyxon.Server.Models
{
    public class ConversationUser
    {
        public Guid ConversationId { get; set; }
        public Conversation Conversation { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; }

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public bool IsAdmin { get; set; }

        // Empty constructor for EF Core
        protected ConversationUser() { }

        public ConversationUser(Guid conversationId, Guid userId, bool isAdmin = false, DateTime? joinedAt = null)
        {
            ConversationId = conversationId;
            UserId = userId;
            IsAdmin = isAdmin;
            JoinedAt = joinedAt ?? DateTime.UtcNow;
        }
    }
}