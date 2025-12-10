using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Client.Models
{
    public class Conversation
    {
        public Guid ConversationId { get; set; }
        public string TargetUsername { get; set; } = string.Empty;

        public List<ChatMessage> Messages { get; set; } = new();

        // metadata
        public string LastMessagePreview => Messages.LastOrDefault()?.Content ?? "No messages yet";
        public DateTime LastActivity => Messages.LastOrDefault()?.SentAt ?? DateTime.MinValue;
        public int UnreadCount { get; set; } = 0;
    }
}