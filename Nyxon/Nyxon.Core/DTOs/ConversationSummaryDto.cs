using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Core.DTOs
{
    public class ConversationSummaryDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public DateTime LastMessageAt { get; set; }
        public bool HasUnreadMessages { get; set; }
    }
}