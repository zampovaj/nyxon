using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Client.Models
{
    public class ActiveConversation
    {
        public Guid ConversationId { get; set; }
        public string TargetUsername { get; set; } = "";
        public List<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }
}