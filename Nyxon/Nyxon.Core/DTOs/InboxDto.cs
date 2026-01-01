using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Core.DTOs
{
    public class InboxDto
    {
        [Required]
        public List<ConversationSummaryDto>? Conversations { get; set; }
        [Required]
        public List<HandshakeDto>? Handshakes { get; set; }

        public InboxDto(List<ConversationSummaryDto>? conversations, List<HandshakeDto>? handshakes)
        {
            Conversations = conversations;
            Handshakes = handshakes;
        }
    }
}