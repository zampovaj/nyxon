using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Version;

/*id : uuid pk ai
users : {user1_id, user2_id, …}
created_at : datetime
version : small int*/

namespace Backend.Models
{
    public class Conversation
    {
        public Guid Id { get; set; }
        public ICollection<ConversationUser> Users { get; set; }
        public DateTime CreatedAt { get; set; }
        public short Version { get; set; }

        public Conversation(Guid id, ICollection<ConversationUser> users, DateTime createdAt, short version)
        {
            Id = id;
            Users = users;
            CreatedAt = createdAt;
            Version = version;
        }

        /// <summary>
        /// Creates a brand new conversation
        /// </summary>
        public Conversation(ICollection<ConversationUser> users)
        {
            Id = Guid.NewGuid();
            Users = users;
            CreatedAt = DateTime.UtcNow;
            Version = AppVersion.Current;
        }
    }
}