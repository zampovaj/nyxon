using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Client.Models
{
    public class Conversation
    {
        public Guid ConversationId { get; set; }
        public string TargetUsername { get; set; } = "";
        public string? LastMessagePreview { get; set; } = null;
        public DateTime LastMessageAt { get; set; }
        public bool HasUnreadMessages { get; set; }
        public string Initials => TargetUsername.Length > 0 ? TargetUsername[0].ToString().ToUpper() : "?";
    }
}