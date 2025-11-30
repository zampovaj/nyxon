using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Models
{
    public class ConversationUser
    {
        public Guid ConversationId { get; set; }
        public Conversation Conversation { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; }

        public ConversationUser(User user, Conversation conversation)
        {
            User = user;
            UserId = user.Id;
            Conversation = conversation;
            ConversationId = conversation.Id;
        }
    }
}