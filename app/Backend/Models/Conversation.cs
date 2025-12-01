using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shared.Version;

/*id : uuid pk ai
users : {user1_id, user2_id, …}
created_at : datetime
version : small int*/

namespace Backend.Models
{
    public class Conversation
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public short Version { get; set; }
        
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
            ConversationUsers = new List<ConversationUser>();
        }
    }
}